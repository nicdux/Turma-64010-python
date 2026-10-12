using System.Data.Common;
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BarberCloud.Api.Data;

public sealed class SqlManagedIdentityInterceptor : DbConnectionInterceptor
{
    private static readonly TokenRequestContext SqlTokenRequest =
        new(["https://database.windows.net/.default"]);

    private readonly ManagedIdentityCredential _credential = new(new ManagedIdentityCredentialOptions());
    private readonly Func<SqlAuthenticationParameters, CancellationToken, Task<SqlAuthenticationToken>> _tokenCallback;

    public SqlManagedIdentityInterceptor()
    {
        _tokenCallback = async (_, cancellationToken) =>
        {
            var token = await _credential.GetTokenAsync(SqlTokenRequest, cancellationToken);
            return new SqlAuthenticationToken(token.Token, token.ExpiresOn);
        };
    }

    public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        if (connection is SqlConnection sqlConnection)
        {
            sqlConnection.AccessTokenCallback = _tokenCallback;
        }

        return base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
    }
}
