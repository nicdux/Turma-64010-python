using System.ComponentModel.DataAnnotations;
using BarberCloud.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BarberCloud.Api.Services;

public sealed class CustomerService(BarberCloudDbContext dbContext)
{
    public async Task<IReadOnlyList<CustomerView>> GetCustomersAsync(CancellationToken cancellationToken) =>
        await dbContext.Customers.AsNoTracking()
            .OrderBy(customer => customer.FullName)
            .Select(customer => new CustomerView(
                customer.CustomerId,
                customer.FullName,
                customer.PhoneE164,
                customer.Email,
                customer.ConsentAt))
            .ToListAsync(cancellationToken);

    public async Task<CustomerView> CreateCustomerAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            FullName = request.FullName.Trim(),
            PhoneE164 = request.PhoneE164,
            Email = request.Email,
            ConsentAt = request.MarketingConsent ? DateTime.UtcNow : null
        };

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CustomerView(
            customer.CustomerId,
            customer.FullName,
            customer.PhoneE164,
            customer.Email,
            customer.ConsentAt);
    }
}

public sealed record CreateCustomerRequest(
    string FullName,
    string? PhoneE164,
    string? Email,
    bool MarketingConsent);

public sealed record CustomerView(
    Guid CustomerId,
    string FullName,
    string? PhoneE164,
    string? Email,
    DateTime? ConsentAt);

public static class CustomerValidation
{
    public static string? Validate(CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length > 160)
        {
            return "Full name is required and must be at most 160 characters.";
        }

        if (request.PhoneE164 is { Length: > 0 } phone
            && (phone.Length > 16 || !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+[1-9]\d{7,14}$")))
        {
            return "Phone must use E.164 format.";
        }

        if (request.Email is { Length: > 0 } email
            && (email.Length > 254 || !new EmailAddressAttribute().IsValid(email)))
        {
            return "Email address is invalid.";
        }

        return null;
    }
}
