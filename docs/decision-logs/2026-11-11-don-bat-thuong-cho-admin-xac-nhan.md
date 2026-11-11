---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-13, FR-32, FR-33, NFR-04]
cards: []
---
# Đơn thiếu/thừa tiền hoặc quá hạn chờ Admin xác nhận, hạn 15 phút

## Bối cảnh
Chuyển khoản có thể thiếu tiền, thừa tiền hoặc đến sau khi đơn hết hạn. Cần quy tắc cho các trường hợp này.

## Các lựa chọn đã xem xét
- Tự động hủy/hoàn tiền hoặc tự cho Paid: không chọn (hoàn tiền nằm ngoài phạm vi).
- Ghi trạng thái đơn và chờ Admin xác nhận: được chọn.

## Quyết định
Đơn thiếu tiền, thừa tiền hoặc quá hạn chỉ được ghi trạng thái tương ứng; Admin xác nhận thì mới chuyển sang trạng thái khác. Hạn thanh toán là 15 phút.

## Lý do
Chủ dự án chốt "cứ ghi trạng thái đơn, chờ admin confirm để chuyển sang trạng thái khác, hạn 15 phút". Không nêu lý do thêm.

## Đánh đổi / hệ quả
- Admin phải xử lý thủ công, phát sinh công việc vận hành.
- Chưa chốt tên trạng thái và trạng thái đích Admin được chọn (PRD Q-20).

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 9).
