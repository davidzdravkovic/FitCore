# Deploy FitCore to a VPS

Same infra pattern as ChatMe / Hardliq: external Docker networks `edge` + `database`, shared Postgres, host nginx on the edge.

## Conflict with Task Manager (Hardliq)?

| Risk | Safe if… |
|------|----------|
| Docker DNS on `edge` | FitCore service is **`fitcore-api`**, not `api`. Hardliq already owns the `api` alias — do not reuse it. |
| Public URL | FitCore uses its **own hostname** (`PUBLIC_ORIGIN` / nginx `server_name`). Sharing Task Manager’s host will fight for `/` and `/api`. |
| Database | `POSTGRES_DB=fitcore` (Hardliq uses `taskmanager`). Same Postgres instance is fine. |
| Compose project | `fitcore-vps` vs `taskmanager-vps` — no clash. |
| JWT | Separate `JWT_SIGNING_KEY` / issuer — apps do not share tokens. |

Nginx must proxy to **`http://fitcore-api:8080`**, not `http://api:8080`.

## First deploy

```bash
cd ~/FitCore/deploy
cp .env.example .env
nano .env
chmod +x docker-init.sh
docker compose -f compose.vps.yaml --env-file .env up -d --build
```

Set `PLATFORM_ADMIN_EMAIL` / `PLATFORM_ADMIN_PASSWORD` in `.env` (seeded on startup if that email is new). Migrations run on API startup (`Database__AutoMigrate=true`).

## Nginx + SPA (Infra repo)

Edge config is in **Infra** (`nginx/conf.d/fitcore.conf` + `snippets/fitcore-locations.conf`):

- Host: `fitcore.astibosoft.com` (A record to the VPS)
- Client: `FITCORE_DIST` → `/` (Flutter `build/web`)
- API: `/api/` → `http://fitcore-api:8080`
- `fit-core-crm.duckdns.org` redirects every path to this host. Keep its certificate so that redirect can use HTTPS.

The VPS does not serve the landing page.

On the VPS, after pulling Infra and setting `FITCORE_DIST` in Infra `.env`:

```bash
# Certificate for the app host. Keep the DuckDNS certificate.
sudo docker compose -f ~/Infra/compose.yaml stop nginx
sudo certbot certonly --standalone -d fitcore.astibosoft.com
sudo docker compose -f ~/Infra/compose.yaml up -d

cd ~/FitCore.Client && flutter build web
# FITCORE_DIST=/home/ubuntu/FitCore.Client/build/web
```

`PUBLIC_ORIGIN=https://fitcore.astibosoft.com` is the app and API origin. Set it in `deploy/.env` and recreate the API container. Invite links use it. The release client calls this same host plus `/api/` because it uses the address it was opened on.

## Useful commands

```bash
cd ~/FitCore/deploy
docker compose -f compose.vps.yaml --env-file .env ps
docker compose -f compose.vps.yaml --env-file .env logs -f fitcore-api
docker compose -f compose.vps.yaml --env-file .env up -d --build
docker compose -f compose.vps.yaml --env-file .env down
```

## Update

```bash
cd ~/FitCore && git pull
cd deploy && docker compose -f compose.vps.yaml --env-file .env up -d --build
```

## Security

- Never commit `.env`
- `JWT_SIGNING_KEY` ≥ 32 characters
- Prefer HTTPS on a dedicated domain before sharing the app
