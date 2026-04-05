# Specification Compliance

This document maps every requirement from [SPECIFICATION.md](https://github.com/user/ballast-lane-board/blob/master/SPECIFICATION.md) to its implementation in the Ballast Lane Board project.

---

## Compliance Matrix

| # | Requirement | Status | Implementation | Key Files |
|---|---|---|---|---|
| 1 | API and data layer using .NET C#, ASP.NET MVC, Web API | ✅ | ASP.NET .NET 10 Web API with MVC controllers | `WebApi/Program.cs`, `Controllers/` |
| 2 | A database or data store | ✅ | PostgreSQL 17 in Docker | `docker-compose.yml` |
| 3 | Clean Architecture principles | ✅ | 4-project structure with strict dependency flow | `Domain/`, `Application/`, `Infra/`, `WebApi/` |
| 4 | Test-Driven Development (TDD) | ✅ | xUnit tests across 4 test projects | `tests/` |
| 5 | Informal user story | ✅ | Documented in project README | `README.md` |
| 6 | CRUD operations | ✅ | Full REST API: GET, POST, PUT, PATCH, DELETE | `Controllers/TasksController.cs` |
| 7 | User creation & authentication | ✅ | Keycloak OIDC + AuthController for registration | `Controllers/AuthController.cs`, `Keycloak/KeycloakAdminClient.cs` |
| 8 | Persist user data | ✅ | PostgreSQL `AppUser` table + Keycloak user store | `Identity/AppUser.cs`, `Data/UserUoW.cs` |
| 9 | Do NOT use: Entity Framework, Dapper, Mediator | ✅ | Raw ADO.NET (`IDbConnection` via Npgsql) with Repository + UoW pattern | `Infra/Data/` |
| 10 | Database: PK + ≥2 additional fields | ✅ | TaskItem (6 data fields), AppUser (5 data fields) | `TaskItem.cs`, `AppUser.cs` |
| 11 | ASP.NET Web API with proper HTTP verbs/responses | ✅ | RESTful endpoints with 200, 201, 204, 400, 401, 403, 404 | `Controllers/TasksController.cs` |
| 12 | Auth: user creation, login, authorized & non-authorized endpoints | ✅ | Register (public), me/sync (authorized), tasks (authorized) | `Controllers/AuthController.cs` |
| 13 | Data access layer for CRUD | ✅ | Repository + UnitOfWork pattern | `Abstractions/`, `Data/` |
| 14 | Business logic layer (independent) | ✅ | Domain entities + Application services, zero framework deps | `TaskItem.cs`, `TaskService.cs`, `UserService.cs` |
| 15 | Unit tests: data access, business logic, API | ✅ | 4 test projects covering all layers | `tests/` |
| 16 | Frontend with any framework | ✅ | Angular 19 SPA | `BallastLaneBoard.ClientApp/` |
| 17 | Responsive UI | ✅ | Tailwind CSS 4 with responsive classes | `ClientApp/src/` |
| 18 | State management | ✅ | RxJS-based service state management | `api.service.ts`, `auth.service.ts` |
| 19 | Task fields: title, description, status, due_date | ✅ | All fields present in TaskItem entity | `TaskItem.cs` |
| 20 | Tasks associated with user | ✅ | `UserId` foreign key + ownership enforcement | `TaskItem.cs`, `TaskService.cs` |
| 21 | README with setup instructions | ✅ | Comprehensive setup guide with architecture overview | `README.md` |
| 22 | Seeded data & demo credentials | ✅ | Keycloak realm import + local user seeding | `ballast-lane-board-realm.json`, `Sql/init.sql` |

---

## Detailed Analysis

### CRUD Operations

**Status:** ✅ Fully Implemented

`TasksController` implements complete RESTful CRUD:

| Endpoint | Verb | Success Code | Description |
|---|---|---|---|
| `/api/tasks` | GET | 200 | List (admin: all, user: own) |
| `/api/tasks/{id}` | GET | 200 | Get by ID with ownership check |
| `/api/tasks` | POST | 201 | Create with validation |
| `/api/tasks/{id}` | PUT | 200 | Update with ownership check |
| `/api/tasks/{id}/status` | PATCH | 204 | Status transition with validation |
| `/api/tasks/{id}` | DELETE | 204 | Delete with ownership check |

Error handling uses the `Result<T>` pattern — validation failures return `400 Bad Request` with typed error messages rather than exceptions.

### Authentication & Authorization

**Status:** ✅ Fully Implemented

The project uses **Keycloak OIDC** (enterprise-grade, standards-compliant):

1. **Registration**: `POST /api/auth/register` creates a user in Keycloak via Admin REST API, then mirrors locally as `AppUser`
2. **Login**: Users authenticate via Keycloak's login page (OIDC Authorization Code flow). The API does not handle passwords.
3. **Authorized endpoints**: All `/api/tasks/*` endpoints require `[Authorize]` with a valid JWT Bearer token
4. **Public endpoints**: `POST /api/auth/register` is `[AllowAnonymous]`
5. **Role-based access**: Admins (Keycloak `realm_access.roles`) see all tasks; users see their own

### Clean Architecture

**Status:** ✅ Fully Implemented

```
WebApi (composition root)
  └─→ Application (services + interfaces)
       └─→ Domain (zero dependencies)
            ↑
       Infrastructure (implements interfaces)
```

- **Domain**: Pure C# — `TaskItem`, `AppUser`, `Result<T>`, domain events. No `using` for frameworks.
- **Application**: Defines `ITaskUoW`, `IUserUoW`, `IRepository<T>`, `ITaskRepository`, `IUserRepository`, `IIdentityProviderClient`. References only Domain.
- **Infrastructure**: Implements all interfaces with raw ADO.NET (Npgsql `IDbConnection`) and Keycloak HTTP client.
- **WebApi**: Composes layers via DI. Controllers delegate to Application services.

### Testing Strategy

**Status:** ✅ Fully Implemented

| Project | Scope | Dependencies |
|---|---|---|
| `Domain.UnitTests` | Entity invariants, factory methods, status transitions | None (pure domain) |
| `Application.UnitTests` | Service logic, CRUD flows, authorization | In-memory UoW doubles |
| `Infra.IntegrationTests` | SQL schema, seeds, repository behavior | Testcontainers PostgreSQL |
| `WebApi.IntegrationTests` | Full endpoint coverage, auth/ownership | WebApplicationFactory + Testcontainers |

### Database Design

**Status:** ✅ Exceeds Requirements

**TaskItem** (Primary key + 7 additional fields):
- `Id` (PK, UUID)
- `Title` (required)
- `Description` (optional)
- `Status` (Pending / InProgress / Completed)
- `DueDate` (optional, validated as future date)
- `UserId` (FK → AppUser)
- `CreatedAt`, `UpdatedAt`

**AppUser** (Primary key + 5 additional fields):
- `Id` (PK, UUID)
- `ExternalSubject` (Keycloak user ID)
- `Username`, `Email`
- `Role` (User / Admin)
- `CreatedAt`, `LastSeenAt`

### Business Logic Layer

**Status:** ✅ Fully Implemented

Business rules live in **Domain entities** (validation, invariants) and **Application services** (orchestration, ownership):

- `TaskItem.Create()` — validates title, due date
- `TaskItem.ChangeStatus()` — enforces valid state transitions and ownership
- `TaskItem.Delete()` — ownership check
- `TaskService` — orchestrates UoW, enforces admin-vs-user access

All methods return `Result<T>` for explicit error handling.

### Frontend

**Status:** ✅ Fully Implemented

- **Framework**: Angular 19 with Tailwind CSS 4
- **UI**: Kanban board with three columns (Pending, In Progress, Completed)
- **Auth**: OIDC via `angular-auth-oidc-client`
- **State**: RxJS-based services (`ApiService`, `AuthService`)
- **Responsive**: Mobile-first Tailwind CSS breakpoints

---

## Beyond Requirements

The project implements several features that go beyond the specification:

| Feature | Spec Requirement | Actual Implementation |
|---|---|---|
| **Keycloak OIDC** | "Create and authenticate users" | Full OpenID Connect with enterprise IdP, social login-ready |
| **Docker Compose** | "A database" | Complete 4-service orchestrated deployment |
| **Kanban Board** | "Frontend with CRUD" | Interactive drag-and-drop Kanban UI |
| **Result\<T\> Pattern** | (not required) | Functional error handling without exceptions |
| **Domain Events** | (not required) | Event-driven architecture within UoW boundary |
| **Identity Mirroring** | (not required) | Keycloak ↔ local AppUser sync for relational queries |
| **Testcontainers** | "Unit tests" | Real database integration tests with containerized PostgreSQL |
| **Auto-Migration** | (not required) | `DatabaseMigrationHostedService` applies idempotent SQL schema on startup |
| **Nginx Reverse Proxy** | (not required) | Single entry point with path-based routing |
| **Custom Keycloak Theme** | (not required) | Branded login page matching the app |
