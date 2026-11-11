---
date: 2026-11-11
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-09, FR-12, FR-53, NFR-04]
cards: []
---
# Chống Paid hai lần bằng UPDATE có điều kiện và ràng buộc unique

## Bối cảnh
Hook có thể gọi trùng hoặc đồng thời, job hết hạn có thể chạy cùng lúc với hook (NFR-04).

## Các lựa chọn đã xem xét
- Khóa lạc quan (xmin/RowVersion) rồi thử lại: cân nhắc, phức tạp hơn mà kết quả tương tự.
- Advisory lock theo Order: không chọn, dễ quên giải phóng.
- UPDATE Order có điều kiện trạng thái kiểm tra số dòng ảnh hưởng, cộng unique index (StudentId, CourseId) của Enrollment và unique idempotency key: được chọn.

## Quyết định
Chuyển trạng thái Order bằng 1 câu UPDATE có điều kiện trạng thái trong transaction; ràng buộc unique ở DB là chốt chặn cuối.

## Lý do
Đúng 1 lần dù gọi đồng thời (NFR-04); DB thực thi nên không phụ thuộc code ứng dụng.

## Đánh đổi / hệ quả
Logic chuyển trạng thái nằm ở câu SQL/repository; cần test race condition với PostgreSQL thật (NFR-16).

## Bằng chứng
Agent chọn tạm khi viết [docs/architecture.md](../architecture.md); chủ dự án chưa xác nhận. Outline TEDU-67 (bài 2.5, 2.9, 8.4).
