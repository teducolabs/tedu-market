# Deploy — TEDU Market

Nguồn: NFR trong [prd.md](prd.md), [architecture.md](architecture.md) mục 2 (Tech Stack), 4 (Bảo mật), 7 (Rủi ro). Hosting: [1 VPS](decision-logs/2026-11-11-chay-1-vps-docker-compose-nginx.md), [staging cùng VPS](decision-logs/2026-11-11-staging-cung-vps-production-compose-rieng.md).
Repo hiện chưa có code: tên file, service, endpoint dưới đây là **quy ước đích**; khi code khác thì sửa file này trước.

## 1. Môi trường

| | Local | Staging | Production |
|---|---|---|---|
| Mục đích | Dev, test | Thử migration, smoke, e2e trước production | Người dùng thật |
| Compose | `docker-compose.dev.yml` | `docker-compose.prod.yml`, project `tedu-stg` | cùng file, project `tedu-prod` |
| Domain | `localhost` | `www-stg` / `admin-stg` / `api-stg` | `www` / `admin` / `api` (3 subdomain, HTTPS) |
| Database | PostgreSQL 18 container, dữ liệu bỏ được | DB riêng, dữ liệu giả | DB riêng, backup hằng ngày |
| Config | `appsettings.Development.json`, `.env.local` | `.env.stg` | `.env.prod` |
| Thanh toán | hook gọi tay | hook gọi tay với token staging | `X-API-Token` production |

Staging không bao giờ dùng dữ liệu hoặc secret của production.

## 2. Hạ tầng

Một VPS, Docker Compose, Nginx theo subdomain (khớp architecture.md mục 1, 2). Thư mục triển khai `/srv/tedu-market` (user `deploy`, không sudo).

| Thành phần | Chạy ở | Ghi chú |
|---|---|---|
| Api + BackgroundService (Outbox, hết hạn đơn, ffmpeg) | container `api` | không có worker riêng |
| StudentWeb | container `web` | Next.js standalone |
| AdminApp | container `admin` hoặc file tĩnh Nginx | SPA |
| PostgreSQL 18, MinIO, Seq | container, volume riêng mỗi môi trường | Seq và MinIO console chỉ bind `127.0.0.1`, vào bằng SSH tunnel |
| Thư mục Video | volume ngoài wwwroot, Nginx `internal` | chỉ phục vụ qua X-Accel-Redirect |
| Image | GHCR, tag = commit SHA | không deploy tag `latest` |

## 3. Biến môi trường / secret

- Lưu: `.env.stg`, `.env.prod` trên VPS (`chmod 600`, owner `deploy`) và GitHub Actions Secrets. **Không commit** (`.env*`, `secret.json` đã trong `.gitignore`). Repo chỉ có `.env.example` chứa tên biến, không chứa giá trị.
- Local: `cp .env.example .env.local`, tự sinh giá trị giả (`openssl rand -hex 32`). Không xin secret staging/production để chạy local.
- Mật khẩu Admin seed (NFR-22): sinh ngẫu nhiên lần chạy đầu, ghi `secret.json` trong volume secrets. Đọc 1 lần: `docker compose -p tedu-prod exec api cat /app/secrets/secret.json`, cất vào trình quản lý mật khẩu, đổi mật khẩu khi đăng nhập lần đầu.
- Log và output của agent không được in giá trị secret.

| Biến | Dùng cho |
|---|---|
| `ConnectionStrings__Default`, `POSTGRES_PASSWORD` | PostgreSQL |
| `PaymentHook__ApiToken` | `X-API-Token` của hook (NFR-07) |
| `VideoPlayback__SigningKey` | ký token phát video |
| `Minio__AccessKey`, `Minio__SecretKey` | thumbnail |
| `Minio__Endpoint` | URL MinIO cho `/health/ready` (không bí mật; mặc định `http://localhost:9000`) |
| `Google__ClientId`, `Google__ClientSecret` | đăng nhập Google |
| `Resend__ApiKey` | email (NFR-18) |
| `OpenIddict__EncryptionKey`, `OpenIddict__SigningKey` | token OIDC |
| `Cors__AllowedOrigins` | origin AdminApp, StudentWeb (NFR-09) |
| `Seq__ServerUrl`, `Seq__ApiKey` | log |
| `StudentWeb__SessionSecret`, `StudentWeb__OidcClientSecret` | session phía server |
| `IMAGE_TAG` | tag image cần chạy |
| `VPS_HOST`, `VPS_USERNAME`, `VPS_SSH_KEY` (chỉ GitHub Secrets) | CI SSH, key riêng cho CI |
| `BACKUP_REMOTE`, `BACKUP_REMOTE_KEY` (chỉ trên VPS) | đích backup ngoài VPS |

