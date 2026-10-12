namespace BarberCloud.Api.Services;

public interface IDashboardReadService
{
    DashboardSummary GetSummary(Guid tenantId);
}

public sealed record DashboardSummary(
    Guid TenantId,
    string DataStatus,
    int ActiveClients,
    int AppointmentsToday,
    decimal RevenueThisMonth);

public sealed class DemoDashboardReadService : IDashboardReadService
{
    public DashboardSummary GetSummary(Guid tenantId) =>
        new(tenantId, "demo", 1248, 8, 18420m);
}
