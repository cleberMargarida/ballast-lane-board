## Plan: Full-Stack Task Management Scaffold

Rebuild the .NET 10 console starter into a full Clean Architecture solution with Angular + Tailwind, PostgreSQL, Keycloak OIDC, JWT role-based API protection, Swagger (dark theme), and xUnit v3 tests. Enforce the flow **Controller → ApplicationService → Domain** with Infrastructure wired only at the WebApi composition root.

---

### Phase 1 — Solution Restructure *(blocks all other phases)*

1. **Delete the old console project** ([ballast-lane-board/](ballast-lane-board/)) and replace the [ballast-lane-board.slnx](ballast-lane-board.slnx) with the new `src/tests` layout.
2. **Create source projects** under `src/`:
   - `BallastLaneBoard.Domain` — no dependencies
   - `BallastLaneBoard.Application` → depends on Domain
   - `BallastLaneBoard.Infra` → depends on Domain + Application abstractions
   - `BallastLaneBoard.WebApi` → depends on Application + Infra (composition root)
   - `BallastLaneBoard.ClientApp` — Angular SPA, hosted by WebApi via SPA middleware
3. **Create test projects** under `tests/`:
   - `BallastLaneBoard.Domain.UnitTests`
   - `BallastLaneBoard.Application.UnitTests`
   - `BallastLaneBoard.Infra.IntegrationTests`
   - `BallastLaneBoard.WebApi.IntegrationTests`
4. All projects target `net10.0`, nullable enabled, namespace root `BallastLaneBoard`.

### Phase 2 — Domain & Application Contracts *(parallel with Phase 3 container setup)*

5. **Domain entities**: `TaskItem` (title, description, status enum, due_date, userId, audit timestamps) and `AppUser` (app ID, Keycloak subject, username, email, role snapshot, created/last-seen). Business rules: required title, valid status transitions, ownership enforcement, due-date validation.
6. **Application layer**: DTOs, request/response models, repository interfaces (`ITaskRepository`, `IUserRepository`), and application services for task CRUD, current-user sync, and registration. No direct DB access — everything behind interfaces.

### Phase 3 — Infrastructure & Keycloak *(depends on Phase 1; container setup parallel with Phase 2)*

7. **PostgreSQL data access** using **Entity Framework Core** with the dot-bet UoW-per-bounded-context pattern. Implement a `DbContextUow<TEntity, TDomainEvent>` abstract base (EF DbContext + `IUnitOfWork`), then two concrete UoWs: `TaskUoW : DbContextUow<TaskItem, TaskEvent>, ITaskUoW` (exposes `IRepository<TaskItem>` + `IProducer<TaskEvent>`) and `UserUoW : DbContextUow<AppUser, UserEvent>, IUserUoW` (exposes `IRepository<AppUser>` + `IProducer<UserEvent>`). App tables live in the `app` schema; outbox/event tables also land in `app`. `EFRepository<T>` wraps `DbSet<T>` behind `IRepository<T>`. Seed data via EF `HasData` or an explicit migration seed.
8. **Single shared PostgreSQL container** for both Keycloak and the application. Keycloak connects to database `keycloak`; the app connects to database `ballastlaneboard` (separate databases on the same server, so schemas inside each DB remain clean). **Docker Compose**: one `postgres:17-alpine` service plus a `keycloak:26.2 start-dev --import-realm` service wired to the same host. Mount the Keycloak realm JSON and custom login theme. Admin credentials: `admin`/`admin`.
9. **Registration endpoint** creates users via the **Keycloak Admin API** then mirrors them locally into `UserUoW`. Login itself stays OIDC-delegated to Keycloak (no proxied credentials).
10. **JWT authorization mapping**: configure ASP.NET JWT bearer validation against Keycloak discovery metadata, map `realm_access.roles` claims to `ClaimTypes.Role` in `OnTokenValidated`, and define `User` and `Admin` authorization policies.

### Phase 4 — WebApi Host *(depends on Phases 2 & 3)*

11. **Composition root** in `Program.cs`: register controllers, application services, repositories, authentication, authorization, CORS/SPA proxy, exception handling, health checks, XML doc comments.
12. **Controllers**: `TasksController` (full CRUD, protected), `AuthController` (register, current-user, sync — public registration endpoint, protected user endpoints), `HealthController` (public). Correct HTTP verbs and status codes.
13. **OpenAPI/Swagger**: enable XML documentation export (`GenerateDocumentationFile`), wire Swagger with Bearer token injection support, apply a **dark theme** CSS override.
14. **SPA middleware**: serve Angular dev server in development, built assets in production — plug-and-play.

### Phase 5 — Angular Client *(depends on Phase 4; parallel with Phase 6)*

15. **Scaffold Angular app** under `src/BallastLaneBoard.ClientApp` with Tailwind CSS. Feature layout: `auth/`, `tasks/`, `shared/`, `core/`.
16. **OIDC integration**: authorization code + PKCE against Keycloak, route guards, HTTP bearer interceptor, logout, current-user bootstrap sync with WebApi.
17. **Task management UI**: list, create, edit, delete, status update. Responsive layout (desktop + mobile). Role-based UI hints where appropriate.

### Phase 6 — Testing *(depends on Phases 2–4; parallel with Phase 5)*

