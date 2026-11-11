---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: [2026-11-11-khoa-bi-an-van-hoc-duoc-voi-student-da-mua.md]
superseded_by:
related: [FR-16, FR-22, FR-36, FR-49]
cards: []
---
# Khóa chờ duyệt lại hoặc bị từ chối: khách không thấy, học viên xem bản đã duyệt cũ

## Bối cảnh
Log trước chốt Student đã mua vẫn học được khóa bị ẩn, nhưng chưa nói họ thấy bản nào (bản vừa sửa hay bản cũ). Thêm tình huống Admin từ chối duyệt.

## Các lựa chọn đã xem xét
- Student đã mua thấy ngay bản vừa sửa (nội dung chưa duyệt): không chọn.
- Student đã mua xem bản đã duyệt gần nhất, bản mới chỉ hiện sau khi duyệt: được chọn.

## Quyết định
Khi khóa chờ duyệt lại hoặc Admin từ chối duyệt, khách không thấy khóa; Student đã có Enrollment vẫn xem bản đã duyệt gần nhất; bản mới chỉ hiển thị khi được Admin duyệt.

## Lý do
Chủ dự án chốt "admin từ chối duyệt thì khóa học ẩn đi với khách, với học viên đã học thì vẫn xem được bản cũ, bản mới chỉ show nếu đã approve".

## Đánh đổi / hệ quả
Hệ thống phải giữ hai bản nội dung của một khóa (bản đã duyệt và bản đang chờ), tăng độ phức tạp dữ liệu và lưu trữ (kể cả file video). Chi tiết lý do từ chối và gửi duyệt lại: PRD Q-22.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 20, 22, lượt 5).
