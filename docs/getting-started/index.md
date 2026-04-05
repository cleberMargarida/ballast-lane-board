# 🚀 Getting Started

## What is Ballast Lane Board?

Ballast Lane Board is a full-stack task management platform built with modern .NET and Angular technologies. It demonstrates Clean Architecture principles with enterprise-grade authentication, a Kanban-style UI, and Docker Compose deployment.

---

## Key Features

- **Clean Architecture** — 4-layer separation: Domain, Application, Infrastructure, WebApi
- **Keycloak OIDC Authentication** — OpenID Connect with role-based access control
- **Task CRUD with Status Workflow** — Pending → InProgress → Completed transitions
- **Role-Based Access** — Admins see all tasks; users see only their own
- **Angular 19 Kanban Board** — Drag-and-drop task management with Tailwind CSS
- **Docker Compose Deployment** — One-command full-stack deployment
- **Comprehensive Test Suite** — Unit tests (Domain + Application) and integration tests (Infra + WebApi with Testcontainers)
- **Auto-Migration on Startup** — EF Core migrations run automatically when the API starts

---

## Architecture at a Glance

| Layer | Project | Responsibility |
|---|---|---|
| **Domain** | `BallastLaneBoard.Domain` | Entities, value objects, business rules. Zero dependencies. |
| **Application** | `BallastLaneBoard.Application` | Services, DTOs, repository/UoW interfaces. Depends only on Domain. |
| **Infrastructure** | `BallastLaneBoard.Infra` | EF Core, Keycloak client, migrations. Implements Application interfaces. |
| **WebApi** | `BallastLaneBoard.WebApi` | ASP.NET controllers, Swagger, JWT auth, SPA hosting. |
| **Client** | `BallastLaneBoard.ClientApp` | Angular 19 SPA with Tailwind CSS and OIDC integration. |

---

## Next Steps

- [📦 Installation](installation.md) — Prerequisites and setup
- [⚡ Quick Start](quick-start.md) — Run the app and make your first request
- [🏛️ Architecture Overview](../architecture/index.md) — Deep dive into patterns and decisions
