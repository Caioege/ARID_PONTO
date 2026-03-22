---
type: doc
name: security
description: Security policies, authentication, secrets management, and compliance requirements
category: security
generated: 2026-03-21
status: filled
scaffoldVersion: "2.0.0"
---
## Security & Compliance Notes

ARID_PONTO handles sensitive personnel data, including attendance records and benefit information. Protecting this data is a top priority, enforced through server-side validation and secure coding practices.

## Authentication & Authorization

- **Authentication**: The system uses a session-based authentication model provided by ASP.NET Core. Users must log in with a unique username and password.
- **Authorization**: Role-Based Access Control (RBAC) is implemented to restrict access to sensitive features (e.g., only HR Admins can calculate bonuses or edit employee records).
- **Session Management**: Sessions are managed securely on the server, with appropriate timeouts and secure cookie flags.

## Secrets & Sensitive Data

- **Connection Strings**: Stored in `appsettings.json` or environment variables. In production, these should be managed through secure vault services.
- **Data Classification**: Employee identifiers, work history, and bonus amounts are considered highly sensitive.
- **Encryption**: Data at rest in the SQL Server should be protected via Transparent Data Encryption (TDE) where possible.

## Compliance & Policies

- **LGPD (Brazil)**: As a Brazilian project, compliance with the General Data Protection Law is essential. This includes data minimization and the right to access/rectify attendance records.
- **Audit Logs**: The system includes SQL-based audit scripts (e.g., `auditoria_ausencias.sql`) to track changes to critical data points.

## Related Resources

- [architecture.md](./architecture.md)
