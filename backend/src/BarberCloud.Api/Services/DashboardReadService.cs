using BarberCloud.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BarberCloud.Api.Services;

public sealed class DashboardReadService(BarberCloudDbContext dbContext)
{
    public async Task<DashboardSummary> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonth = monthStart.AddMonths(1);

        var activeClients = await dbContext.Customers.CountAsync(cancellationToken);
        var appointmentsToday = await dbContext.Appointments.CountAsync(item =>
            item.StartsAt >= now.Date && item.StartsAt < now.Date.AddDays(1)
            && item.Status != AppointmentStatus.Cancelled, cancellationToken);
        var completedServicePrices = await dbContext.Appointments
            .Where(item => item.StartsAt >= monthStart && item.StartsAt < nextMonth
                && item.Status == AppointmentStatus.Completed)
            .Join(dbContext.Services,
                appointment => new { appointment.TenantId, appointment.ServiceId },
                service => new { service.TenantId, service.ServiceId },
                (_, service) => service.Price)
            .ToListAsync(cancellationToken);
        var revenue = completedServicePrices.Sum();

        return new DashboardSummary(activeClients, appointmentsToday, revenue);
    }
}

public sealed record DashboardSummary(int ActiveClients, int AppointmentsToday, decimal RevenueThisMonth);
