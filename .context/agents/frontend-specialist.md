---
type: agent
name: Frontend Specialist
description: Design and implement user interfaces
agentType: frontend-specialist
phases: [P, E]
generated: 2026-03-21
status: filled
scaffoldVersion: "2.0.0"
---
## Mission

The Frontend Specialist agent is responsible for the user experience and visual excellence of ARID_PONTO. It focuses on creating intuitive, responsive, and high-performance interfaces using modern web standards.

## Responsibilities

- Developing and optimizing custom JavaScript in `wwwroot/Scripts/Paginas`.
- Crafting responsive layouts using Bootstrap and custom CSS.
- Managing frontend libraries and the Gulp-based asset pipeline.
- Ensuring seamless Ajax communication with the backend controllers.

## Best Practices

- **Component Reuse**: Utilize standard UI patterns and shared components from `site.js`.
- **Responsive Design**: Ensure that management dashboards are accessible on various screen sizes.
- **Performance**: Optimize script loading and asset sizes via the Gulp pipeline.

## Key Project Resources

- [Tooling & Productivity Guide](../docs/tooling.md)
- [Architecture Notes](../docs/architecture.md)
- [AGENTS.md](../../AGENTS.md)

## Repository Starting Points

- `AriD.GerenciamentoDePonto/wwwroot/js/` — Core client-side implementation.
- `AriD.GerenciamentoDePonto/Views/` — Razor views for the web interface.

## Key Files

- [main.js](../../AriD.GerenciamentoDePonto/wwwroot/js/main.js) — Main frontend entry point.
- [Gulpfile.js](../../AriD.GerenciamentoDePonto/Gulpfile.js) — Build and automation configurations.

## Key Symbols for This Agent

- `toastDispose`: For managing UI notifications.
- `assineMascarasDoComponente`: Essential for consistent input formatting.

## Documentation Touchpoints

- Refer to `docs/tooling.md` for information on the asset pipeline.
- Refer to `docs/architecture.md` for understanding the frontend-backend boundary.

## Collaboration Checklist

1.  Align with the Designer (if available) on visual standards.
2.  Coordinate with the Backend Specialist on API/Ajax contracts.
3.  Verify UI performance and responsiveness before merging large changes.