Tài khoản nhận QR nằm trong `appsettings.json`, không phải secret.

## 4. Build & CI/CD

GitHub Actions, các job nối bằng `needs`; tầng nào fail thì job sau skipped, không có ngoại lệ, không `continue-on-error`.

1. `unit`: `dotnet test --filter Category=Unit --settings backend/coverage.runsettings --collect:"XPlat Code Coverage"`; Domain + Application phủ ≥ 70% (NFR-16), dưới ngưỡng thì fail.
2. `integration`: `dotnet test --filter Category=Integration` (Testcontainers, PostgreSQL thật; gồm test race hook/checkout, NFR-04). Chỉ chạy khi `unit` pass.
3. `e2e`: dựng compose trên runner, chạy `pnpm --filter e2e test` (đăng ký → mua → hook → Enrollment → phát video). Chỉ chạy khi `integration` pass.
4. `build`: `pnpm turbo build --filter=...[origin/master]`, `docker build` 3 image (api, web, admin), push GHCR tag `${GITHUB_SHA}`.
5. `deploy-stg`: tự chạy trên `master` khi 1–4 pass (mục 6).
6. `deploy-prod`: job `workflow_dispatch` dùng GitHub Environment `production` có required reviewer; không tự chạy.

Pull request chỉ chạy bước 1–3. Lighthouse SEO ≥ 90 (NFR-11) chạy trên staging sau `deploy-stg`; dưới ngưỡng thì không lên production.

## 5. Database migration

- Công cụ: EF Core migrations, chạy bằng **migration bundle** trong image riêng, không `dotnet ef database update` trên server.
- Sinh bundle: `dotnet ef migrations bundle --project src/Infrastructure --startup-project src/Api --self-contained -o efbundle`.
- Xem SQL trước khi merge: `dotnet ef migrations script <from> <to> --idempotent`. Migration khóa bảng lớn (Order, Course) phải chạy ngoài giờ cao điểm.
- Mọi migration phải có `Down` chạy được **hoặc** kế hoạch rollback ghi trong PR (restore từ dump bước 6.3). Thiếu cả hai thì không lên production.
- Đổi schema theo expand/contract: thêm cột/bảng ở bản N, bỏ cột cũ ở bản N+1, để image N-1 vẫn chạy được trên schema mới (bắt buộc cho rollback mục 7).
- Local: `docker compose --env-file .env.local -f docker-compose.dev.yml up -d`, đặt `ConnectionStrings__Default` (cùng `POSTGRES_PASSWORD` trong `.env.local`), rồi `cd backend && dotnet tool restore` (ghim `dotnet-ef` 10.0.4 trong `backend/dotnet-tools.json`) và `dotnet ef database update --project src/TeduMarket.Infrastructure --startup-project src/TeduMarket.Api`. Staging và production: bundle trong bước 6.

## 6. Quy trình deploy

Chạy cho staging trước, xong mới tới production. `ENV` = `stg` | `prod`; `SHA` = commit đã qua CI; mọi lệnh chạy trên VPS tại `/srv/tedu-market`.

**Pre-check**
1. Pipeline của `SHA` xanh đủ unit, integration, e2e: `gh run list --commit $SHA --json conclusion,name`.
2. Chỉ production: staging đã chạy đúng `SHA`, smoke staging pass, Lighthouse đạt.
3. Chỉ production: backup mới hơn 24 giờ **và** vừa tạo dump trước migration: `./scripts/backup.sh prod pre-$SHA` (pg_dump + kiểm tra `pg_restore --list`).
4. Migration mới đã có `Down` hoặc kế hoạch rollback (mục 5). Ổ đĩa trống ≥ 20%: `df -h /srv`.
5. Lưu tag đang chạy để rollback: `cp .tag-$ENV .tag-$ENV.prev` (file `.tag-$ENV` chứa `IMAGE_TAG` hiện tại).

**Build**
6. Image đã có từ CI: `docker manifest inspect ghcr.io/<org>/tedu-market-api:$SHA` (và `web`, `admin`, `migrator`). Không build trên VPS.

**Migrate**
7. `echo $SHA > .tag-$ENV && export IMAGE_TAG=$SHA && docker compose -p tedu-$ENV -f docker-compose.prod.yml --env-file .env.$ENV run --rm migrator`. Lỗi thì dừng, không deploy, đi mục 7.

**Deploy**
8. `docker compose -p tedu-$ENV -f docker-compose.prod.yml --env-file .env.$ENV pull`
9. `docker compose -p tedu-$ENV -f docker-compose.prod.yml --env-file .env.$ENV up -d --remove-orphans --wait --wait-timeout 120` (`--wait` chờ healthcheck; quá hạn thì coi như fail).
10. `docker image prune -f`

