using Microsoft.EntityFrameworkCore;

namespace BarberCloud.Api.Data;

public sealed class BarberCloudDbContext(
    DbContextOptions<BarberCloudDbContext> options,
    ITenantContext tenantContext) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Barber> Barbers => Set<Barber>();
    public DbSet<ServiceOffering> Services => Set<ServiceOffering>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers", "core");
            entity.HasKey(item => new { item.TenantId, item.CustomerId });
            entity.Property(item => item.TenantId).HasColumnName("tenant_id");
            entity.Property(item => item.CustomerId).HasColumnName("customer_id");
            entity.Property(item => item.FullName).HasColumnName("full_name").HasMaxLength(160).IsRequired();
            entity.Property(item => item.PhoneE164).HasColumnName("phone_e164").HasMaxLength(16);
            entity.Property(item => item.Email).HasColumnName("email").HasMaxLength(254);
            entity.Property(item => item.ConsentAt).HasColumnName("consent_at");
            entity.Property(item => item.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(item => new { item.TenantId, item.PhoneE164 });
            entity.HasQueryFilter(item => item.TenantId == tenantContext.TenantId);
        });

        modelBuilder.Entity<Barber>(entity =>
        {
            entity.ToTable("Barbers", "core");
            entity.HasKey(item => new { item.TenantId, item.BarberId });
            entity.Property(item => item.TenantId).HasColumnName("tenant_id");
            entity.Property(item => item.BarberId).HasColumnName("barber_id");
            entity.Property(item => item.DisplayName).HasColumnName("display_name").HasMaxLength(120).IsRequired();
            entity.Property(item => item.IsActive).HasColumnName("is_active");
            entity.HasQueryFilter(item => item.TenantId == tenantContext.TenantId);
        });

        modelBuilder.Entity<ServiceOffering>(entity =>
        {
            entity.ToTable("Services", "core", table =>
            {
                table.HasCheckConstraint("CK_Services_duration", "duration_minutes BETWEEN 5 AND 480");
                table.HasCheckConstraint("CK_Services_price", "price >= 0");
            });
            entity.HasKey(item => new { item.TenantId, item.ServiceId });
            entity.Property(item => item.TenantId).HasColumnName("tenant_id");
            entity.Property(item => item.ServiceId).HasColumnName("service_id");
            entity.Property(item => item.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            entity.Property(item => item.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(item => item.Price).HasColumnName("price").HasPrecision(10, 2);
            entity.Property(item => item.IsActive).HasColumnName("is_active");
            entity.HasQueryFilter(item => item.TenantId == tenantContext.TenantId);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments", "core", table =>
            {
                table.HasCheckConstraint("CK_Appointments_time", "ends_at > starts_at");
                table.HasCheckConstraint(
                    "CK_Appointments_status",
                    "status IN ('booked', 'confirmed', 'completed', 'cancelled', 'no_show')");
            });
            entity.HasKey(item => new { item.TenantId, item.AppointmentId });
            entity.Property(item => item.TenantId).HasColumnName("tenant_id");
            entity.Property(item => item.AppointmentId).HasColumnName("appointment_id");
            entity.Property(item => item.CustomerId).HasColumnName("customer_id");
            entity.Property(item => item.BarberId).HasColumnName("barber_id");
            entity.Property(item => item.ServiceId).HasColumnName("service_id");
            entity.Property(item => item.StartsAt).HasColumnName("starts_at");
            entity.Property(item => item.EndsAt).HasColumnName("ends_at");
            entity.Property(item => item.Status).HasColumnName("status")
                .HasConversion(
                    status => ToDatabaseStatus(status),
                    status => FromDatabaseStatus(status))
                .HasMaxLength(20);
            entity.Property(item => item.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(item => new { item.TenantId, item.BarberId, item.StartsAt });
            entity.HasOne<Customer>().WithMany()
                .HasForeignKey(item => new { item.TenantId, item.CustomerId })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Barber>().WithMany()
                .HasForeignKey(item => new { item.TenantId, item.BarberId })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ServiceOffering>().WithMany()
                .HasForeignKey(item => new { item.TenantId, item.ServiceId })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(item => item.TenantId == tenantContext.TenantId);
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.ToTable("AuditEvents", "core");
            entity.HasKey(item => new { item.TenantId, item.AuditId });
            entity.Property(item => item.TenantId).HasColumnName("tenant_id");
            entity.Property(item => item.AuditId).HasColumnName("audit_id");
            entity.Property(item => item.ActorId).HasColumnName("actor_id").HasMaxLength(128).IsRequired();
            entity.Property(item => item.Action).HasColumnName("action").HasMaxLength(16).IsRequired();
            entity.Property(item => item.EntityType).HasColumnName("entity_type").HasMaxLength(64).IsRequired();
            entity.Property(item => item.EntityId).HasColumnName("entity_id").HasMaxLength(128).IsRequired();
            entity.Property(item => item.OccurredAt).HasColumnName("occurred_at");
            entity.HasIndex(item => new { item.TenantId, item.OccurredAt });
            entity.HasQueryFilter(item => item.TenantId == tenantContext.TenantId);
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnforceTenantOwnership();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnforceTenantOwnership();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnforceTenantOwnership()
    {
        var tenantId = tenantContext.TenantId;
        var entries = ChangeTracker.Entries<ITenantOwned>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.TenantId = tenantId;
            }
            else if (entry.Entity.TenantId != tenantId)
            {
                throw new InvalidOperationException("Cross-tenant writes are forbidden.");
            }
        }

        foreach (var entry in entries.Where(entry => entry.Entity is not AuditEvent))
        {
            var entityId = string.Join(
                ":",
                entry.Properties
                    .Where(property => property.Metadata.IsPrimaryKey() && property.Metadata.Name != nameof(ITenantOwned.TenantId))
                    .Select(property => property.CurrentValue?.ToString() ?? ""));

            AuditEvents.Add(new AuditEvent
            {
                TenantId = tenantId,
                ActorId = tenantContext.ActorId,
                Action = entry.State switch
                {
                    EntityState.Added => "create",
                    EntityState.Modified => "update",
                    EntityState.Deleted => "delete",
                    _ => throw new InvalidOperationException("Unexpected audited entity state.")
                },
                EntityType = entry.Metadata.ClrType.Name,
                EntityId = entityId
            });
        }
    }

    private static string ToDatabaseStatus(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Booked => "booked",
        AppointmentStatus.Confirmed => "confirmed",
        AppointmentStatus.Completed => "completed",
        AppointmentStatus.Cancelled => "cancelled",
        AppointmentStatus.NoShow => "no_show",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown appointment status.")
    };

    private static AppointmentStatus FromDatabaseStatus(string status) => status switch
    {
        "booked" => AppointmentStatus.Booked,
        "confirmed" => AppointmentStatus.Confirmed,
        "completed" => AppointmentStatus.Completed,
        "cancelled" => AppointmentStatus.Cancelled,
        "no_show" => AppointmentStatus.NoShow,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown database appointment status.")
    };
}
