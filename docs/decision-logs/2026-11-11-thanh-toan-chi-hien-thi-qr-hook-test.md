---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-10, FR-11, FR-12, FR-13, NFR-07]
cards: []
---
# Thanh toán: chỉ hiển thị QR, kích hoạt bằng hook gọi thử

## Bối cảnh
Outline TEDU-67 lẫn SePay (QR VietQR + webhook) và VNPay ở nhiều bài; ghi chú sản xuất của outline yêu cầu chốt cổng thanh toán. PRD cần biết giai đoạn này tích hợp đến đâu.

## Các lựa chọn đã xem xét
- Tích hợp SePay thật (QR + webhook ký HMAC): không chọn ở giai đoạn này; "tích hợp cổng thanh toán thật" nằm ngoài phạm vi.
- Tích hợp VNPay: không chọn.
- Chỉ hiển thị QR và dùng hook gọi thử để chuyển đơn sang Paid: được chọn.

## Quyết định
Hệ thống chỉ hiển thị mã QR để quét; đơn được kích hoạt bằng hook mà team tự gọi thử, chưa tích hợp cổng thật.

## Lý do
Chủ dự án chốt "cổng thanh toán chỉ là show QR để quét, tự lấy hook call test để active chưa tích hợp thật". Không nêu lý do thêm.

## Đánh đổi / hệ quả
- Bài 2.4, 2.5, 8.4 của outline cần điều chỉnh theo hướng hook gọi thử.
- Hook kích hoạt đơn là rủi ro bảo mật nếu mở ở production; cách bảo vệ chưa chốt (PRD Q-19, NFR-07).
- Đơn chưa tự đối soát với ngân hàng thật.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 8); [docs/course/tedu-67.md](../course/tedu-67.md) (ghi chú sản xuất, điểm 1).
