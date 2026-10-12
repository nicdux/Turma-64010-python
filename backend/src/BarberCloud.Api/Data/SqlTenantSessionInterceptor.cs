using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BarberCloud.Api.Data;

public sealed class SqlTenantSessionInterceptor(ITenantContext tenantContext) : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var command = CreateTenantCommand(connection, tenantContext.TenantId);
        command?.ExecuteNonQuery();
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await using var command = CreateTenantCommand(connection, tenantContext.TenantId);
        if (command is not null)
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private static DbCommand? CreateTenantCommand(
        DbConnection connection,
        Guid tenantId)
    {
        if (connection is not SqlConnection)
        {
            return null;
        }

        var command = connection.CreateCommand();
        command.CommandText = "EXEC sys.sp_set_session_context @key=N'tenant_id', @value=@tenantId";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@tenantId";
        parameter.Value = tenantId;
        command.Parameters.Add(parameter);
        return command;
    }
}
