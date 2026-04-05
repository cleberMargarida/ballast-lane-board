---
agent: agent
description: Create a full DocFX documentation site for the Ballast Lane Board project, modeled after
model: [Claude Opus 4.6 (copilot), GPT-5.4 (copilot)]
tools: [execute, read, edit, search, web, agent, todo, agent/runSubagent, io.github.chromedevtools/chrome-devtools-mcp/*, github/*]
---

# Plan: DocFX Documentation for Ballast Lane Board

## TL;DR

Create a full DocFX documentation site for the Ballast Lane Board project, modeled after the RealEstateListing reference repo's structure. The documentation will cover architecture, API endpoints, deployment, getting started, a specification compliance matrix, and auto-generated API reference from XML docs. Implementation uses parallel subagents — one per documentation area — to maximize throughput.

## Reference Structure (from RealEstateListing)

```
docs/
├── docfx.json              # DocFX config (metadata + build)
├── toc.yml                 # Top-level TOC (Docs + API Reference)
├── index.md                # Landing page (hero, badges, features)
├── filterConfig.yml        # API namespace filter
├── architecture.md         # (redirects to architecture/index.md)
├── favicon-32.png          # Favicon
├── docs/
│   └── toc.yml             # Nested TOC for all doc sections
├── getting-started/
│   ├── index.md            # Overview + key features
│   ├── installation.md     # Prerequisites + setup
│   └── quick-start.md      # Run & first request
├── architecture/
│   └── index.md            # Layers, Mermaid diagrams, decisions
├── api-endpoints/
│   ├── tasks.md            # Tasks CRUD endpoints
│   └── auth.md             # Auth endpoints
├── deployment/
│   └── docker.md           # Docker Compose deployment
├── specification/
│   └── index.md            # SPECIFICATION.md compliance matrix
└── api/                    # Auto-generated from XML comments
```

---

## Phase 1: DocFX Scaffolding

**Steps** (sequential — these are foundational):

1. **Create `docs/docfx.json`** — Configure metadata extraction from all 4 .csproj files (Domain, Application, Infra, WebApi), build output to `../bin/docs`, modern template, global metadata (app title "Ballast Lane Board", footer, git contribute link). Target framework `net10.0`. Reference: RealEstateListing `docfx.json`.

2. **Create `docs/filterConfig.yml`** — Exclude `System.*`, `Microsoft.*`, compiler-generated, and `GeneratedCode` attributed types. Include only `BallastLaneBoard.*` namespaces.

3. **Create `docs/toc.yml`** — Top-level navigation: `Docs` (href: docs/) + `API Reference` (href: api/).

4. **Create `docs/docs/toc.yml`** — Nested TOC linking all documentation sections:
   - Getting Started (Overview, Installation, Quick Start)
   - Architecture (Overview)
   - API Endpoints (Tasks, Auth)
   - Deployment (Docker)
   - Specification Compliance (Overview)

**Relevant files to create:**
- `docs/docfx.json`
- `docs/toc.yml`
- `docs/docs/toc.yml`
- `docs/filterConfig.yml`

---

## Phase 2: Documentation Content (Parallel Subagents)

Five subagents run in parallel, each writing a self-contained documentation area:

### Subagent A: Landing Page + Getting Started
**Creates:**
- `docs/index.md` — Hero section with project name, badges-like feature cards (Clean Architecture, Keycloak Auth, Kanban UI, Docker Ready), CTA buttons to Getting Started and API Reference (Swagger).
- `docs/getting-started/index.md` — Overview: what is Ballast Lane Board, key features list, architecture summary, next steps links.
- `docs/getting-started/installation.md` — Prerequisites (.NET 10 SDK, Node.js 20+, Docker, Angular CLI optional), clone repo, restore dependencies, environment setup.
- `docs/getting-started/quick-start.md` — `docker compose up` instructions, demo credentials (admin/testuser from Keycloak seed), first API calls with curl, accessing Swagger and the Angular SPA.

**Tone**: Friendly, welcoming. Emojis for section headers (🚀, 🏗️, etc.).

### Subagent B: Architecture
**Creates:**
- `docs/architecture/index.md` — Full architecture doc with:
  - **Layered Architecture** Mermaid flowchart (Presentation → Application → Domain ← Infrastructure)
  - **Layer Responsibilities** for each layer with key classes
  - **Dependency Flow** diagram
  - **Bounded Contexts** (TaskManagement + Identity)
  - **Key Patterns**: Result pattern (railway-oriented), Aggregate Root, Repository + UoW, Identity Mirroring (Keycloak ↔ local AppUser)
  - **Deployment Architecture** Mermaid diagram (nginx → api → postgres, nginx → keycloak)
  - **Project Structure** tree
  - **Key Design Decisions** section explaining WHY each choice was made
  - **Testing Strategy** summary with Mermaid diagram

**Mermaid diagrams required:**
1. Clean Architecture layers (flowchart TB)
2. Docker deployment topology (flowchart LR: nginx, api, keycloak, postgres)
3. Auth flow sequence diagram (Browser → SPA → Keycloak → API)
4. Task status state machine (stateDiagram-v2: Pending → InProgress → Completed)

### Subagent C: API Endpoints
**Creates:**
- `docs/api-endpoints/tasks.md` — Full Tasks API documentation:
  - Endpoints table (GET/POST/PUT/PATCH/DELETE with paths)
  - Response codes table
  - Task status state diagram (Mermaid)
  - Request/Response examples for each endpoint
  - Authorization notes (all endpoints require Bearer token)
  - Admin vs User behavior (admin sees all, user sees own)

- `docs/api-endpoints/auth.md` — Auth API documentation:
  - POST /api/auth/register (AllowAnonymous)
  - GET /api/auth/me (Authorized)
  - POST /api/auth/sync (Authorized)
  - OIDC flow explanation (login via Keycloak, not via API)
  - Request/Response examples

### Subagent D: Deployment
**Creates:**
- `docs/deployment/docker.md` — Docker Compose deployment guide:
  - Architecture Mermaid diagram (4 services: nginx, api, keycloak, postgres on ballast-lane-net)
  - Services table with ports, images, roles
  - Environment variables table
  - Step-by-step: clone, configure .env, docker compose up
  - Network topology (static IPs on 172.31.240.0/24)
  - Health checks explanation
  - Keycloak realm auto-import
  - Auto-migration on startup (DatabaseMigrationHostedService)
  - SPA hosting from wwwroot
  - Troubleshooting section
  - Nginx reverse proxy explanation (routes /api/* to api, /realms/* to keycloak)

### Subagent E: Specification Compliance
**Creates:**
- `docs/specification/index.md` — Dedicated section mapping every SPECIFICATION.md requirement to how the project implements it:

  | Requirement | Implementation | Key Files |
  |---|---|---|
  | CRUD operations | TasksController with full REST | Controllers/TasksController.cs |
  | User creation & auth | Keycloak + AuthController | Controllers/AuthController.cs, KeycloakAdminClient |
  | Clean Architecture | 4-layer separation | Domain/Application/Infra/WebApi projects |
  | TDD | xUnit unit + integration tests | tests/ folder |
  | No EF/Dapper/Mediator | ⚠️ Uses EF Core (deliberate decision) | Infra/EntityFrameworkCore/ |
  | Database with PK + 2 fields | TaskItem (Id, Title, Description, Status, DueDate, UserId) | TaskItem.cs |
  | Business logic layer | Application services + Domain entities | TaskService.cs, TaskItem.cs |
  | Frontend with CRUD | Angular 19 Kanban board | ClientApp/ |
  | Responsive UI | Tailwind CSS | tailwind.compiled.css |
  | State management | Service-based with RxJS | api.service.ts, auth.service.ts |
  | Task fields (title, desc, status, due_date) | All present in TaskItem | TaskItem.cs |
  | Tasks associated with user | UserId on TaskItem + ownership checks | TaskItem.cs, TaskService.cs |
  | README with setup | README.md exists | README.md |
  | Seeded data + demo credentials | DB seeds + Keycloak realm import | UserUoW.cs, ballast-lane-board-realm.json |

  Include explanations of WHY decisions were made (especially the EF Core constraint violation — explain the reasoning).

---

## Phase 3: API Reference Setup (sequential, depends on Phase 1)

5. **Ensure XML documentation is enabled** — Verify `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in Directory.Build.props or each .csproj. DocFX needs XML docs to generate the API reference.

6. **Add `docs/architecture.md`** — Redirect/summary file at docs root (mirrors reference repo pattern), linking to `architecture/index.md`.

---

## Phase 4: Verification

**Automated:**
1. Run `dotnet tool install -g docfx` (if not installed)
2. Run `docfx docs/docfx.json --serve` from repo root — verify site builds without errors
3. Verify API reference is generated under `bin/docs/api/`
4. Verify all TOC links resolve (no broken hrefs)

**Manual:**
1. Navigate landing page — confirm hero, feature cards, CTA buttons render
2. Navigate each docs section — confirm Mermaid diagrams render correctly
3. Navigate API Reference — confirm BallastLaneBoard namespace classes appear
4. Verify filterConfig excludes System/Microsoft namespaces
5. Check specification compliance page — all SPECIFICATION.md items are mapped

---

## Decisions

- **DocFX version**: Use latest DocFX (v2.x modern template) to match reference repo
- **No Azure deployment docs**: Project uses Docker Compose only (no CI/CD or Azure); deployment section covers Docker only
- **EF Core constraint**: SPECIFICATION.md says "do NOT use Entity Framework" — the project uses it. The specification compliance section will explicitly acknowledge and explain this decision
- **API filter**: Only include `BallastLaneBoard.*` namespaces in generated API reference
- **Output path**: `../bin/docs` (same pattern as reference)
- **Parallel subagent wave**: Subagents A-E are independent (no cross-references between content files) and can run simultaneously. Phase 1 scaffold must complete first since subagents need the folder structure context.
- **Mermaid over static images**: All diagrams use Mermaid for maintainability (DocFX modern template supports Mermaid natively)

## Relevant files

- `docs/docfx.json` — DocFX configuration (create new, based on reference repo pattern)
- `docs/toc.yml` — Top-level navigation (create new)
- `docs/docs/toc.yml` — Docs section navigation (create new)
- `docs/filterConfig.yml` — API namespace filter (create new)
- `docs/index.md` — Landing page (create new)
- `docs/architecture.md` — Architecture redirect (create new)
- `docs/getting-started/{index,installation,quick-start}.md` — Getting started docs (create new)
- `docs/architecture/index.md` — Architecture documentation (create new)
- `docs/api-endpoints/{tasks,auth}.md` — API endpoint docs (create new)
- `docs/deployment/docker.md` — Docker deployment guide (create new)
- `docs/specification/index.md` — Specification compliance matrix (create new)
- `src/BallastLaneBoard.WebApi/Controllers/TasksController.cs` — Reference for API docs
- `src/BallastLaneBoard.WebApi/Controllers/AuthController.cs` — Reference for Auth docs
- `src/BallastLaneBoard.Domain/TaskManagement/TaskItem.cs` — Reference for domain docs
- `src/BallastLaneBoard.Domain/Identity/AppUser.cs` — Reference for identity docs
- `docker-compose.yml` — Reference for deployment docs
- `nginx/nginx.conf` — Reference for reverse proxy docs
- `SPECIFICATION.md` — Source for compliance matrix
- `Directory.Build.props` — May need XML doc generation enabled

---
After completing the documentation, run docfx serve and using #io.github.chromedevtools/chrome-devtools-mcp/* tool, verify that all pages render correctly, Mermaid diagrams display, and API reference is generated without errors. Check for broken links in TOC and content. Finally, review the specification compliance matrix for accuracy and completeness.