---
type: doc
name: project-overview
description: High-level overview of the project, its purpose, and key components
category: overview
generated: 2026-03-21
status: filled
scaffoldVersion: "2.0.0"
---
## Project Overview

ARID_PONTO is a comprehensive management system for employee time tracking (Ponto Eletrônico) and benefit calculation (VA/VT). It provides an intuitive interface for both employees and HR administrators to manage work hours, absences, and specialized calculations for public sector requirements.

## Codebase Reference

> **Detailed Analysis**: For complete symbol counts, architecture layers, and dependency graphs, see [`codebase-map.json`](./codebase-map.json).

## Quick Facts

- **Root**: `C:\src\ARID_PONTO`
- **Languages**: C# (.NET 8), JavaScript (jQuery), SQL.
- **Primary Framework**: ASP.NET Core MVC.
- **Entry Points**: 
    - [Program.cs](file:///c:/src/ARID_PONTO/AriD.GerenciamentoDePonto/Program.cs) (Backend)
    - [main.js](file:///c:/src/ARID_PONTO/AriD.GerenciamentoDePonto/wwwroot/js/main.js) (Frontend)

## File Structure & Code Organization

- `AriD.GerenciamentoDePonto/` — The main MVC web application.
- `AriD.Servicos/` — Business logic and data processing services.
- `AriD.BibliotecaDeClasses/` — Shared domain models and entities.
- `PdfParser/` — Utility for extracting data from PDF reports.
- Root SQL Files — Database schema and versioned migration scripts (e.g., `alteracoes_20260315.sql`).

## Technology Stack Summary

The project leverages a modern .NET 8 backend with a robust SQL Server database. The frontend is built using standard web technologies (HTML/CSS/JS) with jQuery for interactivity and Bootstrap for layout. Asset management and task automation are handled by Gulp.

## UI & Interaction Libraries

- **Bootstrap**: Core layout and responsive components.
- **jQuery**: Client-side logic and Ajax communications.
- **SweetAlert2**: Beautiful and interactive user notifications.
- **JQuery Validation**: Unobtrusive client-side form validation.

## Getting Started Checklist

1.  **Configure Database**: Run the SQL scripts in the root directory to set up the local schema.
2.  **NuGet Restore**: Restore C# dependencies via Visual Studio or `dotnet restore`.
3.  **NPM Install**: Run `npm install` in the `AriD.GerenciamentoDePonto` directory.
4.  **Run Development**: Launch the web application from Visual Studio and review the `development-workflow.md` for daily tasks.

## Related Resources

- [architecture.md](./architecture.md)
- [development-workflow.md](./development-workflow.md)
- [tooling.md](./tooling.md)
- [codebase-map.json](./codebase-map.json)
