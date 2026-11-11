---
date: 2026-11-11
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-23, NFR-03]
cards: []
---
# Dùng MinIO (tương thích S3) lưu thumbnail, upload bằng presigned URL

## Bối cảnh
FR-23 yêu cầu ảnh thumbnail tải thẳng lên kho lưu trữ qua presigned URL.

## Các lựa chọn đã xem xét
- Thư mục local của API: không chọn, không có presigned URL, ảnh đi qua API.
- Dịch vụ S3 trả phí: không chọn, ngân sách N/A.
- MinIO trong Docker Compose: được chọn, đổi sang S3 thật chỉ đổi cấu hình.

## Quyết định
Thumbnail lưu ở MinIO; API cấp presigned PUT có giới hạn kiểu và dung lượng.

## Lý do
Thỏa FR-23; ảnh phục vụ qua Nginx có cache giúp LCP (NFR-03).

## Đánh đổi / hệ quả
Thêm 1 container cần backup (NFR-19).

## Bằng chứng
Agent chọn tạm khi viết [docs/architecture.md](../architecture.md); chủ dự án chưa xác nhận. Outline TEDU-67 (bài 1.8, 6.5).
