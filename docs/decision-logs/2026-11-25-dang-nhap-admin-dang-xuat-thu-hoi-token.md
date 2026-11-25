---
date: 2026-11-25
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-04, FR-42, NFR-06, NFR-09, NFR-22]
cards: [T15]
---
# Đăng nhập Admin, đăng xuất thu hồi token và đăng ký client OIDC ở T15

## Bối cảnh
T15 cần trang đăng nhập email + mật khẩu (nơi `/connect/authorize` chuyển tới), đổi mật khẩu, đăng xuất thu hồi token và đăng ký client `admin-app`, `student-web`. Tài liệu chưa chốt cách dựng trang, cách thu hồi và nguồn cấu hình client.

## Các lựa chọn đã xem xét
- Trang đăng nhập: HTML tối giản do API dựng (Minimal API, không Razor) chọn, vì Authorization Server chạy chung API và Admin cần trang trước khi AdminApp có token; trang đăng nhập trong AdminApp không chọn vì đường dẫn `/account/login` thuộc Authorization Server và cookie phiên của nó nằm ở origin API.
- CSP của trang này: băm SHA-256 của khối `<style>` duy nhất chọn (middleware security headers chỉ thêm CSP mặc định `default-src 'none'` nếu trang chưa tự đặt); `unsafe-inline` không chọn.
- CSRF của form: antiforgery token của ASP.NET Core, kiểm tra thủ công để thiếu/sai trả 400.
- Đăng xuất: endpoint end-session chuẩn OIDC `/connect/logout` chọn, thu hồi toàn bộ token và authorization của người dùng (lấy từ `id_token_hint`, nếu không có thì từ cookie) rồi xóa cookie Identity; endpoint `/revoke` riêng không chọn vì không xóa phiên cookie. Access token bị từ chối ngay nhờ `EnableTokenEntryValidation()` ở phía validation (mỗi request API tra DB kiểm tra token); không bật thì token cũ vẫn dùng được đến khi hết hạn.
- Đổi mật khẩu: `POST /api/v1/account/change-password` chỉ policy Admin (Instructor, Học viên không có mật khẩu); lỗi trả theo trường `currentPassword`/`newPassword` (FR-26). Đổi mật khẩu thành công thì thu hồi mọi token hiện có (kể cả token đang gọi), Admin phải đăng nhập lại.
- Client OIDC: đăng ký lúc API khởi động từ cấu hình (`AdminApp:*`, `StudentWeb:*`, secret `StudentWeb:OidcClientSecret` trùng tên biến đã có trong deploy.md), chạy lại thì cập nhật; thiếu cấu hình thì bỏ qua kèm log cảnh báo.
- Khóa tài khoản: dùng lockout mặc định của Identity (5 lần sai, 5 phút).

## Quyết định
Như mục "chọn" ở trên.

## Lý do
FR-04 (đăng xuất thu hồi token), FR-42, NFR-06 (Code + PKCE, secret ngoài source), NFR-09 (security headers), NFR-22; không thêm thư viện ngoài Tech Stack.

## Đánh đổi / hệ quả
Mỗi request API có Bearer thêm 1 truy vấn DB kiểm tra token (chấp nhận ở quy mô MVP). Redirect URI mặc định ở `appsettings.Development.json` (`http://localhost:5173/auth/callback`) là giả định, T20 phải khớp hoặc đổi cấu hình. Tên route callback của AdminApp và `StudentWeb` do T19/T20 chốt. Khóa tài khoản theo lockout cho phép người khác cố ý khóa Admin tạm thời. Trang đăng nhập chưa có nút "Đăng nhập bằng Google" (thuộc T16, T19).

## Bằng chứng
`backend/src/TeduMarket.Api/Identity/AccountEndpoints.cs`, `ConnectEndpoints.cs`, `LoginPage.cs`, `backend/src/TeduMarket.Infrastructure/Identity/OidcClientSeeder.cs`, `UserSessionRevoker.cs`.
