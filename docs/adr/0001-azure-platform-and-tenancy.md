# ADR 0001: Azure platform and initial tenant isolation

- Status: Accepted as a proposed implementation baseline
- Date: 2026-10-12

## Context

BARBERCLOUD is a Brazilian barbershop SaaS expected to grow toward 1,000 tenants. The first delivery must keep operating cost manageable while preserving a path to stronger tenant isolation. Identity, customer PII, availability, and unexpected cloud charges are material risks.

## Decision

- Use Bicep for Azure resources and GitHub Actions for CI. Infrastructure is resource-group scoped and explicitly deployed by a human-triggered workflow.
- Begin with a shared Azure SQL database, tenant-scoped rows, and a database RLS design. Preserve a path to database-per-tenant routing without claiming it has been built.
- Use Entra-issued identity tokens and a trusted application tenant claim. Never authorize based on user-controlled headers.
- Use system-assigned managed identity, Key Vault RBAC, TLS, disabled public data-plane access, and least-privilege role assignments in the infrastructure template.
- Use Entra External ID for future customer identity rather than starting new Azure AD B2C dependencies.
- Gate higher-cost Redis, Service Bus, Event Grid, OpenAI, Communication Services, and Power BI Embedded resources behind an explicit infrastructure parameter.

## Consequences

- The shared-database model is economical for an early SaaS but requires application-level tenant context, RLS, automated isolation tests, and incident-ready auditability before any real tenant data is stored.
- Private endpoints, VNet integration, and private DNS are required to make the private SQL, Key Vault, and Storage resources reachable from App Service. They are not included yet; the baseline template is not production deployable as-is.
- Entra External ID requires a separate customer identity setup and verified domain. Identity federation and tenant provisioning are not implemented in this repository.
- The gated services can incur recurring or consumption-based charges and need feature-specific privacy, consent, quota, and operational design.
- A subscription owner must approve cost, region, quotas, and resource creation before provisioning.

## Rejected alternatives

- Public SQL/Key Vault/Storage endpoints with broad firewall exceptions: rejected because they expand exposure and bypass the intended private access boundary.
- Database-per-tenant from day one: deferred because it multiplies provisioning and operational overhead before tenant count and isolation requirements are known.
- Inferring SaaS tenancy directly from Entra directory `tid`: rejected because a customer identity directory and an application tenant are distinct concepts.
