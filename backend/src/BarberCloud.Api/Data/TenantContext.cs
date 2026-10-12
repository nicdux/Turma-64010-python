using System.Security.Claims;

namespace BarberCloud.Api.Data;

public interface ITenantContext
{
    Guid TenantId { get; }
    string ActorId { get; }
}

public sealed class ClaimsTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public Guid TenantId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirstValue("tenant_id");
            return Guid.TryParse(claim, out var tenantId)
                ? tenantId
                : throw new InvalidOperationException("A validated tenant_id claim is required for data access.");
        }
    }

    public string ActorId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue("oid")
        ?? httpContextAccessor.HttpContext?.User.FindFirstValue("sub")
        ?? throw new InvalidOperationException("A validated subject claim is required for audit attribution.");
}
