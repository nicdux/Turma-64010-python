CREATE SCHEMA core;
GO
CREATE SCHEMA security;
GO

CREATE TABLE core.Tenants (
    tenant_id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Tenants PRIMARY KEY,
    display_name NVARCHAR(160) NOT NULL,
    created_at DATETIME2(3) NOT NULL CONSTRAINT DF_Tenants_created_at DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE core.Customers (
    tenant_id UNIQUEIDENTIFIER NOT NULL,
    customer_id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Customers_customer_id DEFAULT NEWSEQUENTIALID(),
    full_name NVARCHAR(160) NOT NULL,
    phone_e164 NVARCHAR(16) NULL,
    email NVARCHAR(254) NULL,
    consent_at DATETIME2(3) NULL,
    created_at DATETIME2(3) NOT NULL CONSTRAINT DF_Customers_created_at DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Customers PRIMARY KEY (tenant_id, customer_id),
    CONSTRAINT FK_Customers_Tenants FOREIGN KEY (tenant_id) REFERENCES core.Tenants (tenant_id)
);
GO

CREATE TABLE core.Barbers (
    tenant_id UNIQUEIDENTIFIER NOT NULL,
    barber_id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Barbers_barber_id DEFAULT NEWSEQUENTIALID(),
    display_name NVARCHAR(120) NOT NULL,
    is_active BIT NOT NULL CONSTRAINT DF_Barbers_is_active DEFAULT 1,
    CONSTRAINT PK_Barbers PRIMARY KEY (tenant_id, barber_id),
    CONSTRAINT FK_Barbers_Tenants FOREIGN KEY (tenant_id) REFERENCES core.Tenants (tenant_id)
);
GO

CREATE TABLE core.Services (
    tenant_id UNIQUEIDENTIFIER NOT NULL,
    service_id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Services_service_id DEFAULT NEWSEQUENTIALID(),
    name NVARCHAR(120) NOT NULL,
    duration_minutes SMALLINT NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    is_active BIT NOT NULL CONSTRAINT DF_Services_is_active DEFAULT 1,
    CONSTRAINT PK_Services PRIMARY KEY (tenant_id, service_id),
    CONSTRAINT FK_Services_Tenants FOREIGN KEY (tenant_id) REFERENCES core.Tenants (tenant_id),
    CONSTRAINT CK_Services_duration CHECK (duration_minutes BETWEEN 5 AND 480),
    CONSTRAINT CK_Services_price CHECK (price >= 0)
);
GO

CREATE TABLE core.Appointments (
    tenant_id UNIQUEIDENTIFIER NOT NULL,
    appointment_id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Appointments_id DEFAULT NEWSEQUENTIALID(),
    customer_id UNIQUEIDENTIFIER NOT NULL,
    barber_id UNIQUEIDENTIFIER NOT NULL,
    service_id UNIQUEIDENTIFIER NOT NULL,
    starts_at DATETIME2(3) NOT NULL,
    ends_at DATETIME2(3) NOT NULL,
    status NVARCHAR(20) NOT NULL,
    created_at DATETIME2(3) NOT NULL CONSTRAINT DF_Appointments_created_at DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Appointments PRIMARY KEY (tenant_id, appointment_id),
    CONSTRAINT FK_Appointments_Customers FOREIGN KEY (tenant_id, customer_id) REFERENCES core.Customers (tenant_id, customer_id),
    CONSTRAINT FK_Appointments_Barbers FOREIGN KEY (tenant_id, barber_id) REFERENCES core.Barbers (tenant_id, barber_id),
    CONSTRAINT FK_Appointments_Services FOREIGN KEY (tenant_id, service_id) REFERENCES core.Services (tenant_id, service_id),
    CONSTRAINT CK_Appointments_time CHECK (ends_at > starts_at),
    CONSTRAINT CK_Appointments_status CHECK (status IN ('booked', 'confirmed', 'completed', 'cancelled', 'no_show'))
);
GO

CREATE INDEX IX_Appointments_tenant_starts_at ON core.Appointments (tenant_id, starts_at);
CREATE INDEX IX_Customers_tenant_phone ON core.Customers (tenant_id, phone_e164);
GO

CREATE FUNCTION security.fn_tenantAccessPredicate(@tenant_id UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS fn_accessResult
WHERE @tenant_id = TRY_CONVERT(UNIQUEIDENTIFIER, SESSION_CONTEXT(N'tenant_id'));
GO

CREATE SECURITY POLICY security.TenantIsolationPolicy
    ADD FILTER PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Tenants,
    ADD BLOCK PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Tenants AFTER INSERT,
    ADD BLOCK PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Tenants AFTER UPDATE,
    ADD FILTER PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Customers,
    ADD BLOCK PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Customers AFTER INSERT,
    ADD BLOCK PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Customers AFTER UPDATE,
    ADD FILTER PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Barbers,
    ADD BLOCK PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Barbers AFTER INSERT,
    ADD BLOCK PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Barbers AFTER UPDATE,
    ADD FILTER PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Services,
    ADD BLOCK PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Services AFTER INSERT,
    ADD BLOCK PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Services AFTER UPDATE,
    ADD FILTER PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Appointments,
    ADD BLOCK PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Appointments AFTER INSERT,
    ADD BLOCK PREDICATE security.fn_tenantAccessPredicate(tenant_id) ON core.Appointments AFTER UPDATE
WITH (STATE = ON, SCHEMABINDING = ON);
GO
