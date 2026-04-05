---
name: sync-keycloak-hosts
description: 'Sync Keycloak SPA client URIs for a custom hostname. USE FOR: configure host, change hostname, fix redirect_uri, fix post_logout_redirect_uri, sync keycloak URIs, custom domain, CORS error after host change. DO NOT USE FOR: Keycloak realm creation, user management, role mapping.'
argument-hint: 'Hostname to configure, e.g. ballastlane.board.local:8082'
---

# Sync Keycloak SPA Client URIs

Update the live **ballast-lane-board-spa** Keycloak client so `redirectUris`, `webOrigins`, and `post.logout.redirect.uris` match a given gateway hostname, then update `appsettings.Development.json` so the .NET API reaches Keycloak through the same gateway.

## When to Use

- Switching to a custom hostname (e.g. `ballastlane.board.local:8082`)
- Keycloak returns **"Invalid parameter: redirect_uri"** or **"Invalid redirect uri"**
- Post-logout redirect fails with a Keycloak error page
- CORS errors because `webOrigins` doesn't include the current host
- After recreating the Docker volume and the live DB drifts from the realm JSON

## Inputs

| Input | Source | Fallback |
|-------|--------|----------|
| **Hostname** | User argument (e.g. `myhost:8082`) | `KEYCLOAK_HOSTNAME` from [.env](../../.env) |
| **Admin password** | `KEYCLOAK_ADMIN_PASSWORD` in [.env](../../.env) | `admin` |

The scheme is always `http://`.

## Procedure

### 1. Read `.env`

Parse [.env](../../.env) to get `KEYCLOAK_HOSTNAME`, `GATEWAY_PORT`, and `KEYCLOAK_ADMIN_PASSWORD`.

### 2. Resolve gateway

If the user supplied a hostname argument, use it. Otherwise fall back to `KEYCLOAK_HOSTNAME`.
The gateway URL is `http://<hostname>`.

### 3. Authenticate

```
POST http://<gateway>/realms/master/protocol/openid-connect/token
  grant_type=password
  client_id=admin-cli
  username=admin
  password=<admin-password>
```

Use `Invoke-RestMethod` (project runs on Windows/PowerShell).

### 4. Find the SPA client

```
GET /admin/realms/ballast-lane-board/clients
```

Filter by `clientId -eq "ballast-lane-board-spa"`. Note the internal UUID (`id`).

### 5. Build URI lists

Always include these dev origins **plus** the gateway:

| Label | Origin |
|-------|--------|
| Kestrel HTTP | `http://localhost:5293` |
| Kestrel HTTPS | `https://localhost:7160` |
| ng serve | `http://localhost:4200` |
| Gateway | `http://<hostname>` |
| Bare localhost | `http://localhost` |

Derive from these:

- **redirectUris** — each origin + `/*`
- **webOrigins** — each origin as-is
- **post.logout.redirect.uris** — each origin *and* each origin + `/*`, joined with `##`

### 6. Update the client

```
PUT /admin/realms/ballast-lane-board/clients/<uuid>
```

Set `redirectUris`, `webOrigins`, and `attributes."post.logout.redirect.uris"` on the fetched client object, then PUT the full JSON back.

### 7. Verify

Re-fetch the client and print `redirectUris`, `webOrigins`, and `post.logout.redirect.uris` so the user can confirm.

### 8. Update appsettings

Edit [appsettings.Development.json](../../src/BallastLaneBoard.WebApi/appsettings.Development.json):

- `OpenIdConnect.Authority` → `http://<hostname>/realms/ballast-lane-board`
- `IdentityProvider.AdminUrl` → `http://<hostname>`

### 9. Notify

Tell the user: **log out and log back in** so fresh tokens pick up the new issuer.

## Important

- The admin token expires quickly — get a fresh one if any request returns 401.
- Do **not** modify [ballast-lane-board-realm.json](../../keycloak/ballast-lane-board-realm.json) — it's only read on first DB creation.
- The docker-compose config is in [docker-compose.yml](../../docker-compose.yml) — the `KC_HOSTNAME` env var in there controls what Keycloak considers its public hostname; this skill only patches the client URIs, not the Keycloak server config.