**Smoke test** (`SUF` = rỗng cho production, `-stg` cho staging)
11. `curl -fsS https://api$SUF.<domain>/health/ready` → 200 (kiểm tra PostgreSQL, MinIO).
12. `curl -fsS https://api$SUF.<domain>/api/v1/courses?page=1` → 200, dưới 300 ms.
13. `curl -fsSI https://www$SUF.<domain>/courses` → 200; `https://admin$SUF.<domain>` → 200; HTTP chuyển hướng sang HTTPS (NFR-09).
14. Hook sai token: `curl -s -o /dev/null -w '%{http_code}' -X POST https://api$SUF.<domain>/api/v1/payments/hook -H 'X-API-Token: sai'` → 400 `INVALID_TOKEN`.
15. Mở 1 Lesson xem thử, video phát trong ≤ 3 giây; request manifest không token → bị từ chối (NFR-20, NFR-21).
16. Fail bất kỳ bước 11–15: rollback ngay (mục 7), không "xem thêm".

Ngắt ngắn khi `up -d` thay container là chấp nhận được (NFR-14); nên tránh deploy lúc nhiều đơn `New` đang chờ thanh toán (xem Seq trước).

## 7. Rollback

Rollback ngay khi: migrate lỗi; `--wait` quá hạn; smoke 11–15 fail; tỷ lệ 5xx > 2% trong 10 phút đầu; Outbox tồn đọng (Paid → Enrollment > 30 giây, NFR-02); lỗi mất dữ liệu hoặc lộ quyền (NFR-08, NFR-21). Mục tiêu quay về bản cũ trong 15 phút để giữ ngân sách ~7,3 giờ ngừng/tháng của NFR-14.

**Staging và production (cùng các bước, đổi `ENV`)**
1. Quay image: `export IMAGE_TAG=$(cat .tag-$ENV.prev); cp .tag-$ENV.prev .tag-$ENV && docker compose -p tedu-$ENV -f docker-compose.prod.yml --env-file .env.$ENV up -d --wait`.
2. Schema: nếu migration theo expand/contract thì image cũ chạy được, bỏ qua. Nếu có `Down`: `docker compose ... run --rm migrator --migration <TênMigrationTrước>`. Nếu không: restore dump trước migration (bước 3).
3. Restore dump (chỉ khi bắt buộc, chấp nhận mất dữ liệu ghi từ lúc dump; **cần người dùng xác nhận** nếu là production): `docker compose -p tedu-$ENV stop api web && ./scripts/restore.sh $ENV pre-$SHA && docker compose -p tedu-$ENV start api web`.
4. Chạy lại smoke 11–15, ghi sự cố và nguyên nhân vào PR hoặc decision log.

**Mất VPS (NFR-19, RTO ≤ 4 giờ, RPO ≤ 24 giờ)**: dựng VPS mới, cài Docker, clone repo, đặt `.env.prod` từ trình quản lý mật khẩu, `./scripts/restore.sh prod latest` (pg_dump hằng ngày + thư mục Video và MinIO sync từ `BACKUP_REMOTE`), `up -d`, trỏ lại DNS. Diễn tập mỗi quý và ghi thời gian thực tế; chưa diễn tập thì NFR-19 chưa được coi là đạt.

## 8. Monitoring sau deploy

Quan sát 30 phút sau mỗi deploy production, ghi kết quả vào PR/commit.
1. Log lỗi (Seq qua tunnel `ssh -L 5340:127.0.0.1:5340 deploy@<vps>`): `@Level = 'Error'` từ lúc deploy; truy vết bằng `CorrelationId` (NFR-15). Có lỗi mới lặp lại là tín hiệu rollback.
2. Health: `/health/live` (process sống) và `/health/ready` (PostgreSQL, MinIO) trả 200; probe từ ngoài VPS mỗi phút, alert khi fail 3 lần liên tiếp (NFR-14).
3. Nghiệp vụ: Outbox không tồn đọng quá 30 giây (NFR-02); hook trả 200/400 như mong đợi, không có 500; Order quá hạn được quét (FR-32).
4. Tài nguyên: CPU, RAM, ổ đĩa < 80% (ffmpeg tranh CPU, staging chung VPS); alert đĩa vì Video và backup lớn nhanh.
5. Hằng ngày: backup chạy xong và đồng bộ ra ngoài VPS (RPO 24 giờ); thiếu backup là alert mức production; hạn mức Resend gói free (NFR-18); chứng chỉ TLS còn ≥ 14 ngày (NFR-09).
6. Alert gửi về kênh chủ dự án chọn (chưa chốt — hỏi trước khi cấu hình).

## 9. Quy tắc cho agent

Production chỉ deploy khi người dùng xác nhận rõ ràng trong phiên hiện tại (xem CLAUDE.md mục 8). Staging được tự deploy nếu pipeline cho phép.
