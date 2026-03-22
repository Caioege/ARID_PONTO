---
type: doc
name: tooling
description: Scripts, IDE settings, automation, and developer productivity tips
category: tooling
generated: 2026-03-21
status: filled
scaffoldVersion: "2.0.0"
---
## Tooling & Productivity Guide

The development environment for ARID_PONTO centers around the Microsoft stack for the backend and modern JavaScript utilities for the frontend.

## Required Tooling

- **Visual Studio 2022**: Recommended IDE for .NET 8 development.
- **SQL Server Management Studio (SSMS)**: For managing the local database and running migration scripts.
- **Node.js & NPM**: Required for running the Gulp-based asset pipeline.
- **Gulp CLI**: Used for compiling scripts and styles (see `Gulpfile.js`).

## Recommended Automation

- **`npm run dev`**: Starts the asset pipeline in watch mode, automatically compiling changes to JS and CSS in `wwwroot`.
- **SQL Script Versioning**: Follow the naming convention `alteracoes_YYYYMMDD.sql` to keep database changes organized.
- **Pre-commit Checks**: Developers should run `dotnet build` and `npm run build` before pushing to ensure zero build errors.

## IDE / Editor Setup

- **Visual Studio Extensions**: 
    - JavaScript Snippets
    - Markdown Editor (for maintaining `.context` docs)
- **EditorConfig**: A `.editorconfig` file is recommended to maintain consistent indentation and coding styles across the team.

## Related Resources

- [development-workflow.md](./development-workflow.md)
