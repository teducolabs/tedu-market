---
date: 2026-11-26
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-04, FR-05, FR-42, NFR-06, NFR-09]
cards: [T20]
---
# Màn hình /sign-in, token trong bộ nhớ và URL API suy ra từ origin ở AdminApp (T20)

## Bối cảnh
T20 nối AdminApp với Authorization Server. ux.md chỉ mô tả Admin (mở app chưa đăng nhập → chuyển thẳng `/account/login`), nhưng Instructor phải đăng nhập bằng Google (`identity_provider=google`) mà AdminApp chưa có màn hình nào để chọn. Ngoài ra một image `admin` dùng cho cả staging và production (tag = SHA) nên không nhúng URL API lúc build được, và vai trò giả `?role=` của T10 phải bỏ.

## Các lựa chọn đã xem xét
- Chọn cách đăng nhập: màn hình `/sign-in` trong AdminApp với 2 nút (Google cho Instructor, email cho Admin) chọn (người dùng xác nhận 2026-11-26); thêm nút Google vào trang `/account/login` của API không chọn vì phải sửa backend ngoài phạm vi T20; chỉ làm Admin không chọn vì bỏ dở FR-04 cho Instructor.
- Thư viện: `react-oidc-context` + `oidc-client-ts` (đã nêu ở architecture.md mục 2); `oidc-client-ts` là phụ thuộc trực tiếp của `react-oidc-context`, thêm vào `package.json` để tạo `UserManager` và cấu hình kho.
- Kho token: `InMemoryWebStorage` cho user/token chọn (NFR-06); trạng thái yêu cầu đăng nhập (state, code verifier) để ở kho mặc định của thư viện vì phải sống qua lần chuyển trang. Hệ quả: tải lại trang mất phiên trong bộ nhớ, người dùng quay lại `/sign-in` và bấm lại; Admin/Instructor còn cookie Identity ở API nên không phải nhập lại mật khẩu.
- Quay lại trang cũ: `state.returnTo` trong yêu cầu đăng nhập, lọc open redirect (chỉ đường dẫn nội bộ).
- Đổi mật khẩu xong: gọi `/connect/logout` (`signoutRedirect`) thay vì chỉ xóa token trong bộ nhớ, vì security stamp mới chỉ vô hiệu cookie Identity sau chu kỳ kiểm tra (mặc định 30 phút) nên nếu không xóa phiên thì "đăng nhập lại bằng mật khẩu mới" có thể tự vào mà không cần mật khẩu. Chờ 1,5 giây cho toast hiện trước khi chuyển trang.
- URL API: biến build `VITE_API_URL` nếu có; không có thì suy ra từ origin (`localhost` → `http://localhost:5000`; nhãn đầu `admin`/`admin-stg` → `api`/`api-stg`) chọn; thêm biến runtime/`config.js` do Nginx sinh không chọn vì phải đổi Dockerfile và deploy; nhúng URL lúc build không chọn vì image dùng chung hai môi trường.
- Vai trò giả `?role=` (log T10) bỏ; vai trò lấy từ claim `role` của id_token.

## Quyết định
Như mục "chọn" ở trên.

## Lý do
FR-04, FR-42, NFR-06 (Code + PKCE, token không ở `localStorage`), một image cho hai môi trường (deploy.md mục 2), tiếp tục dùng thư viện đã nêu ở Tech Stack.

## Đánh đổi / hệ quả
Quy ước đặt tên domain `admin*`/`api*` thành ràng buộc ngầm; domain khác quy ước phải đặt `VITE_API_URL` lúc build. Mất phiên khi tải lại trang (đánh đổi của token trong bộ nhớ). Vai trò ở UI chỉ để chọn sidebar, API vẫn kiểm quyền (FR-05). Chromium thiếu thư viện hệ thống ở máy dev nên e2e Playwright chưa chạy được; T21 xác nhận trên staging.

## Bằng chứng
`apps/admin-app/src/lib/auth/`, `apps/admin-app/src/routes/sign-in-page.tsx`, `apps/admin-app/src/routes/change-password-page.tsx`, `e2e/admin-app-sign-in.spec.ts`.
