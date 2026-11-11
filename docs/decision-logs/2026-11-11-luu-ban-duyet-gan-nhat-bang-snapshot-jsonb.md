---
date: 2026-11-11
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-16, FR-22, FR-49]
cards: []
---
# Lưu LastApprovedVersion bằng snapshot JSONB

## Bối cảnh
Student đã mua vẫn học bản đã duyệt khi khóa chờ duyệt lại hoặc bị từ chối ([log](2026-11-11-khoa-duyet-lai-hoc-vien-xem-ban-cu.md)); Instructor sửa bản làm việc song song.

## Các lựa chọn đã xem xét
- Mỗi Lesson/Course có cột trạng thái bản nháp và bản đã duyệt: không chọn, truy vấn công khai phức tạp.
- Bảng phiên bản đầy đủ từng bản: không chọn, thừa vì chỉ cần bản gần nhất.
- Course (bản làm việc) + 1 dòng LastApprovedVersion chứa snapshot JSONB (thông tin, giá, danh sách Lesson và Video): được chọn.

## Quyết định
Khi Admin duyệt, ghi snapshot bất biến của khóa vào LastApprovedVersion; truy vấn công khai và quyền học đọc snapshot.

## Lý do
Đọc công khai đơn giản và nhanh (NFR-01), bản làm việc sửa tự do.

## Đánh đổi / hệ quả
File Video của bản cũ phải giữ đến lần duyệt kế tiếp; dữ liệu lặp giữa Course và snapshot.

## Bằng chứng
Agent chọn tạm khi viết [docs/architecture.md](../architecture.md); chủ dự án chưa xác nhận. [docs/prd.md](../prd.md) FR-16, FR-22, FR-49.
