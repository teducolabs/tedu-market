---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: [2026-11-11-doi-ty-le-chia-chi-ap-dung-don-chua-paid.md]
superseded_by:
related: [FR-38, FR-39]
cards: []
---
# Tỷ lệ chia được sao chép vào Order khi đơn hoàn thành

## Bối cảnh
Log trước chỉ nói đơn đã Paid không bị ảnh hưởng khi đổi tỷ lệ, kèm một câu chưa rõ về "Instructor mới". Cần chốt thời điểm cố định tỷ lệ.

## Các lựa chọn đã xem xét
- Tính phần chia theo tỷ lệ hiện hành mỗi lần báo cáo: không chọn (đơn cũ đổi theo).
- Sao chép tỷ lệ vào Order tại thời điểm đơn hoàn thành: được chọn.

## Quyết định
Khi đơn hoàn thành, tỷ lệ chia hiện hành được sao chép vào Order; Admin đổi tỷ lệ của Instructor chỉ có tác dụng với các đơn hoàn thành sau đó.

## Lý do
Chủ dự án chốt "khi có đơn hàng hoàn thành thì tỷ lệ chia được copy ở thời điểm hiện tại vào order, còn thay đổi tỷ lệ bởi admin chỉ có tác dụng với các đơn tương lai".

## Đánh đổi / hệ quả
Order lưu thêm tỷ lệ (theo từng khóa/Instructor trong đơn); báo cáo dùng giá trị đã lưu, không tính lại.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 24, lượt 5).
