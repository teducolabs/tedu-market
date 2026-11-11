---
date: 2026-11-11
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [NFR-13, NFR-17]
cards: []
---
# Monorepo pnpm + Turborepo, API client sinh bằng openapi-ts

## Bối cảnh
Hai frontend (AdminApp bằng React, StudentWeb bằng Next.js) phải dùng chung API client sinh từ OpenAPI (NFR-13).

## Các lựa chọn đã xem xét
- Hai repo riêng, tự viết client: không chọn, lệch contract khó phát hiện.
- Monorepo pnpm workspace + Turborepo, package api-client dùng @hey-api/openapi-ts: được chọn.

## Quyết định
Frontend là monorepo, api-client sinh tự động từ OpenAPI /api/v1 dùng chung 2 app; AdminApp là SPA Vite.

## Lý do
Đổi contract thì lỗi compile xuất hiện ở cả hai app (NFR-13).

## Đánh đổi / hệ quả
Phải chạy lại generate mỗi khi API đổi; giữ ổn định version URL /api/v1.

## Bằng chứng
Agent chọn tạm khi viết [docs/architecture.md](../architecture.md); chủ dự án chưa xác nhận. Outline TEDU-67 (quyết định kiến trúc số 3, Module 5).
