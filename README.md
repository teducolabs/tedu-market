# TEDU Market

Nền tảng bán khóa học trực tuyến: Web học viên (Next.js), App quản trị (React Vite), API (.NET 10), PostgreSQL 18.

Tài liệu chi tiết nằm trong [docs/](docs/): [prd.md](docs/prd.md), [architecture.md](docs/architecture.md), [conventions.md](docs/conventions.md), [deploy.md](docs/deploy.md), tiến độ ở [status.md](docs/status.md).

## Yêu cầu

| Công cụ | Phiên bản |
|---|---|
| Docker + Docker Compose | bản mới |
| .NET SDK | 10 |
| Node.js | ≥ 24 |
| pnpm | 10.22.0 (`corepack enable`) |
| openssl | để sinh giá trị giả |

## Cấu trúc

```
backend/          API .NET (src/, tests/)
apps/student-web  Web học viên (Next.js 16)
apps/admin-app    App quản trị (React + Vite)
packages/         Thư viện dùng chung (api-client, ...)
e2e/              Test e2e Playwright
docker-compose.dev.yml   Hạ tầng local: PostgreSQL, MinIO, Seq
```

## Chạy trên localhost

Hiện `docker-compose.dev.yml` chỉ chạy **hạ tầng** (PostgreSQL, MinIO, Seq). API và frontend chạy trực tiếp trên máy.

### Chạy nhanh: một lệnh

Sau khi đã tạo `.env.local` (bước 1 bên dưới), chạy tất cả ở thư mục gốc:

```bash
pnpm dev        # hạ tầng Docker → migration → API (5000) + Web học viên (3000) + App quản trị (5173)
pnpm dev:down   # tắt container Docker khi xong (Ctrl+C trong `pnpm dev` chỉ dừng API và frontend)
```

Script nằm ở [scripts/dev.sh](scripts/dev.sh). Các bước thủ công tương ứng ở dưới.

### 1. Hạ tầng bằng Docker

```bash
cp .env.example .env.local
```

Điền giá trị giả cho 4 biến trong `.env.local` (không dùng secret thật):

```bash
POSTGRES_PASSWORD=$(openssl rand -hex 16)
MINIO_ACCESS_KEY=$(openssl rand -hex 8)
MINIO_SECRET_KEY=$(openssl rand -hex 16)
ConnectionStrings__Default="Host=localhost;Port=5432;Database=tedu_market;Username=tedu;Password=<POSTGRES_PASSWORD>"
```

`<POSTGRES_PASSWORD>` phải trùng giá trị `POSTGRES_PASSWORD` ở trên. Rồi chạy:

```bash
docker compose --env-file .env.local -f docker-compose.dev.yml up -d
docker compose --env-file .env.local -f docker-compose.dev.yml ps   # chờ postgres "healthy"
```

| Dịch vụ | Địa chỉ |
|---|---|
| PostgreSQL | `localhost:5432` (user `tedu`, DB `tedu_market`) |
| MinIO API / console | http://localhost:9000 / http://localhost:9001 |
| Seq (log) | http://localhost:5340 |

Dừng: `docker compose --env-file .env.local -f docker-compose.dev.yml down` (thêm `-v` để xóa cả dữ liệu).

### 2. Backend

```bash
export $(grep -v '^#' .env.local | xargs)   # nạp ConnectionStrings__Default vào shell
cd backend
dotnet tool restore
dotnet ef database update --project src/TeduMarket.Infrastructure --startup-project src/TeduMarket.Api
dotnet run --project src/TeduMarket.Api
```

- API chạy ở http://localhost:5000; kiểm tra: `curl http://localhost:5000/health/live`.
- Lần chạy đầu API tự tạo Admin `support@tedu.com.vn` và ghi mật khẩu ngẫu nhiên vào `backend/src/TeduMarket.Api/secrets/secret.json` (đã nằm trong `.gitignore`). Đọc mật khẩu từ file đó; không in ra log.
- Có thể đổi đường dẫn file bằng biến `AdminSeed__SecretFilePath`.

### 3. Frontend

Ở thư mục gốc repo:

```bash
pnpm install
pnpm --filter @tedu-market/student-web dev   # http://localhost:3000
pnpm --filter @tedu-market/admin-app dev     # http://localhost:5173 (cổng mặc định của Vite)
```

Chạy 2 lệnh `dev` ở 2 terminal riêng.

## Kiểm tra chất lượng

```bash
# Backend
cd backend
dotnet build -warnaserror
dotnet format --verify-no-changes
dotnet test                 # integration test cần Docker (Testcontainers)

# Frontend (thư mục gốc)
pnpm lint
pnpm format:check
pnpm typecheck
pnpm build
pnpm --filter @tedu-market/student-web test
pnpm --filter @tedu-market/admin-app test
pnpm e2e                    # Playwright; cần cài thư viện hệ thống cho Chromium
```

## Quy ước làm việc

Mỗi task gắn một mã thẻ trong [docs/status.md](docs/status.md); commit bắt đầu bằng mã thẻ (`T123: <việc>`), làm thẳng trên `master`. Chi tiết xem [AGENTS.md](AGENTS.md).
