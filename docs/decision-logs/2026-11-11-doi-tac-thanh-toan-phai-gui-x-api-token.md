---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-12, NFR-07, 2026-11-11-bao-ve-hook-bang-x-api-token.md, 2026-11-11-hook-do-doi-tac-ngan-hang-goi-khi-tich-hop.md]
cards: []
---
# Đối tác thanh toán phải truyền X-API-Token

## Bối cảnh
Khi tích hợp thật, đối tác ngân hàng gọi hook; cần biết họ xác thực bằng cách nào.

## Các lựa chọn đã xem xét
- Cơ chế riêng của đối tác (ví dụ chữ ký HMAC): không chọn.
- Đối tác truyền `X-API-Token` như khi test: được chọn.

## Quyết định
Đối tác thanh toán bắt buộc truyền header `X-API-Token`.

## Lý do
Chủ dự án chốt "đối tác phải truyền X-API-Token".

## Đánh đổi / hệ quả
Phụ thuộc đối tác chấp nhận gửi header tùy chỉnh; nếu đối tác không hỗ trợ thì phải đổi quyết định bằng log mới.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 19, lượt 4).
