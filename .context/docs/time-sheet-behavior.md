---
type: doc
name: time-sheet-behavior
description: Detailed description of the Time Sheet (Folha de Ponto) behavior and rules
category: domain
generated: 2026-03-21
status: filled
scaffoldVersion: "2.0.0"
---

## Time Sheet (Folha de Ponto) Behavior

The Time Sheet (Folha de Ponto) is the most critical module of the ARID_PONTO system. It handles the official record of employee attendance, punch-ins/outs, and the calculation of worked hours, overtime, and nighttime premiums.

### Core Data Behavior

- **Punch Persistence**: Every punch (batida) is recorded with a timestamp and associated with an employee.
- **Immutability of Original Records**: While corrections can be made (justificativas), the original punch data must be preserved for auditing.
- **Calculation Chain**: 
    1.  **Punches**: Raw entry/exit times.
    2.  **Shift Allocation**: Punches are mapped to the employee's assigned work schedule (jornada).
    3.  **Tolerance Rules**: Arrival and departure tolerances are applied before calculating net hours.
    4.  **Interruptions**: Absences (with or without justification) and leaves are factored in.
    5.  **Final Tally**: Results are exported for payroll processing.

### Critical Safety Rules

> [!IMPORTANT]
> **Minimal Impact Principle**: Any modification to the Time Sheet logic must be extremely surgical. Rewriting existing logic is strictly prohibited unless there is no other path and it has been explicitly approved.

- **Incremental Changes**: Always favor adding small overrides or specific logic segments over refactoring the core calculation engine.
- **Verification**: Every change to the Time Sheet must be verified against historical data to ensure no retrospective changes in calculated hours for previous periods.

### Data Access Pattern

- **EF Core**: Used for CRUD operations on justification records and configuration settings.
- **Dapper**: Extensively used in the **Time Mirror (Espelho de Ponto)** for high-performance retrieval of large datasets, especially when generating reports across hundreds of employees.

### Typical UI Flow

1.  The employee/manager views the current period.
2.  The system uses Dapper to pull complex joined data from the MySQL database.
3.  Any adjustments (e.g., forgetting to punch out) are handled via modals that use EF Core to save the adjustment record.
4.  The Time Sheet recalculates the affected day in real-time or via a batch process.

## Related Resources
- [architecture.md](./architecture.md)
- [data-flow.md](./data-flow.md)
- [glossary.md](./glossary.md)
