# Project Structure Documentation

This document explains the folder hierarchy and decoupling architecture of the **CogMedi Hospital Management System** solution.

---

## 1. Directory Structure

```text
CogMediHospitalManagementSystemSolution
│
├── myNewRepo (CogMediHospitalManagementSystem - MVC Web Application)
│   ├── Controllers (MVC Web View Controllers)
│   ├── Views (Razor MVC Views)
│   ├── Models (Entity Framework Models)
│   ├── DTOs (Data Transfer Objects)
│   ├── Enums (User Role Enums)
│   ├── Services (Core Business Logic Services)
│   ├── Repositories (Data Access Layer Repositories)
│   ├── ViewModels (UI Binding Models)
│   ├── Data (DbContext & DbInitializer Seeding)
│   ├── wwwroot (Static JS, CSS, Media)
│   ├── Program.cs (MVC App Host Configuration)
│   └── appsettings.json (MVC Settings & API URL Configuration)
│
├── CogMediHospitalManagementSystemApi (ASP.NET Core Web API Gateway)
│   ├── Controllers (API Endpoints Only)
│   ├── Properties (launchSettings.json)
│   ├── Program.cs (API App Host Configuration)
│   └── appsettings.json (Database Connection Settings)
│
└── CogMediHospitalManagementSystem.slnx (Visual Studio Solution File)
```

---

## 2. Project Responsibilities

### A. MVC Web Application (`myNewRepo` / `CogMediHospitalManagementSystem`)
The primary project containing the application core logic:
* **Models / Data Layer**: Interacts with SQL Server via EF Core.
* **Services**: Encapsulates core business transactions (hiring staff, updating EHR, managing medicine stock).
* **Repositories**: Abstracted CRUD access to DB tables.
* **MVC Controllers & Views**: Renders UI dashboard views and handles browser interactions.

### B. Web API Gateway (`CogMediHospitalManagementSystemApi`)
A lightweight layer hosting the API controllers:
* Exposes standard JSON REST endpoints (`/api/*`).
* Invokes backend services via dependency injection referencing the MVC project.
* Exposes stateless hooks for external integrators and client scripts.

---

## 3. Rationale for Relocation
* **Single Source of Truth**: Eliminates duplicated model/DTO/enum files across separate repositories by building a direct assembly reference.
* **Scalability**: The Web API and the MVC front-end can be built, scaled, and hosted independently.
* **Separation of Concerns**: UI rendering logic is completely decoupled from stateless API routing.
