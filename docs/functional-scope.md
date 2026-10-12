# Functional scope and initial onboarding

## Current prototype

The dashboard demonstrates a dark-only barbershop overview: headline business metrics, revenue chart with area/bar switching, service mix donut, today's appointment list, team performance, and a CEO Copilot insight card. All values are illustrative and the controls are not wired to business workflows.

The API currently exposes a health endpoint and an authenticated demo dashboard summary. It does not read or modify customer records.

## First production vertical slice

1. Entra sign-in, MFA policy, and secure barbershop tenant provisioning.
2. Tenant-scoped customer, barber, service, and appointment records with SQL RLS.
3. Appointment create/update/cancel flows, working hours, conflict checking, and audit events.
4. Real dashboard aggregation and role-appropriate navigation.
5. Consent-aware customer communications after WhatsApp/ACS credentials and templates are configured.
6. CEO Copilot backed only by authorized tenant data, with citations/metrics, rate limits, prompt-injection defenses, and human review before sending campaigns.

## Barbearia Avelino onboarding dependencies

Production onboarding is blocked until the business approves its identity administrator, tenant/domain, user list/roles, privacy notice and lawful basis, data-retention periods, and service catalog/working hours. Messaging also requires the relevant Meta/WhatsApp or Communication Services account, verified sender, approved templates, and credentials. No credentials should be sent through source control or plain-text chat.
