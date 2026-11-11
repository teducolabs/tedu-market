---
date: 2026-11-11
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [NFR-01, NFR-10, NFR-12, NFR-14]
cards: []
---
# Cache và rate limit trong bộ nhớ, không dùng Redis giai đoạn đầu

## Bối cảnh
Cần cache API catalog (NFR-01) và rate limit checkout (NFR-10). Outline có Redis trong Docker Compose.

## Các lựa chọn đã xem xét
- Redis: không chọn lúc này, chỉ có 1 instance (NFR-12), thêm thành phần cần giám sát (NFR-14).
- Output Cache/IMemoryCache và RateLimiter dựng sẵn của ASP.NET Core: được chọn.

## Quyết định
Không dùng Redis; dùng cache và rate limiter trong bộ nhớ của API.

## Lý do
Đủ cho 1 instance, ít thành phần vận hành.

## Đánh đổi / hệ quả
Cache và bộ đếm rate limit mất khi restart và không chia sẻ giữa nhiều instance; scale ngang phải thêm Redis và viết lại phần này.

## Bằng chứng
Agent chọn tạm khi viết [docs/architecture.md](../architecture.md); chủ dự án chưa xác nhận. Outline TEDU-67 (bài 0.10, 4.4); [docs/prd.md](../prd.md) NFR-12.
