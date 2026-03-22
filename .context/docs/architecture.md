---
type: doc
name: architecture
description: System architecture, layers, patterns, and design decisions
category: architecture
generated: 2026-03-21
status: filled
scaffoldVersion: "2.0.0"
---
## Architecture Notes

The ARID_PONTO system is a time management and human resources application built on the .NET 8 ecosystem. It follows a modular monolith approach, separating business logic, services, and the web presentation layer to ensure maintainability and scalability.

## System Architecture Overview

The application is structured as a traditional N-Tier architecture:
1.  **Presentation Layer (`AriD.GerenciamentoDePonto`)**: An ASP.NET Core MVC application providing the user interface and API endpoints.
2.  **Service Layer (`AriD.Servicos`)**: Encapsulates business rules and orchestrates data movement between the presentation and data layers.
3.  **Domain/Class Library (`AriD.BibliotecaDeClasses`)**: Contains the core entities, domain logic, and shared contracts.
4.  **Support Tools (`PdfParser`)**: Specialized modules for document processing.

Requests typically flow from the browser through the MVC Controllers in `AriD.GerenciamentoDePonto`, which call services in `AriD.Servicos`. These services interact with the database (managed via SQL scripts in the root) and return domain objects from `AriD.BibliotecaDeClasses`.

## Architectural Layers

- **Presentation**: ASP.NET Core MVC (`AriD.GerenciamentoDePonto`)
- **Business Logic**: Service implementation (`AriD.Servicos`)
- **Domain**: Entities and shared logic (`AriD.BibliotecaDeClasses`)
- **Utilities**: PDF parsing and other helpers (`PdfParser`)
- **Frontend Assets**: jQuery, Bootstrap, and custom scripts (`AriD.GerenciamentoDePonto/wwwroot`)

> See [`codebase-map.json`](./codebase-map.json) for complete symbol counts and dependency graphs.

## Detected Design Patterns

| Pattern | Confidence | Locations | Description |
|---------|------------|-----------|-------------|
| MVC | High | `AriD.GerenciamentoDePonto/Controllers` | Primary web framework pattern |
| Service Layer | High | `AriD.Servicos/` | Orchestrates logic between controllers and data |
| Repository (Implied) | Medium | Data access files | Handles database interactions |
| Dependency Injection | High | `Program.cs` / `Startup.cs` | Native .NET Core DI for services |

## Entry Points

- [Program.cs](file:///c:/src/ARID_PONTO/AriD.GerenciamentoDePonto/Program.cs) - Main application startup.
- [Gulpfile.js](file:///c:/src/ARID_PONTO/AriD.GerenciamentoDePonto/Gulpfile.js) - Asset pipeline entry point.
- [Dashboard/index.js](file:///c:/src/ARID_PONTO/AriD.GerenciamentoDePonto/wwwroot/Scripts/Paginas/Dashboard/index.js) - Main client-side entry point for the dashboard.

## Public API

The system exposes its functionality through MVC Controllers and potentially Web API endpoints.
_See codebase-map.json for a complete list of exported symbols._

## Internal System Boundaries

The main boundary exists between the web application and the library/service projects. This ensures that business logic remains independent of the UI framework. Data consistency is maintained through SQL transactions and service-layer validation.

## External Service Dependencies

- **MySQL**: Primary data store.
- **Entity Framework Core**: Used for the majority of data operations (CRUD).
- **Dapper**: Used for performance-critical areas such as dashboards and complex reports.
- **Client-side Libraries**: jQuery, Bootstrap, SweetAlert2 for UI.

## Key Decisions & Trade-offs

- **Server-side Rendering**: Chosen for SEO and initial load performance in a management context.
- **Standard Screen Pattern**: Most screens follow a consistent layout with standardized data tables, filters, and modal-based editing.
- **Hybrid Data Access**: EF Core for maintainability in standard modules, Dapper for raw performance in dashboards and high-volume reports.
- **jQuery/Manual JS**: Used for client-side interactivity, favoring direct DOM manipulation for specific management tasks over a heavyweight SPA framework.

## Top Directories Snapshot

- `AriD.GerenciamentoDePonto/` — Main web project (~200 files)
- `AriD.Servicos/` — Business services (~50 files)
- `AriD.BibliotecaDeClasses/` — Domain entities (~100 files)
- `PdfParser/` — Utility for PDF handling
- SQL Files in root — Database migrations and setup scripts

## Related Resources

- [project-overview.md](./project-overview.md)
- [data-flow.md](./data-flow.md)
- [codebase-map.json](./codebase-map.json)
