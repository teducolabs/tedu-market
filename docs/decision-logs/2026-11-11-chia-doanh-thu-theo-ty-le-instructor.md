---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-25, FR-38, FR-39]
cards: []
---
# Chia doanh thu tự động theo tỷ lệ chung, ghi đè theo Instructor

## Bối cảnh
Nhiều Instructor độc lập bán khóa trên cùng nền tảng; outline chỉ có báo cáo doanh thu theo Instructor, không có chia doanh thu.

## Các lựa chọn đã xem xét
- Không chia, nền tảng giữ toàn bộ: không chọn.
- Một tỷ lệ chung cho mọi Instructor: không chọn riêng lẻ.
- Tỷ lệ chung trong cấu hình, ghi đè được theo từng Instructor: được chọn.

## Quyết định
Tiền bán được tự chia theo tỷ lệ của từng Instructor; có một tỷ lệ chung trong cấu hình, mỗi Instructor có thể được đặt tỷ lệ riêng để ghi đè.

## Lý do
Chủ dự án chốt "tiền bán được tự chia theo tỷ lệ quy định của từng Instructor, có 1 tỷ lệ chung quy định trong cấu hình, nhưng được ghi đè ở từng instructor".

## Đánh đổi / hệ quả
Thêm tính toán và dữ liệu tỷ lệ cho Order/Payment. Chưa chốt: chỉ ghi nhận hay chi trả thật, ai đặt tỷ lệ riêng, ảnh hưởng tới đơn đã Paid khi đổi tỷ lệ, giá trị tỷ lệ chung (PRD Q-24). Không có cổng thanh toán thật nên chi trả thật chưa khả thi.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 23).
