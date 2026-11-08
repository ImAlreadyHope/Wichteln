# Wichteln

A small Christmas gift-exchange (Wichteln) app for Justin, Luana, Lydia, Angelika and Denis.

- **Login by personal link.** Each person gets a link like `https://wichteln.example.de/?login=…`. No passwords.
- **Wishlists.** Everyone can add, edit and delete their own wishes (with an optional link) until the unlock date.
- **The draw.** From **20 November 2026, 00:00 (German time)** the "Zeige meinen Wichtel" button unlocks. Flocke spins a reel through the others' avatars and lands on your Wichtel. After that you only see that person and their wishlist.
- **Rules.** Nobody draws themselves, every person is drawn exactly once (also enforced by a unique key in the database), and Justin ↔ Luana and Angelika ↔ Denis can never draw each other. The server draws once for everyone on the first reveal, picking uniformly among all valid combinations.

## Projects

| Folder | What |
| --- | --- |
| `src/Wichteln.Client` | Blazor WebAssembly site (static files, hosted on Cybrancee/Plesk) |
| `src/Wichteln.Api` | ASP.NET Core 8 minimal API with EF Core + MySQL (Docker on the IONOS VPS behind Caddy) |
| `src/Wichteln.Shared` | DTOs and wish validation shared by both |
| `tests/Wichteln.Tests` | Draw-rule tests and API tests (wishlist freeze, reveal, uniqueness) |
| `deploy/` | Dockerfile, docker-compose with Caddy, `.env.example` |
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

### 1. MySQL on Cybrancee (Plesk)

1. In Plesk, **Databases → Add Database**: name `wichteln`, create a user.
2. Under the database user, allow **remote access** from the IONOS VPS IP address (Plesk: *Access control → Allow remote connections from specific hosts*).
3. Note the host name Plesk shows for MySQL. The API creates its tables on first start.

### 2. API on the IONOS VPS (Docker + Caddy)

1. Point a DNS A record, e.g. `api.wichteln.example.de`, at the VPS. Open ports 80 and 443.
2. On the VPS:
   ```bash
   git clone <this repo> wichteln && cd wichteln/deploy
   cp .env.example .env    # fill in API_DOMAIN, APP_URL, MYSQL_CONNECTION, ADMIN_KEY
   docker compose up -d --build
   curl https://api.wichteln.example.de/health
   ```
   Caddy fetches the HTTPS certificate automatically. If Caddy already runs on the VPS for other sites, drop the `caddy` service and add the `reverse_proxy` block from `deploy/Caddyfile` to your existing Caddyfile, publishing the API on a local port instead.
3. Get everyone's personal links:
   ```bash
   curl -H "X-Admin-Key: <ADMIN_KEY>" https://api.wichteln.example.de/admin/links
   ```
   Send each person only their own link.

Admin endpoints (all need the `X-Admin-Key` header):

| Endpoint | Does |
| --- | --- |
| `GET /admin/links` | Lists each person's link, wish count and whether they revealed |
| `POST /admin/new-link/{name}` | Gives one person a new link (the old one stops working) |
| `POST /admin/reset-draw` | Deletes the draw and every reveal, for example after a test run |

### 3. Blazor site on Cybrancee (Plesk)

1. Set the API address in `src/Wichteln.Client/wwwroot/appsettings.json` (`ApiBaseUrl`, with a trailing slash).
2. Publish:
   ```bash
   dotnet workload install wasm-tools   # optional, smaller and faster build
   dotnet publish src/Wichteln.Client -c Release -o publish/client
   ```
3. Upload the **contents** of `publish/client/wwwroot` (including the hidden `.htaccess`) to the domain's `httpdocs` folder with the Plesk File Manager or FTP.
4. `.htaccess` sets the WebAssembly MIME types, serves the pre-compressed Brotli files and sends unknown paths to `index.html`. If your Plesk runs on Windows/IIS, use `publish/client/web.config` instead.

The GitHub Actions workflow builds and tests every push and attaches the published site as an artifact.

## Photos (optional)

Put square photos in `src/Wichteln.Client/wwwroot/img/people/` as `justin.jpg`, `luana.jpg` and so on, and list the names under `"Photos"` in `wwwroot/appsettings.json`. Without a photo, the avatar shows the person's initial.

## Settings

| Setting (env var on the API) | Default |
| --- | --- |
| `Wichteln__UnlockAt` | `2026-11-20T00:00:00+01:00` |
| `Wichteln__People__0..4` | Justin, Luana, Lydia, Angelika, Denis |
| `Wichteln__ForbiddenPairs__0__0` / `__0__1` … | Justin/Luana, Angelika/Denis |
| `Wichteln__AllowedOrigins__0` | the site's address (CORS) |
