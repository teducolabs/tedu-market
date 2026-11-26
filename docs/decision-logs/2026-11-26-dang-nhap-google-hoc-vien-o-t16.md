---
date: 2026-11-26
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-03, FR-27, NFR-06, NFR-22]
cards: [T16]
---
# Đăng nhập Google của Học viên ở T16

## Bối cảnh
T16 cần đăng nhập Google qua OpenIddict: lần đầu tạo Học viên, lần sau dùng lại, hủy ở Google thì báo lỗi rõ ràng. Tài liệu chưa chốt thư viện, cách StudentWeb chọn Google, cách báo lỗi và xử lý email trùng.

## Các lựa chọn đã xem xét
- Thư viện: `Microsoft.AspNetCore.Authentication.Google` (gói chính thức của ASP.NET Core, cùng họ với Identity đã dùng) chọn; tự viết luồng OAuth2 không chọn.
- Cách chọn Google: tham số tùy chọn `identity_provider=google` trên `/connect/authorize`; chưa đăng nhập thì chuyển tới `/account/google` rồi sang Google. Không có tham số thì vẫn về trang đăng nhập Admin `/account/login`. Nút "Đăng nhập bằng Google" trên trang Admin không chọn vì ux.md nói Instructor không dùng trang đó.
- Lỗi (hủy ở Google, Google lỗi, email chưa xác minh, email thuộc tài khoản khác): trả về `redirect_uri` đã đăng ký của client kèm `error=access_denied`, `error_description`, `state` theo chuẩn OAuth2, để StudentWeb (T19) hiện banner "Đăng nhập chưa hoàn tất. Bạn thử lại nhé" (ux.md). Không xác minh được redirect_uri thì trả 400 văn bản thường.
- Email đã thuộc tài khoản chưa liên kết Google (ví dụ Admin có mật khẩu): từ chối, không tự liên kết. Tự liên kết bị loại vì cho phép dùng Google để vào tài khoản Admin chỉ nhờ trùng email.
- Chỉ nhận email Google đã xác minh (`email_verified`).
- Tạo tài khoản, gán vai trò Student, liên kết Google nằm trong 1 transaction; hai lần đăng nhập đầu chạy song song thì bên thua dùng lại tài khoản bên thắng.
- Google chỉ bật khi có `Google:ClientId` và `Google:ClientSecret`; thiếu thì `/account/google` trả 404 (cùng cách xử lý với client OIDC chưa cấu hình).
- Test: giả HTTP tới Google (token, userinfo) ở `BackchannelHttpHandler`, handler Google thật vẫn chạy (đúng quy tắc chỉ mock biên ngoài).

## Quyết định
Như mục "chọn" ở trên.

## Lý do
FR-03 (lần đầu tạo Học viên), NFR-06 (Code + PKCE, StudentWeb dùng OIDC phía server), NFR-22 (secret Google ngoài source); thư viện thuộc ASP.NET Core nên không thêm phụ thuộc ngoài hệ sinh thái.

## Đánh đổi / hệ quả
- StudentWeb (T19) phải thêm `identity_provider=google` vào yêu cầu authorize và xử lý `error=access_denied`.
- Người dùng có email trùng tài khoản Admin không đăng nhập được bằng Google; chấp nhận vì Admin không dùng Google.
- Email chào mừng (FR-27) thuộc T18, chưa gửi ở thẻ này.

## Bằng chứng
[docs/ux.md](../ux.md) luồng "Đăng nhập bằng Google"; test `GoogleLoginTests`.
