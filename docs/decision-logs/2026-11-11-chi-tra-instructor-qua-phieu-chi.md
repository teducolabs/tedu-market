---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-38, FR-39, FR-47, FR-48, 2026-11-11-chia-doanh-thu-theo-ty-le-instructor.md]
cards: []
---
# Chi trả Instructor: ghi nhận → Admin duyệt → phiếu chi → kế toán chuyển khoản

## Bối cảnh
Đã chốt tự chia doanh thu theo tỷ lệ; cần chốt tiền đến tay Instructor bằng quy trình nào, khi chưa có cổng thanh toán thật.

## Các lựa chọn đã xem xét
- Tự động chuyển tiền cho Instructor: không chọn (không có tích hợp ngân hàng).
- Chỉ ghi nhận, Admin duyệt rồi tạo phiếu chi cho kế toán chuyển khoản: được chọn.

## Quyết định
Khoản chia được ghi nhận khi đơn Paid; Admin duyệt thì mới sang tài khoản của Instructor; Admin bấm thanh toán thì hệ thống tạo phiếu chi và kế toán chuyển khoản. Tỷ lệ chia do Admin thống nhất với Instructor; tỷ lệ chung nằm trong cấu hình hệ thống.

## Lý do
Chủ dự án chốt như trên, không nêu lý do thêm.

## Đánh đổi / hệ quả
- Chuyển khoản thủ công ngoài hệ thống; cần sổ số dư và phiếu chi đối soát được.
- "Tài khoản của Instructor" tôi đọc là số dư trong hệ thống; cần xác nhận (PRD Q-28).

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 24, lượt 4).
