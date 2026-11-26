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

# Mỗi tiến trình chạy trong session riêng (setsid) để dừng được cả cây tiến trình con
# (pnpm -> next/vite, dotnet watch -> API), không để sót chạy nền.
pids=()
cleaned=0
cleanup() {
  [ "$cleaned" -eq 1 ] && return
  cleaned=1
  trap '' INT TERM
  echo
  echo "==> Dừng API và frontend"
  for pid in "${pids[@]}"; do
    kill -TERM -- "-$pid" 2>/dev/null || true
  done
  for _ in 1 2 3 4 5 6 7 8 9 10; do
    alive=0
    for pid in "${pids[@]}"; do
      kill -0 -- "-$pid" 2>/dev/null && alive=1
    done
    [ "$alive" -eq 0 ] && break
    sleep 0.5
  done
  for pid in "${pids[@]}"; do
    kill -KILL -- "-$pid" 2>/dev/null || true
  done
  wait 2>/dev/null || true
}
trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM

echo "==> Khởi động API (5000), Web học viên (3000), App quản trị (5173)"
setsid dotnet watch run --non-interactive --project backend/src/TeduMarket.Api &
pids+=($!)
setsid pnpm --filter @tedu-market/student-web dev &
pids+=($!)
setsid pnpm --filter @tedu-market/admin-app dev &
pids+=($!)

# Một tiến trình thoát (lỗi) thì dừng tất cả.
wait -n
