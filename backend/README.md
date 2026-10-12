# Backend API

ASP.NET Core 9 minimal API with validated Entra JWT bearer authentication, required UUID `tenant_id` claim, EF Core persistence, and rate limiting.

## Local development

```bash
Entra__TenantId="094b4847-83d3-42be-b67d-c18dd6c0e29d" \
Entra__Audience="<registered-api-audience>" \
dotnet run --project src/BarberCloud.Api --urls http://localhost:5080
```

Development uses SQLite (`barbercloud.db`) and creates the local schema on startup. Production refuses to start unless configured for SQL Server. Swagger is enabled only in Development.

## Routes

| Method | Route | Description |
| --- | --- | --- |
| GET | `/health` | Public liveness probe |
| GET | `/v1/dashboard/summary` | Tenant dashboard totals |
| GET/POST | `/v1/customers` | List/create customers |
| GET/POST | `/v1/barbers` | List/create barbers |
| GET/POST | `/v1/services` | List/create services |
| GET | `/v1/appointments?from=<UTC>&to=<UTC>` | Tenant appointments, max 93 days |
| POST | `/v1/appointments` | Create appointment after tenant-scoped reference, duration and conflict validation |

All `/v1` routes require an Entra access token with a trusted UUID `tenant_id` claim. A subject claim (`oid` or `sub`) is required for audit attribution. No tenant ID is accepted in a request header or body. Unit/integration tests use a test-only authentication scheme that maps a test header into claims; it is not registered in Production.

## Persistence and tenant isolation

- EF global filters scope all domain reads; `SaveChanges` stamps added rows and rejects cross-tenant writes.
- SQL Server connections use managed identity, set `SESSION_CONTEXT('tenant_id')` on open, and use the RLS policy in `database/tenant-isolation.sql`.
- Mutation audit events are saved with the business transaction and deliberately exclude PII values.
- Before Production, run the SQL DDL/RLS script and managed identity bootstrap for every database, map the trusted identity claim to the SaaS tenant, and verify connection-pool/RLS behavior in Azure SQL. The SQL template has not been executed in an Azure subscription.
- Production uses Azure SQL. SQLite is for local development/tests only; SQL RLS is not provided by SQLite.
