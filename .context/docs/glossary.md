---
type: doc
name: glossary
description: Project terminology, type definitions, domain entities, and business rules
category: glossary
generated: 2026-03-21
status: filled
scaffoldVersion: "2.0.0"
---
## Glossary & Domain Concepts

ARID_PONTO is a human resources management system focused on time tracking and employee benefits.

## Core Terms

- **Servidor (Employee/Public Servant)**: The primary individual whose time is being tracked. Surfaced in `Servidor/index.js` and related controllers.
- **Vínculo de Trabalho (Work Link)**: The official relationship between the employee and the organization, defining their role and rules.
- **Ponto (Punch/Time Clock Record)**: A record of an employee entering or leaving work.
- **Ausência (Absence)**: A period where the employee was expected to work but did not.
- **Justificativa (Justification)**: A formal reason provided for an absence (e.g., medical certificate).
- **Espelho de Ponto (Time Sheet/Mirror)**: The final report showing all punches and calculated hours for a period.
- **Bonus (Benefits)**: Refers to VA (Vale Alimentação) and VT (Vale Transporte) calculations.

## Type Definitions

Core entities are defined in **`AriD.BibliotecaDeClasses`**:
- `Servidor`: Represents the employee profile.
- `RegistroPonto`: Represents an individual time clock record.
- `BonusCalculo`: Represents the result of a benefit calculation.

## Enumerations

- `TipoVinculo`: Defines categories like "Efetivo", "Comissionado", etc.
- `SituacaoPonto`: Defines the status of a punch (e.g., "Pendente", "Validado").

## Personas / Actors

- **Admin**: Full system access, manages users and global settings.
- **Manager (RH)**: Manages employee records, validates absences, and calculates bonuses.
- **Servidor**: Views their own time mirror and submits justifications.

## Domain Rules & Invariants

- A punch must have a valid timestamp and be associated with a `Servidor`.
- Bonus calculations are based on the number of days worked minus absences.
- Justifications must be approved by a Manager to be considered valid for hour calculations.

## Related Resources

- [project-overview.md](./project-overview.md)
