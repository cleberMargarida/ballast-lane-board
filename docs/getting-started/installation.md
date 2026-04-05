# 📦 Installation

## Prerequisites

| Tool | Version | Link |
|---|---|---|
| .NET SDK | 10.0+ | [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) |
| Node.js | 20+ | [nodejs.org](https://nodejs.org/) |
| Docker Desktop | Latest | [docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop) |
| Angular CLI | *(optional)* | `npm install -g @angular/cli` |

---

## Clone the Repository

```bash
git clone https://github.com/user/ballast-lane-board.git
cd ballast-lane-board
```

---

## Restore Dependencies

### .NET

```bash
dotnet restore
```

### Angular (optional — only needed for local dev with hot reload)

```bash
cd src/BallastLaneBoard.ClientApp
npm install
cd ../..
```

---

## Environment Setup

### Docker Compose Variables

Create a `.env` file in the repository root with the following variables:

```env
POSTGRES_PASSWORD=your_postgres_password
KEYCLOAK_DB_PASSWORD=keycloak
KEYCLOAK_ADMIN_PASSWORD=admin
API_ADMIN_PASSWORD=admin
KEYCLOAK_HOSTNAME=localhost:3000
GATEWAY_PORT=3000
```

| Variable | Description |
|---|---|
| `POSTGRES_PASSWORD` | PostgreSQL superuser password |
| `KEYCLOAK_DB_PASSWORD` | Password for the Keycloak database user |
| `KEYCLOAK_ADMIN_PASSWORD` | Keycloak admin console password |
| `API_ADMIN_PASSWORD` | API's Keycloak admin client password |
| `KEYCLOAK_HOSTNAME` | Externally reachable Keycloak hostname (e.g., `localhost:3000`) |
| `GATEWAY_PORT` | Port exposed by the Nginx gateway |

> [!NOTE]
> The `.env` file should **not** be committed to version control.

---

## Next Step

- [⚡ Quick Start](quick-start.md) — Run the application and make your first request
