---
date: 2026-11-11
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [NFR-12, NFR-14, NFR-16, NFR-17]
cards: []
---
# Chọn modular monolith (Clean Architecture + CQRS) cho backend

## Bối cảnh
PRD chỉ ràng buộc backend .NET, quy mô 10.000 tài khoản / 500 đồng thời trên 1 instance (NFR-12), uptime 99% (NFR-14). Team size và deadline là N/A, nên giả định team nhỏ không có DevOps riêng.

## Các lựa chọn đã xem xét
- Microservices (tách Order/Payment/Course...): không chọn, quy mô 1 instance không cần, thêm vận hành và giao dịch phân tán cho luồng Paid → Enrollment (NFR-05).
- Layered 3 tầng thường: không chọn, outline TEDU-67 và NFR-12 đã yêu cầu tách đọc/ghi.
- Modular monolith, Clean Architecture 4 project (Domain, Application, Infrastructure, Api) + CQRS (ghi: EF Core, đọc: Dapper): được chọn.

## Quyết định
Một backend .NET deploy 1 khối, chia module nghiệp vụ theo thư mục trong từng layer.

## Lý do
Ít thành phần phải vận hành (NFR-14), giao dịch DB đơn giản cho Order Paid + Outbox (NFR-04, NFR-05), test Handler/Domain tách khỏi hạ tầng (NFR-16).

## Đánh đổi / hệ quả
Module dùng chung 1 DB và 1 process: lỗi hoặc quá tải ở module này ảnh hưởng toàn bộ; muốn tách service sau phải cắt theo ranh giới module (phía ghi không join bảng chéo module).

## Bằng chứng
Agent chọn tạm khi viết [docs/architecture.md](../architecture.md); chủ dự án chưa xác nhận. Outline [docs/course/tedu-67.md](../course/tedu-67.md) (kiến trúc, cấu trúc solution).
