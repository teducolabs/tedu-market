---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: [2026-11-11-hook-don-het-han-da-huy-ghi-nhan-cho-admin.md]
superseded_by:
related: [FR-12, FR-33, FR-53]
cards: []
---
# Hook cho đơn Hết hạn/Đã hủy: ghi nhận, đơn sang Đang chờ, trả 200

## Bối cảnh
Log trước chốt hook vẫn ghi nhận khoản tiền cho đơn Hết hạn/Đã hủy nhưng chưa rõ trạng thái đơn và mã phản hồi.

## Các lựa chọn đã xem xét
- Trả 400 như trường hợp thiếu/thừa tiền: không chọn.
- Trả 200 vì khoản tiền đã được ghi nhận: được chọn.

## Quyết định
Hook báo tiền về cho đơn Hết hạn hoặc Đã hủy: ghi nhận khoản tiền, chuyển đơn sang Đang chờ để Admin xem xét, trả 200; không tạo Enrollment.

## Lý do
Chủ dự án chốt "200 đã ghi nhận, pending là Đang chờ".

## Đánh đổi / hệ quả
Đơn Đã hủy có thể quay lại Đang chờ, trạng thái không đi một chiều. Đối tác nhận 200 nên không thử lại; Admin phải xử lý khoản tiền.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 31, lượt 9).
