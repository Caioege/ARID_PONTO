---
type: doc
name: testing-strategy
description: Test frameworks, patterns, coverage requirements, and quality gates
category: testing
generated: 2026-03-21
status: filled
scaffoldVersion: "2.0.0"
---
## Testing Strategy

Quality in ARID_PONTO is maintained through a combination of automated backend tests, manual verification, and database integrity checks.

## Test Types

- **Unit Tests (Backend)**: Implemented using XUnit or NUnit within the solution. These tests focus on logic in `AriD.Servicos` and `AriD.BibliotecaDeClasses`.
- **Frontend Verification**: Manual testing of UI components in `wwwroot/Scripts/Paginas`.
- **Database Testing**: Verification of SQL scripts against a staging database to ensure schema migrations (e.g., `bonus_tabelas.sql`) work as expected.

## Running Tests

- **Backend**: Use the Test Explorer in Visual Studio or run `dotnet test` from the root.
- **Frontend**: Currently relies on manual browser verification and console logging (see `site.js` for common utilities).

## Quality Gates

- **Build Success**: All PRs must compile successfully in both Debug and Release modes.
- **SQL Verification**: Any new `.sql` script must be idempotent and tested against a fresh database backup.
- **Validation**: Every new form must implement both client-side (`jquery.validate`) and server-side validation.

## Related Resources

- [development-workflow.md](./development-workflow.md)
