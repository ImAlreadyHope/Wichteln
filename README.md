# Wichteln

A small Christmas gift-exchange (Wichteln) app for Justin, Luana, Lydia, Angelika and Denis.

- **Login by personal link.** Each person gets a link like `https://wichteln.devilbots.dev/?login=…`. No passwords.
- **Wishlists.** Everyone can add, edit and delete their own wishes (with an optional link) until the unlock date.
- **The draw.** From **20 November 2026, 00:00 (German time)** the "Zeige meinen Wichtel" button unlocks. Flocke spins a reel through the others' avatars and lands on your Wichtel. After that you only see that person and their wishlist.
- **Rules.** Nobody draws themselves, every person is drawn exactly once (also enforced by a unique key in the database), and Justin ↔ Luana and Angelika ↔ Denis can never draw each other. The server draws once for everyone on the first reveal, picking uniformly among all valid combinations.

## Projects

| Folder | What |
| --- | --- |
| `src/Wichteln.Client` | Blazor WebAssembly site (static files, hosted on Cybrancee/Plesk) |
| `src/Wichteln.Api` | ASP.NET Core 8 minimal API with EF Core + MySQL (Docker on the IONOS VPS behind the shared Caddy) |
| `src/Wichteln.Shared` | DTOs and wish validation shared by both |
| `tests/Wichteln.Tests` | Draw-rule tests and API tests (wishlist freeze, reveal, uniqueness) |
| `deploy/` | Dockerfile, docker-compose, Caddy site block, `deploy.sh`, `.env.example` |
| `design/` | The design system: tokens, components CSS, Flocke mascot and icons |

## Run locally

Needs the .NET 8 SDK. Locally the API uses SQLite, so no MySQL is needed.

```bash
dotnet run --project src/Wichteln.Api      # http://localhost:5100
dotnet run --project src/Wichteln.Client   # http://localhost:5200 (separate terminal)
curl -H "X-Admin-Key: dev-admin-key" http://localhost:5100/admin/links   # personal links
dotnet test
```

To try the reveal locally, start the API with an earlier date: `Wichteln__UnlockAt=2026-11-01T00:00:00+01:00 dotnet run --project src/Wichteln.Api`.

## Deploy

```
Browser ──HTTPS──> wichteln.devilbots.dev (Plesk: Blazor client)
                        │  fetch + Bearer login token
                        ▼
            wichteln-api.devilbots.dev (IONOS VPS: shared Caddy ──> wichteln-api container)
                        │
                        ▼
            hosted MariaDB (devilbots.dev:3306)
```

### 1. Database on Plesk

1. In Plesk, **Databases → Add Database**: `devilbot_wichteln` with a user of the same name.
2. Allow **remote access** for that user from the VPS IP `217.154.17.113` (like the Momo database).
3. The API creates its tables on first start.

### 2. API on the IONOS VPS (Docker + shared Caddy)

Same setup as Momo Home: the API lives in `/opt/apps/wichteln`, joins the shared `edge` network, and the shared Caddy
(`/opt/apps/caddy/Caddyfile`) gets the site block from `deploy/Caddyfile.site`.

One-time setup:

1. DNS: an A record `wichteln-api.devilbots.dev` → `217.154.17.113`.
2. On the VPS, create `/opt/apps/wichteln/deploy/.env` from `deploy/.env.example` (database password, `ADMIN_KEY` from `openssl rand -hex 32`).
3. Append `deploy/Caddyfile.site` to `/opt/apps/caddy/Caddyfile` and reload Caddy.

Deploy or update from the dev PC (Git Bash), with the same SSH key as Momo (`~/.ssh/momo_ionos`):

```bash
deploy/deploy.sh root@217.154.17.113
curl https://wichteln-api.devilbots.dev/health
```

Useful on the VPS:

```bash
cd /opt/apps/wichteln/deploy
docker compose ps
docker compose logs -f wichteln-api
```

Get everyone's personal links:

```bash
curl -H "X-Admin-Key: <ADMIN_KEY>" https://wichteln-api.devilbots.dev/admin/links
```

Send each person only their own link.

Admin endpoints (all need the `X-Admin-Key` header):

| Endpoint | Does |
| --- | --- |
| `GET /admin/links` | Lists each person's link, wish count and whether they revealed |
| `POST /admin/new-link/{name}` | Gives one person a new link (the old one stops working) |
| `POST /admin/reset-draw` | Deletes the draw and every reveal, for example after a test run |

### 3. Blazor site on Plesk (wichteln.devilbots.dev)

1. The API address is set in `src/Wichteln.Client/wwwroot/appsettings.json` (`ApiBaseUrl`, with a trailing slash).
2. Publish:
   ```bash
   dotnet workload install wasm-tools   # optional, smaller and faster build
   dotnet publish src/Wichteln.Client -c Release -o publish/client
   ```
3. Upload the **contents** of `publish/client/wwwroot` (including the hidden `.htaccess`) to the domain's `httpdocs` folder with the Plesk File Manager or FTP.
4. `.htaccess` sets the WebAssembly MIME types, serves the pre-compressed Brotli files and sends unknown paths to `index.html`. If your Plesk runs on Windows/IIS, use `publish/client/web.config` instead.

The CI workflow builds and tests every push and attaches the published site as an artifact.

### Automatic deploys

`.github/workflows/deploy.yml` deploys `main` after CI is green: first the API (`deploy/deploy.sh` over SSH), then the client
(FTPS into the site's document root). It skips each part whose secrets are missing. Repository secrets, as in Momo Home:

| Secret | Value |
|---|---|
| `VPS_SSH_KEY` | Private key that can log in to the VPS (the Momo deploy key works) |
| `VPS_HOST` | `root@217.154.17.113` |
| `VPS_KNOWN_HOSTS` | Optional: output of `ssh-keyscan 217.154.17.113` |
| `PLESK_FTP_SERVER`, `PLESK_FTP_USER`, `PLESK_FTP_PASSWORD` | An FTP account for wichteln.devilbots.dev |

If the FTP account opens one level above the document root, add the variable `PLESK_FTP_DIR` = `httpdocs/`.

## Photos (optional)

Put square photos in `src/Wichteln.Client/wwwroot/img/people/` as `justin.jpg`, `luana.jpg` and so on, and list the names under `"Photos"` in `wwwroot/appsettings.json`. Without a photo, the avatar shows the person's initial.

## Settings

| Setting (env var on the API) | Default |
| --- | --- |
| `Wichteln__UnlockAt` | `2026-11-20T00:00:00+01:00` |
| `Wichteln__People__0..4` | Justin, Luana, Lydia, Angelika, Denis |
| `Wichteln__ForbiddenPairs__0__0` / `__0__1` … | Justin/Luana, Angelika/Denis |
| `Wichteln__AllowedOrigins__0` | the site's address (CORS) |
