---
date: 2026-11-17
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [NFR-14, NFR-16]
cards: [T12]
---
# Image AdminApp chạy Nginx; image web nhúng NEXT_PUBLIC_* lúc build

## Bối cảnh
T12 cần 4 image (api, web, admin, migrator) tag theo SHA. deploy.md cho phép AdminApp chạy "container `admin` hoặc file tĩnh Nginx". Image tag SHA dùng chung staging và production.

## Các lựa chọn đã xem xét
- AdminApp là container Nginx không đặc quyền chứa `dist/` + SPA fallback: được chọn, khớp thẻ ("Dockerfile admin") và quy trình `pull`/`up -d` một kiểu cho cả 4 service.
- Copy `dist/` ra thư mục Nginx host: không chọn, phá nguyên tắc deploy chỉ bằng image theo SHA.
- Truyền `NEXT_PUBLIC_*` bằng build arg: chưa làm, vì cùng 1 image SHA không thể mang URL khác nhau cho stg và prod.

## Quyết định
Image `admin` = `nginx-unprivileged` phục vụ file tĩnh; image `web` = Next.js standalone, chưa nhúng `NEXT_PUBLIC_*` (dùng giá trị mặc định localhost) cho tới khi có cách cấu hình theo môi trường lúc chạy.

## Lý do
Giữ đúng "build một lần, deploy nhiều môi trường" (deploy.md mục 2, 6.6).

## Đánh đổi / hệ quả
Canonical/Open Graph của web sẽ sai trên staging/production cho tới khi xử lý (cần thẻ riêng); AdminApp chưa có cấu hình URL API theo môi trường.

## Bằng chứng
`apps/admin-app/Dockerfile`, `apps/student-web/Dockerfile`, `backend/Dockerfile`.
