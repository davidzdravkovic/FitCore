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

- Host: `fit-core-crm.duckdns.org`
- SPA: `FITCORE_DIST` → `/usr/share/nginx/fitcore` (Flutter `build/web`)
- API: `/api/` → `http://fitcore-api:8080` (keeps `/api` path)

On the VPS, after pulling Infra and setting `FITCORE_DIST` in Infra `.env`:

```bash
# Issue TLS once (stop nginx so certbot can bind :80)
sudo docker compose -f ~/Infra/compose.yaml stop nginx
sudo certbot certonly --standalone -d fit-core-crm.duckdns.org
sudo docker compose -f ~/Infra/compose.yaml up -d

# Flutter web (sibling clone)
cd ~/FitCore.Client && flutter build web
# FITCORE_DIST=/home/ubuntu/FitCore.Client/build/web
```

`PUBLIC_ORIGIN=https://fit-core-crm.duckdns.org` must match the browser URL.

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
