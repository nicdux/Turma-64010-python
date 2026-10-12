# BARBERCLOUD CRM

BARBERCLOUD is a multi-tenant CRM foundation for barbershops. This repository currently contains an interactive dark-mode dashboard with clearly labeled demo data, a .NET API starter with Entra JWT validation and tenant-claim enforcement, Azure Bicep infrastructure, and CI workflows.

> **Deployment status:** local foundation only. No Azure resources, production data, third-party messaging, or production domain have been configured or deployed. Dashboard figures are illustrative, not connected to a database.

## Repository layout

| Path | Purpose |
| --- | --- |
| `frontend/` | Next.js, React, TypeScript dashboard |
| `backend/` | ASP.NET Core API and tests |
| `infrastructure/bicep/` | Azure infrastructure templates |
| `database/` | Database design and tenant-isolation notes |
| `devops/` | Operational configuration |
| `docs/` | Architecture, runbooks, ADRs, and roadmap |
| `tests/e2e/` | End-to-end test configuration |
| `scripts/` | Local developer utilities |
| `.github/workflows/` | CI, security scanning, and gated Azure deployment |

## Run locally

Prerequisites: Node.js 20+, .NET 9 SDK (or later), and npm.

```bash
cd frontend
npm ci
npm run dev
```

The dashboard is available at `http://localhost:3000`. Its metrics are sample data.

```bash
cd backend
Entra__TenantId="<entra-directory-id>" \
Entra__Audience="<api-application-id-uri>" \
dotnet run --project src/BarberCloud.Api --urls http://localhost:5080
```

The API listens on `http://localhost:5080`; `/health` is public and `/v1/dashboard/summary` requires an Entra-issued bearer token carrying a UUID `tenant_id` claim. The values above must come from an Entra app registration configured to issue that trusted claim.

## Validate

```bash
cd frontend && npm ci && npm run lint && npm run build
cd backend && dotnet test
```

## Tenant security boundary

Production tenant identity is obtained from the validated token's `tenant_id` claim; callers cannot select a tenant with a request header. Configure `Entra__TenantId`, `Entra__Audience`, and a trusted identity application's claim transformation before exposing the API. The API rejects tokens without the tenant claim. Database row-level security, database-per-tenant routing, persistence, role authorization, and audit-event storage are not yet implemented; do not use this foundation with real customer data.

## Azure deployment

The Bicep entry point is `infrastructure/bicep/main.bicep`. It provisions a baseline web/API, SQL, storage, Key Vault, and monitoring footprint. Advanced paid services are explicitly gated and off by default. Review `docs/architecture.md` and the deployment runbook, obtain subscription/cost approval, and authenticate Azure CLI before deployment. The GitHub deployment workflow is manual and requires a configured Azure federated credential; it does not deploy on every push.

## Roadmap and known gaps

The first milestone is a secure, persistent vertical slice for customers, barbers, services, and appointments. Before production: implement schema and migrations, database RLS, complete Entra/External ID flows, authorization and audit trails, operational alerting, backups, accessibility and load/security tests, and third-party integrations. See `docs/architecture.md`.
