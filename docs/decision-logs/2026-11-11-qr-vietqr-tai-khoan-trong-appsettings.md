---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-10, 2026-11-11-thanh-toan-chi-hien-thi-qr-hook-test.md]
cards: []
---
# QR theo VietQR, tài khoản nhận tiền cấu hình trong appsettings.json

## Bối cảnh
FR-10 cần định dạng QR và nơi lấy tài khoản ngân hàng nhận tiền.

## Các lựa chọn đã xem xét
- Lưu tài khoản nhận tiền trong CSDL, sửa qua giao diện: không chọn.
- Cấu hình trong `appsettings.json`: được chọn.
- QR theo chuẩn khác ngoài VietQR: không chọn.

## Quyết định
Mã QR theo chuẩn VietQR; tài khoản ngân hàng nhận tiền đặt trong `appsettings.json`.

## Lý do
Chủ dự án chốt "tài khoản ngân hàng thì cấu hình appsettings.json, QR thì dùng vietqr".

## Đánh đổi / hệ quả
Đổi tài khoản nhận tiền phải sửa cấu hình và triển khai lại; không sửa được từ app quản trị.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 19, lượt 4).
