---
date: 2026-11-11
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [NFR-12, NFR-14, NFR-19]
cards: []
---
# Chạy trên 1 VPS bằng Docker Compose + Nginx

## Bối cảnh
Ngân sách và team N/A; cần uptime 99% (NFR-14), RPO 24h / RTO 4h (NFR-19), 500 đồng thời trên 1 instance (NFR-12).

## Các lựa chọn đã xem xét
- Kubernetes / dịch vụ managed nhiều thành phần: không chọn, quá tải vận hành cho team nhỏ.
- PaaS (App Service, Cloud Run...): chưa có thông tin ngân sách, lưu video 1 GB/file và chạy ffmpeg khó hơn.
- 1 VPS, Docker Compose (API, AdminApp tĩnh, StudentWeb, PostgreSQL, MinIO, Seq), Nginx theo subdomain api/admin/www: được chọn.

## Quyết định
Triển khai toàn bộ trên 1 VPS bằng Docker Compose, Nginx làm reverse proxy; chi tiết ở tài liệu deploy.

## Lý do
Đơn giản nhất đạt 99% nếu không hỏng phần cứng; backup hằng ngày đạt RPO 24h.

## Đánh đổi / hệ quả
Không có HA: hỏng VPS là ngừng dịch vụ; deploy gây ngắt ngắn; RTO 4h phụ thuộc restore thủ công đã diễn tập.

## Bằng chứng
Agent chọn tạm khi viết [docs/architecture.md](../architecture.md); chủ dự án chưa xác nhận. Outline TEDU-67 (Module 9).
