# Functional scope and initial onboarding

## Current prototype

The dashboard demonstrates a dark-only barbershop overview: headline business metrics, revenue chart with area/bar switching, service mix donut, today's appointment list, team performance, and a CEO Copilot insight card. All visible dashboard values are illustrative and the controls are not yet connected to the API.

The API exposes a public liveness endpoint and authenticated tenant-scoped operations for dashboard summary, customer create/list, barber create/list, service create/list, and appointment create/list. It persists the first vertical slice, validates appointment duration and barber overlap, scopes EF queries to a validated `tenant_id` claim, initializes SQL Server RLS session context, and writes transactional audit events without customer PII. Production still requires applying the SQL bootstrap, SQL identity permissions, a trusted claim mapping, and the external Azure setup.

## First production vertical slice

1. Entra sign-in, MFA policy, and secure barbershop tenant provisioning.
2. Tenant-scoped customer, barber, service, and appointment records with SQL RLS.
3. Appointment create/update/cancel flows, working hours, conflict checking, and audit events.
4. Real dashboard aggregation and role-appropriate navigation.
5. Consent-aware customer communications after WhatsApp/ACS credentials and templates are configured.
6. CEO Copilot backed only by authorized tenant data, with citations/metrics, rate limits, prompt-injection defenses, and human review before sending campaigns.

## Barbearia Avelino onboarding dependencies

Production onboarding is blocked until the business approves its identity administrator, tenant/domain, user list/roles, privacy notice and lawful basis, data-retention periods, and service catalog/working hours. Messaging also requires the relevant Meta/WhatsApp or Communication Services account, verified sender, approved templates, and credentials. No credentials should be sent through source control or plain-text chat.
