---
date: 2026-11-11
status: Superseded
decided_on:
reconstructed: false
supersedes: []
superseded_by: 2026-11-11-trang-thai-don-moi-dang-cho-va-moc-tu-huy.md
related: [FR-08, FR-12, FR-13, FR-32, FR-33, FR-52, 2026-11-11-don-bat-thuong-cho-admin-xac-nhan.md]
cards: []
---
# Tên trạng thái đơn hàng và tự hủy đơn hết hạn sau 1 giờ

## Bối cảnh
Outline dùng Pending/Paid; PRD cần tên chuẩn cho trạng thái đơn và quy tắc cho đơn hết hạn.

## Các lựa chọn đã xem xét
- Pending/Paid theo outline: không chọn.
- Bộ trạng thái tiếng Việt của chủ dự án: được chọn.
- Giữ đơn hết hạn mãi đến khi Admin xử lý: không chọn; chủ dự án chọn tự hủy sau 1 giờ.

## Quyết định
Đơn có 5 trạng thái: Mới, Đang chờ, Đã thanh toán, Đã hủy, Hết hạn. Đơn Hết hạn bị bỏ đó thì tự chuyển Đã hủy sau 1 giờ.

## Lý do
Chủ dự án chốt "trạng thái đơn là mới, đang chờ, đã thanh toán, đã hủy, hết hạn" và "xử lý đơn pending quá hạn cứ để đó, tự hủy sau 1 tiếng".

## Đánh đổi / hệ quả
- Outline và code mẫu dùng Pending/Paid phải đổi tên theo bộ này.
- Ranh giới "Mới" và "Đang chờ", trạng thái cho thiếu/thừa tiền và mốc tính 1 giờ chưa chốt (PRD Q-30).
- Hook đến sau khi đơn đã hủy thì chỉ Admin xử lý được (FR-33).

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 20, lượt 6).
