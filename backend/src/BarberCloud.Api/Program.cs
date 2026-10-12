using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using BarberCloud.Api.Services;
using BarberCloud.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Entra-issued access token with a tenant_id UUID claim."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var entraTenantId = builder.Configuration["Entra:TenantId"];
var audience = builder.Configuration["Entra:Audience"];
if ((string.IsNullOrWhiteSpace(entraTenantId) || string.IsNullOrWhiteSpace(audience))
    && !builder.Environment.IsEnvironment("Testing"))
{
    throw new InvalidOperationException("Configure Entra:TenantId and Entra:Audience before starting the API.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        if (!string.IsNullOrWhiteSpace(entraTenantId))
        {
            options.Authority = $"https://login.microsoftonline.com/{entraTenantId}/v2.0";
        }
        if (!string.IsNullOrWhiteSpace(audience))
        {
            options.Audience = audience;
        }
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            NameClaimType = "name",
            RoleClaimType = "roles"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TenantUser", policy =>
        policy.RequireAuthenticatedUser()
            .RequireAssertion(context =>
                Guid.TryParse(context.User.FindFirstValue("tenant_id"), out _)));
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000"];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, ClaimsTenantContext>();
builder.Services.AddScoped<SqlTenantSessionInterceptor>();
builder.Services.AddSingleton<SqlManagedIdentityInterceptor>();
builder.Services.AddDbContext<BarberCloudDbContext>((serviceProvider, options) =>
{
    var provider = builder.Configuration["Database:Provider"];
    var connectionString = builder.Configuration.GetConnectionString("BarberCloud");
    if (string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Configure ConnectionStrings:BarberCloud for SQL Server.");
        }

        options.UseSqlServer(connectionString);
        options.AddInterceptors(
            serviceProvider.GetRequiredService<SqlTenantSessionInterceptor>(),
            serviceProvider.GetRequiredService<SqlManagedIdentityInterceptor>());
        return;
    }

    if (builder.Environment.IsProduction())
    {
        throw new InvalidOperationException("Production must use SQL Server with row-level security enabled.");
    }

    options.UseSqlite(connectionString ?? "Data Source=barbercloud.db");
});
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<DashboardReadService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BarberCloudDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .AllowAnonymous();

var tenantApi = app.MapGroup("/v1")
    .RequireAuthorization("TenantUser")
    .RequireRateLimiting("api");

tenantApi.MapGet("/dashboard/summary", async (
    DashboardReadService dashboard,
    CancellationToken cancellationToken) =>
    Results.Ok(await dashboard.GetSummaryAsync(cancellationToken)))
    .WithName("GetDashboardSummary");

tenantApi.MapGet("/customers", async (
    CustomerService customers,
    CancellationToken cancellationToken) =>
    Results.Ok(await customers.GetCustomersAsync(cancellationToken)))
    .WithName("GetCustomers");

tenantApi.MapPost("/customers", async (
    CreateCustomerRequest request,
    CustomerService customers,
    CancellationToken cancellationToken) =>
{
    var validationError = CustomerValidation.Validate(request);
    if (validationError is not null)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["request"] = [validationError]
        });
    }

    var customer = await customers.CreateCustomerAsync(request, cancellationToken);
    return Results.Created($"/v1/customers/{customer.CustomerId}", customer);
})
    .WithName("CreateCustomer");

tenantApi.MapGet("/barbers", async (
    CatalogService catalog,
    CancellationToken cancellationToken) =>
    Results.Ok(await catalog.GetBarbersAsync(cancellationToken)))
    .WithName("GetBarbers");

tenantApi.MapPost("/barbers", async (
    CreateBarberRequest request,
    CatalogService catalog,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 120)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["displayName"] = ["Display name is required and must be at most 120 characters."]
        });
    }

    var barber = await catalog.CreateBarberAsync(request, cancellationToken);
    return Results.Created($"/v1/barbers/{barber.BarberId}", barber);
})
    .WithName("CreateBarber");

tenantApi.MapGet("/services", async (
    CatalogService catalog,
    CancellationToken cancellationToken) =>
    Results.Ok(await catalog.GetServicesAsync(cancellationToken)))
    .WithName("GetServices");

tenantApi.MapPost("/services", async (
    CreateServiceRequest request,
    CatalogService catalog,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Name)
        || request.Name.Trim().Length > 120
        || request.DurationMinutes is < 5 or > 480
        || request.Price < 0)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["request"] = ["A valid name, duration (5-480 minutes), and non-negative price are required."]
        });
    }

    var service = await catalog.CreateServiceAsync(request, cancellationToken);
    return Results.Created($"/v1/services/{service.ServiceId}", service);
})
    .WithName("CreateService");

tenantApi.MapGet("/appointments", async (
    DateTimeOffset? from,
    DateTimeOffset? to,
    AppointmentService appointments,
    CancellationToken cancellationToken) =>
{
    if (from is null || to is null || to <= from || to - from > TimeSpan.FromDays(93))
    {
        return Results.BadRequest(new { error = "Provide a valid UTC range no longer than 93 days." });
    }

    if (from.Value.Offset != TimeSpan.Zero || to.Value.Offset != TimeSpan.Zero)
    {
        return Results.BadRequest(new { error = "Appointment query times must use UTC." });
    }

    return Results.Ok(await appointments.GetAppointmentsAsync(
        from.Value.UtcDateTime,
        to.Value.UtcDateTime,
        cancellationToken));
})
    .WithName("GetAppointments");

tenantApi.MapPost("/appointments", async (
    CreateAppointmentRequest request,
    AppointmentService appointments,
    CancellationToken cancellationToken) =>
{
    var result = await appointments.CreateAppointmentAsync(request, cancellationToken);
    return result.Error is not null
        ? Results.BadRequest(new { error = result.Error })
        : Results.Created($"/v1/appointments/{result.Appointment!.AppointmentId}", result.Appointment);
})
    .WithName("CreateAppointment");

app.Run();

public partial class Program;
