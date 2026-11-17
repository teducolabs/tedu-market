---
date: 2026-11-17
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [NFR-13]
cards: [T08]
---
# Commit snapshot OpenAPI, không commit client sinh

## Bối cảnh
`packages/api-client` sinh từ OpenAPI bằng `@hey-api/openapi-ts` (T08). Cần chọn nguồn schema khi generate và có commit code sinh hay không, để Turborepo/CI chạy được mà không phải dựng backend.

## Các lựa chọn đã xem xét
- Generate thẳng từ URL backend đang chạy: không chọn, CI và `turbo build` phải dựng backend + PostgreSQL.
- Thêm gói `Microsoft.Extensions.ApiDescription.Server` để backend xuất schema lúc build: không chọn vì thêm thư viện .NET ngoài Tech Stack (cần decision log riêng), để dành nếu snapshot hay lệch.
- Commit `packages/api-client/openapi.json` (snapshot từ `/openapi/v1.json`), generate ra `src/client/` và không commit thư mục này: được chọn.
- Commit cả code sinh: không chọn, diff nhiễu và dễ bị sửa tay (conventions 6.5).

## Quyết định
Commit `openapi.json` (đã bỏ `servers`); `pnpm generate` (task Turborepo `generate`, chạy trước `typecheck`/`build`/`test`) sinh `src/client/`, bị gitignore. Cập nhật snapshot bằng `pnpm --filter @tedu-market/api-client schema:fetch` khi backend chạy.

## Lý do
Đổi contract thì đổi `openapi.json` trong cùng PR, lỗi compile hiện ở cả hai app (NFR-13), CI không cần backend.

## Đánh đổi / hệ quả
Snapshot có thể lệch backend nếu quên chạy `schema:fetch`; chưa có kiểm tra tự động (xem việc phát sinh trong báo cáo T08).

## Bằng chứng
Agent chọn tạm khi làm T08; chủ dự án chưa xác nhận.
