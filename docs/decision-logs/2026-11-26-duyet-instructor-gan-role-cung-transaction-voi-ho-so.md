---
date: 2026-11-26
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-40, FR-18, Instructor, Identity, 2026-11-11-instructor-dang-ky-thuong-admin-gan-role.md, 2026-11-11-ho-so-instructor-tao-khi-admin-duyet.md]
cards: [T21]
---
# Duyệt Instructor: gán role và tạo Hồ sơ Instructor trong cùng một lần lưu

## Bối cảnh
FR-40 yêu cầu Admin duyệt một Học viên thành Instructor: gán role `Instructor` (thuộc module Identity, bảng của ASP.NET Core Identity) và tạo `InstructorProfile` (module Instructor). Hai việc phải cùng thành công hoặc cùng không xảy ra; mặt khác conventions 6.3 cấm Handler phía ghi truy cập bảng của module khác.

## Các lựa chọn đã xem xét
- Handler gọi `UserManager.AddToRoleAsync` rồi lưu hồ sơ: không chọn. `UserManager` lưu ngay, lỗi ở bước hồ sơ để lại người có role mà không có hồ sơ; Application còn phải tham chiếu ASP.NET Core Identity.
- Cổng `IUserRoleService` (Application, Identity cài đặt ở Infrastructure) chỉ đánh dấu gán role (thêm `IdentityUserRole` vào `AppDbContext` dùng chung, chưa lưu); Handler lưu một lần bằng `IUnitOfWork`: được chọn.
- Tách thành hai bước qua Outbox: không chọn; Admin cần kết quả ngay và không có bên thứ ba để gọi.

## Quyết định
`ApproveInstructorCommandHandler` dùng `IUserRoleService.StageRoleAsync` rồi `IInstructorProfileRepository.Add` và gọi `SaveChangesAsync` một lần; `instructor_profiles.user_id` có unique index.

## Lý do
Giữ ranh giới module (6.3) mà vẫn đảm bảo nguyên tử. Hai Admin duyệt cùng người cùng lúc: unique index và khóa chính của `user_roles` làm lần lưu thứ hai thất bại, không bao giờ có hai hồ sơ.

## Đánh đổi / hệ quả
- Người bị duyệt đang đăng nhập vẫn giữ token cũ (không có role Instructor); phải đăng nhập lại để nhận quyền. Chưa thu hồi token khi duyệt.
- Lần duyệt đua nhau thua cuộc trả 500 (vi phạm unique) thay vì 400; chấp nhận vì Admin thao tác tay, hiếm.
- Chỉ người có role `Student` mới duyệt được (Admin không thể thành Instructor); nội dung `InstructorProfile` ban đầu chỉ có Id, `user_id`, `created_at`, T22 bổ sung trường hiển thị.
- Khóa ngoại `instructor_profiles.user_id` → `users.id` xóa theo (cascade).

## Bằng chứng
Code: `backend/src/TeduMarket.Application/Instructor/Commands/ApproveInstructor/`, `backend/src/TeduMarket.Infrastructure/Identity/UserRoleService.cs`; test `InstructorApprovalTests` (integration).
