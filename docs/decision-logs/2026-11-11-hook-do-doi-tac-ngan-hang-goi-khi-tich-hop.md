---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-12, NFR-07, 2026-11-11-bao-ve-hook-bang-x-api-token.md, 2026-11-11-thanh-toan-chi-hien-thi-qr-hook-test.md]
cards: []
---
# Người gọi hook: chủ dự án khi test, đối tác ngân hàng khi tích hợp thật

## Bối cảnh
Cần biết ai gọi hook kích hoạt đơn ở từng giai đoạn.

## Các lựa chọn đã xem xét
- Admin bấm kích hoạt trong app quản trị: không chọn (Admin chỉ xử lý đơn bất thường, FR-33).
- Hook chỉ để test, về sau đối tác gọi: được chọn.

## Quyết định
Khi test, chủ dự án tự gọi hook. Khi tích hợp thanh toán thật, đối tác thanh toán/ngân hàng gọi hook khi tài khoản công ty nhận được tiền.

## Lý do
Chủ dự án chốt như trên.

## Đánh đổi / hệ quả
Endpoint hook dùng chung cho cả hai giai đoạn, nên cơ chế xác thực phải chịu được việc đối tác gọi; chưa rõ đối tác có dùng `X-API-Token` không (PRD Q-19).

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 19, lượt 3).
