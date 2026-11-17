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
# Image AdminApp chạy Nginx; image web đọc SITE_URL lúc chạy

## Bối cảnh
T12 cần 4 image (api, web, admin, migrator) tag theo SHA. deploy.md cho phép AdminApp chạy "container `admin` hoặc file tĩnh Nginx". Image tag SHA dùng chung staging và production.

## Các lựa chọn đã xem xét
- AdminApp là container Nginx không đặc quyền chứa `dist/` + SPA fallback: được chọn, khớp thẻ ("Dockerfile admin") và quy trình `pull`/`up -d` một kiểu cho cả 4 service.
- Copy `dist/` ra thư mục Nginx host: không chọn, phá nguyên tắc deploy chỉ bằng image theo SHA.
- Truyền `NEXT_PUBLIC_*` bằng build arg: không chọn, vì cùng 1 image SHA không thể mang URL khác nhau cho stg và prod.

## Quyết định
Image `admin` = `nginx-unprivileged` phục vụ file tĩnh; image `web` = Next.js standalone, layout và robots render lúc request và đọc biến `SITE_URL` lúc chạy (thay `NEXT_PUBLIC_SITE_URL`).

## Lý do
Giữ đúng "build một lần, deploy nhiều môi trường" (deploy.md mục 2, 6.6).

## Đánh đổi / hệ quả
Layout render động nên mất prerender tĩnh của layout gốc (trang vẫn cache được ở Nginx nếu cần). Hai app chưa gọi API nên chưa có URL API; khi thẻ nối api-client làm, URL API cũng phải đọc lúc chạy, không dùng biến build-time.

## Bằng chứng
`apps/admin-app/Dockerfile`, `apps/student-web/Dockerfile`, `backend/Dockerfile`.
