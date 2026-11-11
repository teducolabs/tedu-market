---
date: 2026-11-11
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-03, FR-04, FR-40, NFR-06]
cards: []
---
# Authorization Server dùng OpenIddict chạy chung trong API, Google làm external login

## Bối cảnh
Cần OAuth2/OIDC Code + PKCE (NFR-06) cho AdminApp và StudentWeb; Student/Instructor vào bằng Google, Admin bằng email + mật khẩu (FR-03, FR-04).

## Các lựa chọn đã xem xét
- Dịch vụ danh tính ngoài (Keycloak, Auth0, Entra): không chọn, thêm thành phần vận hành hoặc phí, phụ thuộc bên thứ ba cho Admin seed (FR-41).
- Tự ký JWT: không chọn, không có revoke/refresh chuẩn (FR-04 cần thu hồi token khi đăng xuất).
- OpenIddict + ASP.NET Core Identity trong API project, Google là external provider: được chọn.

## Quyết định
OpenIddict chạy trong API project, Identity làm user store, đăng nhập Google qua external login; Admin dùng form email + mật khẩu của chính Authorization Server.

## Lý do
Đúng chuẩn NFR-06, đã chọn ở outline, không thêm service (NFR-14); revoke tập trung khi đăng xuất (FR-04).

## Đánh đổi / hệ quả
Auth và nghiệp vụ cùng process: sự cố API làm mất cả đăng nhập; tách riêng là việc khi scale. Student/Instructor phụ thuộc Google (xem [log SSO](2026-11-11-student-dang-nhap-google-sso.md)).

## Bằng chứng
Agent chọn tạm khi viết [docs/architecture.md](../architecture.md); chủ dự án chưa xác nhận. Outline TEDU-67 (Module 3, quyết định kiến trúc số 2).
