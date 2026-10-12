using BarberCloud.Api.Data;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace BarberCloud.Api.Services;

public sealed class AppointmentService(BarberCloudDbContext dbContext)
{
    public async Task<IReadOnlyList<AppointmentView>> GetAppointmentsAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        return await dbContext.Appointments
            .AsNoTracking()
            .Where(item => item.StartsAt >= from && item.StartsAt < to)
            .OrderBy(item => item.StartsAt)
            .Select(item => new AppointmentView(
                item.AppointmentId,
                item.CustomerId,
                item.BarberId,
                item.ServiceId,
                new DateTimeOffset(item.StartsAt, TimeSpan.Zero),
                new DateTimeOffset(item.EndsAt, TimeSpan.Zero),
                item.Status.ToString()))
            .ToListAsync(cancellationToken);
    }

    public async Task<(AppointmentView? Appointment, string? Error)> CreateAppointmentAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.StartsAt.Offset != TimeSpan.Zero || request.EndsAt.Offset != TimeSpan.Zero)
        {
            return (null, "Appointment times must use UTC.");
        }

        if (request.EndsAt <= request.StartsAt)
        {
            return (null, "Appointment end must be after its start.");
        }

        var customerExists = await dbContext.Customers
            .AnyAsync(item => item.CustomerId == request.CustomerId, cancellationToken);
        var barberExists = await dbContext.Barbers
            .AnyAsync(item => item.BarberId == request.BarberId && item.IsActive, cancellationToken);
        var service = await dbContext.Services
            .Where(item => item.ServiceId == request.ServiceId && item.IsActive)
            .Select(item => new { item.DurationMinutes })
            .SingleOrDefaultAsync(cancellationToken);

        if (!customerExists || !barberExists || service is null)
        {
            return (null, "Customer, active barber, or active service was not found in this tenant.");
        }

        if (request.EndsAt - request.StartsAt != TimeSpan.FromMinutes(service.DurationMinutes))
        {
            return (null, "Appointment duration must match the selected service.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var hasConflict = await dbContext.Appointments.AnyAsync(item =>
            item.BarberId == request.BarberId
            && item.Status != AppointmentStatus.Cancelled
            && item.StartsAt < request.EndsAt.UtcDateTime
            && request.StartsAt.UtcDateTime < item.EndsAt,
            cancellationToken);
        if (hasConflict)
        {
            return (null, "The barber already has an appointment during this time.");
        }

        var appointment = new Appointment
        {
            CustomerId = request.CustomerId,
            BarberId = request.BarberId,
            ServiceId = request.ServiceId,
            StartsAt = request.StartsAt.UtcDateTime,
            EndsAt = request.EndsAt.UtcDateTime,
            Status = AppointmentStatus.Booked
        };
        dbContext.Appointments.Add(appointment);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (new AppointmentView(
            appointment.AppointmentId,
            appointment.CustomerId,
            appointment.BarberId,
            appointment.ServiceId,
            new DateTimeOffset(appointment.StartsAt, TimeSpan.Zero),
            new DateTimeOffset(appointment.EndsAt, TimeSpan.Zero),
            appointment.Status.ToString()), null);
    }
}

public sealed record CreateAppointmentRequest(
    Guid CustomerId,
    Guid BarberId,
    Guid ServiceId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);

public sealed record AppointmentView(
    Guid AppointmentId,
    Guid CustomerId,
    Guid BarberId,
    Guid ServiceId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Status);
