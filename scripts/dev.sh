#!/usr/bin/env bash
# Chạy toàn bộ môi trường local: hạ tầng Docker, migration, API, Web học viên, App quản trị.
# Dùng: pnpm dev   (Ctrl+C để dừng API và 2 frontend; container Docker giữ nguyên, tắt bằng `pnpm dev:down`).
set -euo pipefail

cd "$(dirname "$0")/.."

if [ ! -f .env.local ]; then
  echo "Thiếu .env.local. Chạy: cp .env.example .env.local rồi điền giá trị giả (xem README.md)." >&2
  exit 1
fi

set -a
# shellcheck disable=SC1091
. ./.env.local
set +a

: "${ConnectionStrings__Default:?Thiếu ConnectionStrings__Default trong .env.local}"

compose=(docker compose --env-file .env.local -f docker-compose.dev.yml)

echo "==> Hạ tầng (PostgreSQL, MinIO, Seq)"
"${compose[@]}" up -d --wait

echo "==> Cài phụ thuộc frontend"
pnpm install --frozen-lockfile

echo "==> Migration database"
(cd backend && dotnet tool restore >/dev/null \
  && dotnet ef database update --project src/TeduMarket.Infrastructure --startup-project src/TeduMarket.Api)

pids=()
cleanup() {
  echo
  echo "==> Dừng API và frontend"
  for pid in "${pids[@]}"; do
    pkill -TERM -P "$pid" 2>/dev/null || true # tiến trình con (API do dotnet watch sinh ra)
    kill "$pid" 2>/dev/null || true
  done
  wait 2>/dev/null || true
}
trap cleanup EXIT INT TERM

echo "==> Khởi động API (5000), Web học viên (3000), App quản trị (5173)"
dotnet watch run --non-interactive --project backend/src/TeduMarket.Api &
pids+=($!)
pnpm --filter @tedu-market/student-web dev &
pids+=($!)
pnpm --filter @tedu-market/admin-app dev &
pids+=($!)

# Một tiến trình thoát (lỗi) thì dừng tất cả.
wait -n
