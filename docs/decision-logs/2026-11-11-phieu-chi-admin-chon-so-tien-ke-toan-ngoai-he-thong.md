---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-48, 2026-11-11-chi-tra-instructor-qua-phieu-chi.md]
cards: []
---
# Phiếu chi do Admin chọn số tiền, kế toán xử lý ngoài hệ thống

## Bối cảnh
[Quy trình chi trả Instructor](2026-11-11-chi-tra-instructor-qua-phieu-chi.md) dừng ở "tạo phiếu chi, kế toán chuyển khoản"; cần chốt số tiền, xác nhận và dữ liệu ngân hàng.

## Các lựa chọn đã xem xét
- Phiếu chi tự động theo toàn bộ số dư: không chọn.
- Kế toán xác nhận "đã chuyển" trong hệ thống; lưu tài khoản ngân hàng Instructor trong hệ thống: không chọn.
- Admin chọn số tiền; kế toán làm ngoài hệ thống: được chọn.

## Quyết định
Admin chọn số tiền của phiếu chi; kế toán không xác nhận trong hệ thống; tài khoản ngân hàng Instructor do kế toán lưu, không lưu trong hệ thống.

## Lý do
Chủ dự án chốt như trên, không nêu lý do thêm.

## Đánh đổi / hệ quả
Hệ thống không biết phiếu chi đã chuyển thật hay chưa, đối soát số dư dựa vào phiếu chi đã tạo. Chưa chốt phiếu chi có được vượt số dư không (PRD Q-28).

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 28, lượt 5).
