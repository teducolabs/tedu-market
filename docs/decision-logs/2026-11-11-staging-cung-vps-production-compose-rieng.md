---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [NFR-14, NFR-19, NFR-16]
cards: []
---
# Staging chạy cùng VPS production bằng Compose project riêng

## Bối cảnh
architecture.md và [log 1 VPS](2026-11-11-chay-1-vps-docker-compose-nginx.md) chỉ nói hosting production. Cần chỗ chạy migration và smoke test trước production; thêm máy là chi phí thật nên phải hỏi chủ dự án.

## Các lựa chọn đã xem xét
- VPS staging riêng: không chọn, tốn thêm tiền mỗi tháng; được dùng lại nếu staging làm production chậm.
- Không có staging: không chọn, không có chỗ thử migration và rollback trước production.
- Cùng VPS production, Compose project riêng: được chọn.

## Quyết định
Staging là một Compose project riêng (`tedu-stg`) trên VPS production: subdomain `*-stg`, database, MinIO, volume Video và secret riêng, giới hạn CPU/RAM container.

## Lý do
Không phát sinh chi phí, vẫn có môi trường thử gần giống production (NFR-16 yêu cầu test pass trước deploy).

## Đánh đổi / hệ quả
Staging và production chia chung CPU, RAM, ổ đĩa, băng thông: ffmpeg hoặc test tải ở staging có thể làm chậm production (NFR-14, NFR-20). VPS hỏng thì mất cả hai. Không dùng staging để diễn tập restore toàn VPS (NFR-19). Dung lượng ổ đĩa phải tính cho cả hai môi trường.

## Bằng chứng
Chủ dự án chọn trong phiên viết [docs/deploy.md](../deploy.md), 2026-11-11.
