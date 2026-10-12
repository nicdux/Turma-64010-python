using BarberCloud.Api.Services;
using Xunit;

namespace BarberCloud.Api.Tests;

public sealed class DashboardServiceTests
{
    [Fact]
    public void GetSummary_UsesRequestedTenantAndMarksSeedValuesAsDemo()
    {
        var tenantId = Guid.NewGuid();
        var service = new DemoDashboardReadService();

        var summary = service.GetSummary(tenantId);

        Assert.Equal(tenantId, summary.TenantId);
        Assert.Equal("demo", summary.DataStatus);
        Assert.True(summary.ActiveClients > 0);
        Assert.True(summary.AppointmentsToday > 0);
    }
}