18. **xUnit v3, strict AAA** throughout.
19. **Unit tests**: Domain entity invariants (factory methods, command methods, `Result<T>` outcomes) + Application service logic using **in-memory UoW doubles** (`InMemoryTaskUoW`, `InMemoryUserUoW`) that implement `ITaskUoW`/`IUserUoW` against `List<T>` — no DB, no Testcontainers, fast by design. Mirror the dot-bet `InMemoryUnitOfWork` pattern.
20. **Integration tests**: `BallastLaneBoard.Infra.IntegrationTests` — EF Core UoW against a **Testcontainers** PostgreSQL instance, verifying real schema creation, seed, and repository behavior. `BallastLaneBoard.WebApi.IntegrationTests` — **WebApplicationFactory** + Testcontainers PostgreSQL, `AddInfraServices` swapped to test DB, controlled synthetic JWT for auth, full endpoint coverage.
21. **Auth/ownership tests**: anonymous → 401, wrong role → 403, cross-user task access → 403, valid CRUD → 200/201/204.

### Phase 7 — Documentation *(depends on all above)*

22. **README**: setup instructions, Docker prerequisites, seeded demo data, demo credentials (`admin`/`admin`), architecture rationale, command reference.
23. **GenAI exercise deliverable**: prompt used, generated code samples, validation narrative, corrections made, edge-case handling explanation.
24. **Informal user story** for presentation.

---

### Relevant Files

| Path | Purpose |
|---|---|
| [SPECIFICATION.md](SPECIFICATION.md) | Source requirements to validate against |
| [ballast-lane-board.slnx](ballast-lane-board.slnx) | Replace with new multi-project solution graph |
| [ballast-lane-board/](ballast-lane-board/) | Retire (old console app) |
| `src/BallastLaneBoard.WebApi/` | ASP.NET host, controllers, Swagger, SPA middleware |
| `src/BallastLaneBoard.Application/` | Services, DTOs, repository contracts |
| `src/BallastLaneBoard.Domain/` | Entities, enums, business rules |
| `src/BallastLaneBoard.Infra/` | EF Core `DbContextUow<>` base, `TaskUoW`, `UserUoW`, `EFRepository<T>`, Keycloak Admin API client, EF migrations |
| `src/BallastLaneBoard.ClientApp/` | Angular SPA with Tailwind + OIDC |
| `tests/BallastLaneBoard.*.UnitTests/` | xUnit v3 unit tests |
| `tests/BallastLaneBoard.*.IntegrationTests/` | xUnit v3 + WebApplicationFactory + Testcontainers |

---

### Verification

1. `dotnet build` — full solution compiles cleanly with nullable enabled
2. `docker compose up` — single PostgreSQL container starts; Keycloak connects to `keycloak` DB, app connects to `ballastlaneboard` DB; realm imports; custom theme loads; `admin`/`admin` works
3. EF Core migrations run on startup; seed data populates Users + Tasks tables in `ballastlaneboard` DB
4. `dotnet test` — unit tests pass (in-memory UoW, no containers); Infra + WebApi integration tests pass (Testcontainers PostgreSQL)
5. `dotnet run` on WebApi — Angular SPA loads through SPA middleware (plug-and-play)
6. Sign in via branded Keycloak → token acquired → protected API calls succeed, anonymous calls rejected
7. Swagger UI: dark theme, bearer token injection, XML doc comments visible on all endpoints
8. Ownership: user A cannot edit/delete user B's tasks
9. README + presentation materials cover user story, seeded data, demo creds, architecture, and GenAI narrative

---

### Key Decisions

- **Shared PostgreSQL** — one `postgres:17-alpine` container; Keycloak uses database `keycloak`, the app uses database `ballastlaneboard`. Different databases on the same server keeps isolation without a second container.
- **Entity Framework Core + dot-bet UoW pattern** — `DbContextUow<TEntity, TDomainEvent>` abstract base; one concrete UoW per bounded context (`TaskUoW`, `UserUoW`); `EFRepository<T>` wraps `DbSet<T>`; outbox `IProducer<TEvent>` via EF topic. No raw ADO.NET, no Dapper.
- **Result\<T\> functional error handling** — all domain factory and command methods return `Result<T>` (IsSuccess / IsFailed / Value / Error). Application services propagate failures directly to controllers: `result.IsSuccess ? Ok() : BadRequest(result.Error)`.
- **Control flow follows dot-bet exactly**:
  ```
  HTTP Request
      ↓
  [Controller](appService)
      ↓ HttpGet/Post/Put/Delete
  [ApplicationService](ITaskUoW | IUserUoW)
      ↓ orchestrates
  [Domain Aggregate].Create() → Result<ICreated<Aggregate, Event>>
  [Domain Aggregate].Command() → Result<DomainEvent> | Result
      ↓ on success
  uow.Repository.Add(entity) + uow.Events.Produce(event) + await uow.Commit()
      ↓
  HTTP Response (IsSuccess ? Ok/Created : BadRequest(error))
  ```
- **DI wiring** — `AddApplicationServices()` extension on `IServiceCollection` in Application layer registers all `*Service` scoped. `AddInfraServices(connectionString)` extension in Infra layer registers all `*UoW` DbContexts and their interfaces. WebApi `Program.cs` calls both.
- **Keycloak** owns login; app mirrors users locally via `UserUoW` to satisfy the required Users table and enforce task ownership.
- **Registration** goes through Keycloak Admin API → local user mirror. Browser login stays OIDC-delegated (authorization code + PKCE).
- **Custom Keycloak theme** for branded login (actual Keycloak page, not just Angular styling).
- **In-memory UoW doubles** (`InMemoryTaskUoW`, `InMemoryUserUoW`) for all unit and application-service tests — fast, no containers, direct `List<T>` backed. Testcontainers reserved for Infra and WebApi integration tests only.
