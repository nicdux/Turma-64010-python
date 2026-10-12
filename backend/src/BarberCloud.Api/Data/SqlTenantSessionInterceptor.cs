using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BarberCloud.Api.Data;

public sealed class SqlTenantSessionInterceptor(ITenantContext tenantContext) : DbConnectionInterceptor
{
    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await SetTenantSessionAsync(connection, tenantContext.TenantId, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private static async Task SetTenantSessionAsync(
        DbConnection connection,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!connection.GetType().FullName!.Contains("SqlConnection", StringComparison.Ordinal))
        {
            return;
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "EXEC sys.sp_set_session_context @key=N'tenant_id', @value=@tenantId";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@tenantId";
        parameter.Value = tenantId;
        command.Parameters.Add(parameter);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
