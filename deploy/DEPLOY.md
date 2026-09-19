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

Migrations run on API startup (`Database__AutoMigrate=true`).

## Nginx snippet (host / infra)

```nginx
server {
    server_name fitcore.example.com;   # not the Task Manager host

    root /var/www/fitcore;             # Flutter web build
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    # No URI on proxy_pass — keep /api/... (controllers are [Route("api/...")])
    location /api/ {
        proxy_pass http://fitcore-api:8080;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

SPA and API share one origin so CORS `PUBLIC_ORIGIN` matches the browser URL. Point `/api/` at FitCore’s container name.

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
