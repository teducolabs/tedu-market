---
date: 2026-11-11
status: Superseded
decided_on:
reconstructed: false
supersedes: []
superseded_by: 2026-11-11-hook-don-het-han-da-huy-tra-200-dang-cho.md
related: [FR-12, FR-33, FR-53, 2026-11-11-trang-thai-don-moi-dang-cho-va-moc-tu-huy.md]
cards: []
---
# Hook báo tiền cho đơn Hết hạn/Đã hủy: ghi nhận, chờ Admin xem xét

## Bối cảnh
Khách có thể chuyển tiền sau khi đơn đã Hết hạn hoặc bị hủy (tự động sau 1 giờ); hook vẫn được gọi.

## Các lựa chọn đã xem xét
- Bỏ qua hook, không ghi nhận: không chọn (mất dấu khoản tiền đã nhận).
- Tự chuyển Đã thanh toán và cấp Enrollment: không chọn.
- Ghi nhận và để Admin xem xét: được chọn.

## Quyết định
Hook báo tiền về cho đơn Hết hạn hoặc Đã hủy vẫn được ghi nhận nhưng ở trạng thái chờ ("pending") để Admin xem xét; không tạo Enrollment.

## Lý do
Chủ dự án chốt "vẫn ghi nhận nhưng pending để admin xem xét". Khớp quy tắc đơn bất thường chờ Admin.

## Đánh đổi / hệ quả
- Tôi hiểu "pending" là trạng thái Đang chờ; mã HTTP phản hồi chưa chốt (PRD Q-31).
- Đơn đã Đã hủy có thể quay lại Đang chờ, nên trạng thái không đơn điệu một chiều.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 31, lượt 8).
