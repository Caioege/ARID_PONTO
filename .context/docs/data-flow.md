---
type: doc
name: data-flow
description: How data moves through the system and external integrations
category: data-flow
generated: 2026-03-21
status: filled
scaffoldVersion: "2.0.0"
---
## Data Flow & Integrations

Data in ARID_PONTO primarily flows from user interactions in the web interface through a structured backend pipeline to the MySQL database. The system also integrates with PDF parsing services for automated data entry.

## Module Dependencies

- **`AriD.GerenciamentoDePonto`** → `AriD.Servicos`, `AriD.BibliotecaDeClasses`
- **`AriD.Servicos`** → `AriD.BibliotecaDeClasses`
- **`PdfParser`** → `AriD.BibliotecaDeClasses` (shared entities)
- **Root SQL Scripts** → Database Schema

## Service Layer

- **`AriD.Servicos`**: Contains the core business logic. Typical services include:
    - `PontoService`: Calculations for work hours and overtime.
    - `UsuarioService`: Management of users and permissions.
    - `BonusService`: Logic for VA/VT and other benefits.

## High-level Flow

1.  **Input**: User performs an action (e.g., registering a punch, requesting a bonus) via the browser.
2.  **Frontend Validation**: `jquery.validate.unobtrusive.js` ensures data integrity before submission.
3.  **Controller Entry**: The request hits an ASP.NET MVC Controller in `AriD.GerenciamentoDePonto`.
4.  **Service Invocation**: The Controller delegates logic to the appropriate service in `AriD.Servicos`.
5.  **Data Persistence**: Services use an ORM or direct ADO.NET (implied by SQL scripts) to persist changes to the SQL Server.
6.  **Response**: The Controller returns a View or a JSON response (using `SweetAlert2` for notifications).

## Internal Movement

The system uses standard C# methodology for internal communication:
- **Dependency Injection**: Services and dependencies are injected into controllers.
- **DTOs/Entities**: `AriD.BibliotecaDeClasses` defines the objects that travel between layers.

## External Integrations

- **PDF Parsing**: The `PdfParser` module is used to extract information from reports or external documents, feeding it into the system's domain model.
- **MySQL**: Centralized database using EF Core for general modules and Dapper for high-performance dashboards and the [Time Sheet (Folha de Ponto)](./time-sheet-behavior.md).

## Observability & Failure Modes

- **Logging**: Common .NET logging patterns are used to track errors and system health.
- **Database Transactions**: Ensure data consistency during complex operations (e.g., bonus calculations involving multiple tables).
- **Client-side Alerts**: `ui-toasts.js` and `sweetalert2` provide immediate feedback to users on success or failure.

## Related Resources

- [architecture.md](./architecture.md)
