# ⚡ Quick Start

## 🐳 Option 1: Docker Compose (Full Stack)

The fastest way to run the entire application:

```bash
docker compose up -d
```

This starts **PostgreSQL**, **Keycloak**, the **.NET API** (with Angular SPA), and **Nginx** as a reverse proxy.

Access the application at `http://localhost:3000` (or your configured `GATEWAY_PORT`).

---

## 🛠️ Option 2: Local Development

### 1. Start infrastructure only

```bash
docker compose up -d db keycloak
```

This starts PostgreSQL and Keycloak in Docker.

### 2. Run the API

```bash
dotnet run --project src/BallastLaneBoard.WebApi
```

The API starts at `https://localhost:7160` / `http://localhost:5293`. EF Core migrations run automatically on startup.

### 3. (Optional) Angular hot reload

If you want live reload during frontend development:

```bash
cd src/BallastLaneBoard.ClientApp
npm start
```

Angular dev server runs at `http://localhost:4200`, proxying API calls to the backend.

> [!TIP]
> Set `Spa:UseProxyInDevelopment=true` in the Web API configuration to use the Angular proxy.
> Otherwise, the Web API serves the prebuilt SPA from `wwwroot/` by default.

---

## 🔑 Demo Credentials

| User | Password | Role | Access |
|---|---|---|---|
| `admin` | `admin` | Admin + User | Can see and manage **all** tasks |
| `testuser` | `password` | User | Can see and manage **own** tasks only |

Sign in via the Keycloak login page (the SPA redirects automatically).

---

## 📡 First API Calls

### Register a new user

```bash
curl -X POST http://localhost:5293/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username": "newuser", "email": "new@example.com", "password": "secret123"}'
```

### Get tasks (requires Bearer token)

```bash
# First, obtain a token from Keycloak
TOKEN=$(curl -s -X POST "http://localhost:8080/realms/ballast-lane-board/protocol/openid-connect/token" \
  -d "grant_type=password&client_id=ballast-lane-board-spa&username=admin&password=admin" \
  | jq -r '.access_token')

# Then call the API
curl http://localhost:5293/api/tasks \
  -H "Authorization: Bearer $TOKEN"
```

### Create a task

```bash
curl -X POST http://localhost:5293/api/tasks \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title": "My First Task", "description": "Hello from the API!", "dueDate": "2026-12-31T00:00:00Z"}'
```

---

## 🌐 Access Points

| Service | URL | Notes |
|---|---|---|
| **Application** | `http://localhost:3000` | Full stack via Nginx (Docker Compose) |
| **Swagger UI** | `http://localhost:5293/swagger` | API docs with dark theme + Bearer auth |
| **Keycloak Console** | `http://localhost:3000/admin` | Admin: `admin` / `{KEYCLOAK_ADMIN_PASSWORD}` |
| **Angular Dev Server** | `http://localhost:4200` | Only when running `npm start` separately |

---

## Next Steps

- [🏛️ Architecture Overview](../architecture/index.md) — Understand the design
- [📡 Tasks API](../api-endpoints/tasks.md) — Full endpoint reference
- [🐳 Docker Deployment](../deployment/docker.md) — Production deployment guide
