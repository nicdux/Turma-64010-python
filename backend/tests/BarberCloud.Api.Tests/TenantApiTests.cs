using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using BarberCloud.Api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace BarberCloud.Api.Tests;

public sealed class TenantApiTests
{
    [Fact]
    public async Task ProtectedApi_RejectsRequestsWithoutAuthentication()
    {
        using var factory = new TestApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/v1/customers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Customers_AreIsolatedByAuthenticatedTenant()
    {
        using var factory = new TestApiFactory();
        await factory.InitializeDatabaseAsync();
        using var firstTenant = factory.CreateTenantClient(Guid.NewGuid());
        using var secondTenant = factory.CreateTenantClient(Guid.NewGuid());

        var create = await firstTenant.PostAsJsonAsync("/v1/customers", new
        {
            fullName = "Cliente A",
            phoneE164 = "+5511999999999",
            email = "cliente@example.com",
            marketingConsent = false
        });
        var firstTenantCustomers = await firstTenant.GetFromJsonAsync<CustomerResponse[]>("/v1/customers");
        var secondTenantCustomers = await secondTenant.GetFromJsonAsync<CustomerResponse[]>("/v1/customers");

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.NotNull(firstTenantCustomers);
        Assert.Single(firstTenantCustomers);
        Assert.Equal("Cliente A", firstTenantCustomers[0].FullName);
        Assert.NotNull(secondTenantCustomers);
        Assert.Empty(secondTenantCustomers);
    }

    [Fact]
    public async Task AppointmentFlow_ValidatesDurationAndPreventsBarberDoubleBooking()
    {
        using var factory = new TestApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateTenantClient(Guid.NewGuid());
        var customerResponse = await client.PostAsJsonAsync("/v1/customers", new
        {
            fullName = "Cliente",
            phoneE164 = (string?)null,
            email = (string?)null,
            marketingConsent = false
        });
        var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var barberResponse = await client.PostAsJsonAsync("/v1/barbers", new { displayName = "Lucas" });
        var barber = await barberResponse.Content.ReadFromJsonAsync<BarberResponse>();
        var serviceResponse = await client.PostAsJsonAsync("/v1/services", new
        {
            name = "Corte",
            durationMinutes = (short)30,
            price = 65m
        });
        var service = await serviceResponse.Content.ReadFromJsonAsync<ServiceResponse>();
        Assert.NotNull(customer);
        Assert.NotNull(barber);
        Assert.NotNull(service);

        var appointment = new
        {
            customerId = customer.CustomerId,
            barberId = barber.BarberId,
            serviceId = service.ServiceId,
            startsAt = "2030-05-01T12:00:00Z",
            endsAt = "2030-05-01T12:30:00Z"
        };
        var created = await client.PostAsJsonAsync("/v1/appointments", appointment);
        var conflict = await client.PostAsJsonAsync("/v1/appointments", appointment);
        var daySummary = await client.GetFromJsonAsync<DashboardSummaryResponse>("/v1/dashboard/summary");

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, conflict.StatusCode);
        Assert.NotNull(daySummary);
        Assert.Equal(1, daySummary.ActiveClients);
        Assert.Equal(0, daySummary.AppointmentsToday);
        Assert.Equal(0m, daySummary.RevenueThisMonth);
    }

    [Fact]
    public async Task InvalidCustomerData_IsRejected()
    {
        using var factory = new TestApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateTenantClient(Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/v1/customers", new
        {
            fullName = "Bad phone",
            phoneE164 = "555-1234",
            email = (string?)null,
            marketingConsent = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record CustomerResponse(Guid CustomerId, string FullName);
    private sealed record BarberResponse(Guid BarberId);
    private sealed record ServiceResponse(Guid ServiceId);
    private sealed record DashboardSummaryResponse(int ActiveClients, int AppointmentsToday, decimal RevenueThisMonth);
}

internal sealed class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public TestApiFactory()
    {
        _connection.Open();
    }

    public HttpClient CreateTenantClient(Guid tenantId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Tenant", tenantId.ToString());
        return client;
    }

    public async Task InitializeDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarberCloudDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<BarberCloudDbContext>();
            services.RemoveAll<DbContextOptions<BarberCloudDbContext>>();
            services.AddDbContext<BarberCloudDbContext>(options => options.UseSqlite(_connection));
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultForbidScheme = TestAuthenticationHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-Tenant", out var tenantHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!Guid.TryParse(tenantHeader, out var tenantId))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid test tenant."));
        }

        var identity = new ClaimsIdentity(
            [
                new Claim("tenant_id", tenantId.ToString()),
                new Claim("sub", "test-user")
            ],
            Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
