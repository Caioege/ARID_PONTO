---
type: doc
name: development-workflow
description: Day-to-day engineering processes, branching, and contribution guidelines
category: workflow
generated: 2026-03-21
status: filled
scaffoldVersion: "2.0.0"
---
## Development Workflow

The development process for ARID_PONTO follows a standard .NET development lifecycle, with a focus on database-driven changes and server-side MVC implementation.

## Branching & Releases

- **Branching Model**: Trunk-based development for small changes; feature branches for larger modules (like the Bonus module).
- **PR Strategy**: All changes must be reviewed and pass local build tests before merging.
- **Releases**: Tagged releases are created after successful testing in the development environment.

## Local Development

To set up the project locally:
1.  **Clone** the repository.
2.  **Database Setup**: Execute the SQL scripts in the root (starting with `alteracoes.sql` and `bonus_tabelas.sql`) on your local **MySQL** instance.
3.  **Dependencies**:
    - Backend: Open `AriD.GerenciamentoDePonto.sln` in Visual Studio and allow NuGet to restore packages.
    - Frontend: Run `npm install` in the `AriD.GerenciamentoDePonto` directory.
4.  **Run**:
    - Use Visual Studio to run the `AriD.GerenciamentoDePonto` project.
    - For asset compilation, use `npm run dev` or run Gulp tasks directly.

## Critical Safety Rules

> [!IMPORTANT]
> **Minimal Impact Principle**: Avoid rewriting code that already works unless absolutely necessary and approved. Changes must be surgical to minimize the risk of regressions, especially in the **Time Sheet (Folha de Ponto)** module.

## Change Documentation Requirements

Every request for code changes must include:
1.  **SQL Scripts**: A `.sql` file named by date (e.g., `alteracoes_YYYYMMDD.sql`) containing all database modifications.
2.  **Change Log**: One or more `.md` files in a general changes folder documenting all modifications made to the system.

## Code Review Expectations

- Ensure all database changes follow the naming convention above.
- Verify that both client-side and server-side validation are in place.
- Confirm that existing Time Sheet logic remains intact unless a fix was requested.
- Reference [AGENTS.md](../../AGENTS.md) for collaboration guidelines.

## Onboarding Tasks

- Review `docs/architecture.md` to understand the project structure.
- Set up the local database and run the application to explore the Dashboard and User management features.

## Related Resources

- [testing-strategy.md](./testing-strategy.md)
- [tooling.md](./tooling.md)
