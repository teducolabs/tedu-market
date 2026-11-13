---
date: 2026-11-13
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [NFR-12]
cards: [T03]
---
# Đổi image MinIO sang chainguard/minio và Seq không xác thực cho môi trường dev

## Bối cảnh
`docker compose -f docker-compose.dev.yml up` lỗi: `minio/minio` trên Docker Hub và `quay.io/minio/minio` đều từ chối pull. Ngoài ra `datalust/seq` bản mới không khởi động nếu thiếu cấu hình admin lần đầu.

## Các lựa chọn đã xem xét
- `quay.io/minio/minio`: thử pull, bị 401, loại.
- `bitnamilegacy/minio`: pull được nhưng là bản đóng băng, không cập nhật bản vá, loại.
- `coollabsio/minio`: bản cộng đồng, ít nguồn gốc rõ ràng, loại.
- `chainguard/minio` (Docker Hub): pull được, cùng lệnh `server /data --console-address ":9001"`, health `/minio/health/live` trả 200: được chọn.
- Seq: đặt `SEQ_FIRSTRUN_ADMINPASSWORD` (thêm 1 secret vào `.env.local`) hoặc `SEQ_FIRSTRUN_NOAUTHENTICATION=true`.

## Quyết định
Dùng `chainguard/minio` cho MinIO dev; Seq dev chạy với `SEQ_FIRSTRUN_NOAUTHENTICATION=true`.

## Lý do
Compose dev phải chạy được từ đầu. Mọi cổng Seq chỉ bind `127.0.0.1` nên không cần mật khẩu, tránh thêm secret.

## Đánh đổi / hệ quả
Image MinIO do bên thứ ba duy trì, tag `latest` không ghim. Staging và production (T12, T15) cần chọn và ghim image MinIO riêng. Seq dev không xác thực: không dùng cấu hình này ngoài máy local.

## Bằng chứng
Kết quả chạy thật ngày 2026-11-13: lỗi pull `minio/minio`; log Seq "No default admin password was supplied"; sau sửa MinIO `/minio/health/live` 200, Seq 200 ở `127.0.0.1:5340`.
