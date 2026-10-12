namespace BarberCloud.Api.Data;

public interface ITenantOwned
{
    Guid TenantId { get; set; }
}

public sealed class Customer : ITenantOwned
{
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = "";
    public string? PhoneE164 { get; set; }
    public string? Email { get; set; }
    public DateTime? ConsentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Barber : ITenantOwned
{
    public Guid TenantId { get; set; }
    public Guid BarberId { get; set; } = Guid.NewGuid();
    public string DisplayName { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public sealed class ServiceOffering : ITenantOwned
{
    public Guid TenantId { get; set; }
    public Guid ServiceId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public short DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class Appointment : ITenantOwned
{
    public Guid TenantId { get; set; }
    public Guid AppointmentId { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Guid BarberId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class AuditEvent : ITenantOwned
{
    public Guid TenantId { get; set; }
    public Guid AuditId { get; set; } = Guid.NewGuid();
    public string ActorId { get; set; } = "";
    public string Action { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

public enum AppointmentStatus
{
    Booked,
    Confirmed,
    Completed,
    Cancelled,
    NoShow
}
