---
date: 2026-11-26
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-18, FR-43, FR-40, Instructor, 2026-11-11-ho-so-instructor-tao-khi-admin-duyet.md, 2026-11-26-duyet-instructor-gan-role-cung-transaction-voi-ho-so.md]
cards: [T22]
---
# Nội dung Hồ sơ Instructor và nghĩa của "xóa hồ sơ"

## Bối cảnh
T22 cho Admin sửa/xóa và Instructor sửa Hồ sơ Instructor, nhưng PRD chưa nêu hồ sơ gồm trường nào, và "xóa" có gỡ role hay không. Module Course chưa có nên chưa kiểm tra được ràng buộc với khóa học.

## Các lựa chọn đã xem xét
- Trường hồ sơ: tên hiển thị + giới thiệu + ảnh đại diện (Url): được chọn (chủ dự án chọn trong phiên T22). Chỉ tên + giới thiệu: không chọn. Upload ảnh: để sau, ngoài phạm vi.
- Xóa = xóa bản ghi và gỡ role `Instructor` cùng một lần lưu: được chọn (chủ dự án chọn). Chỉ xóa nội dung, giữ role: không chọn. Xóa bản ghi nhưng giữ role: không chọn vì Instructor còn quyền mà không có hồ sơ.
- `display_name` bắt buộc ngay khi duyệt (lấy từ email): không chọn vì lộ email ra khóa học. `display_name` nullable tới lần cập nhật đầu: được chọn (đề xuất của agent, cần xác nhận).
- Thêm `GET /api/v1/instructor-profiles/me`: được chọn (đề xuất của agent) vì Instructor không có cách khác biết Id hồ sơ của mình (`GET /users` chỉ Admin); T24 cần.

## Quyết định
`InstructorProfile` có `DisplayName` (bắt buộc ≤ 100 khi cập nhật, null trước đó), `Biography` (≤ 2000, tùy chọn), `AvatarUrl` (http/https ≤ 2048, tùy chọn), `UpdatedAt`. `PUT` thay toàn bộ nội dung; `DELETE` (Admin) xóa hồ sơ và gỡ role `Instructor` trong một `SaveChangesAsync`.

## Lý do
FR-18, FR-43; giữ nguyên tử giữa role và hồ sơ như log T21; chủ sở hữu kiểm tra trong Handler (NFR-08, 6.8).

## Đánh đổi / hệ quả
- Sau `SaveChangesAsync`, cả Xóa hồ sơ (T22) và Duyệt Instructor (T21) gọi `IUserSessionRevoker.RevokeAllAsync` (cổng của Identity, cài bằng `UserSessionRevoker`) để thu hồi mọi token của người đó: quyền Instructor mất ngay khi xóa, và người vừa được duyệt phải đăng nhập lại để nhận role (trước đó token cũ vẫn dùng được nhưng thiếu role). Thu hồi chạy sau lần lưu nên nếu lỗi giữa chừng thì role đã đổi nhưng token cũ còn; request trả 500 và Admin thử lại được. Có hiệu lực ngay vì API bật `EnableTokenEntryValidation`.
- Chưa chặn xóa khi Instructor còn khóa học và chưa có "hiển thị đúng trên khóa học": chờ module Course, cần thẻ riêng.
- Ảnh đại diện chỉ là Url bên ngoài, chưa upload.

## Bằng chứng
Trả lời của chủ dự án trong phiên T22 ngày 2026-11-26; code `backend/src/TeduMarket.Application/Instructor/`, test `InstructorProfileTests` (integration).
