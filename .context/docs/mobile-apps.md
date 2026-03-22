---
type: doc
name: mobile-apps
description: Detailed documentation of the Flutter mobile applications and their backend integration
category: architecture
generated: 2026-03-22
status: filled
scaffoldVersion: "2.0.0"
---
## Mobile Applications Overview

The ARID_PONTO system includes native-like cross-platform mobile functionality facilitated by Flutter. These apps are entirely housed in the `Aplicativos/` folder at the root of the solution. The apps provide specific, specialized workflows that are better suited for mobile devices (like geolocation tracking, photo taking, and mobility) than the standard web interface.

There are two primary applications:
1. **PONTO App** (`Aplicativos/ARID_PONTO_APP/`)
2. **Motoristas App** (`Aplicativos/ARID_MOTORISTA/`)

## PONTO App

The PONTO Application is designed for all servers (employees) to manage their daily time and attendance routines remotely.

### Key Features
- **Remote Time Registration (Clock-In/Out)**: Allows employees to punch their time remotely. This feature mandates the capture of a **photo** (facial recognition/proof of presence) and **geolocation** (GPS coordinates) to ensure authenticity.
- **Record Consultation**: Employees can view their past time punches and history.
- **Timesheet (Folha de Ponto)**: Visualization of the employee's consolidated timesheet.
- **Justifications & Certificates**: Functionality to upload medical certificates (atestados) or provide justifications for absences or delays.
- **Other Data Visualization**: Users can see their schedules, bonds (vínculos), organizational units, and organizational events.

### Backend Integration
The Ponto App communicates primarily with the `AppController.cs` in the backend API project (`AriD.GerenciamentoDePonto`).

- **`POST /api/app/autentique`**: Authenticates the user and returns their profile (including base64 photo).
- **`GET /api/app/horarios-trabalho/{servidorId}`**: Fetches the employee's assigned work schedules.
- **`GET /api/app/eventos/{organizacaoId}`**: Retrieves organizational events.
- **`GET /api/app/vinculos/{servidorId}`**: Gets employee work bonds.
- **`GET /api/app/unidade/{vinculoId}`**: Gets the units/locations assigned.
- **`GET /api/app/justificativas/{organizacaoId}`**: Gets available justification types.
- **`GET /api/app/folha-ponto/{vinculoId}/{unidadeId}/{mesDeReferencia}`**: Generates and downloads the PDF timesheet.
- **`GET /api/app/ultimos-registros-servidor/{servidorId}`**: Obtains the latest time punches.
- **`POST /api/app/receptar-ponto`**: Receives a new time punch payload (multipart form including photo and GPS).

## Motoristas App

The Motoristas (Drivers) Application is a new feature specifically tailored for servers who operate vehicles and execute predefined routes.

### Key Features
- **Route Execution**: Drivers can view and start assigned routes.
- **Pre-trip Checklist**: Before starting a route, the driver must fill out a mandatory checklist registered in the system (e.g., checking tires, fuel, vehicle condition).
- **Background Location Tracking**: Once a route is initiated, the application begins sending the driver's geographic location to the server in the background. This allows managers (gestores) to track the real-time position of their fleet.

### Backend Integration
The Motoristas App communicates primarily with the `RotaAppController.cs` in the backend API project.

- **`POST /api/rota-app/autentique`**: Authenticates the driver and retrieves their profile details.
- *(Additional endpoints for checklist submission, route management, and background location reception are incorporated or will be added as this module evolves.)*

## Architecture and Interoperability

Both mobile applications are treated as external clients in the system's architecture. They consume the internal APIs (`AriD.GerenciamentoDePonto/Controllers/`) which then pass the requests down to the Service Layer (`AriD.Servicos`). 

Because they rely on the same business logic as the web platform, any rules regarding time tracking (e.g., tolerance calculations for Intercalated Shifts, bonus deductions) applied in the `ServicoDeFolhaDePonto` or `ServicoRegistroDePonto` seamlessly affect the mobile users. 

### Security & Token Handling
Mobile apps currently use simple payload-based authentication but must ensure secure transmission of sensitive data like GPS coordinates and facial biometrics to the API endpoints.

> **Hardware Clock-ins**: In addition to the mobile app, ARID_PONTO also supports physical time clocks via `RegistroApiController.cs` which allows external biometric/RFID equipment (`AIFaceEVO.API-ARID.TECNOLOGIA`) to integrate.
