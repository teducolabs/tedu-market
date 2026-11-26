---
date: 2026-11-25
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-04, FR-05, NFR-06, NFR-08, NFR-22]
cards: [T13, T15]
---
# Cấu hình Identity + OpenIddict ở T13: khóa, vai trò cộng dồn, tên bảng

## Bối cảnh
T13 dựng Authorization Server theo [log OpenIddict](2026-11-11-openiddict-chay-chung-trong-api-google-external.md). Cần chốt vài chi tiết kỹ thuật chưa có trong tài liệu.

## Các lựa chọn đã xem xét
- Khóa token: `OpenIddict__EncryptionKey` (base64, ≥ 32 byte) và `OpenIddict__SigningKey` (khóa riêng RSA ≥ 2048 bit PKCS#8, PEM hoặc base64 DER; sửa ở T15 vì bản đầu dùng khóa ký đối xứng mà OpenIddict từ chối) chọn; khóa tạm trong bộ nhớ chỉ cho Development; ngoài Development thiếu khóa thì API không khởi động (fail-fast). Chứng chỉ X.509 không chọn vì deploy.md đã chốt biến khóa.
- Vai trò: Instructor giữ thêm vai trò Student (Duyệt Instructor chỉ thêm vai trò) chọn, để policy `Student` là một `RequireRole`; policy "Student hoặc Instructor" không chọn.
- Gieo 3 vai trò bằng `HasData` trong migration (Id cố định) chọn; gieo lúc khởi động không chọn vì cần chạy cả khi dùng migrator bundle.
- Tên bảng/cột Identity, OpenIddict: tự viết `SnakeCaseNaming` (~70 dòng) chọn; thêm `EFCore.NamingConventions` không chọn vì ngoài Tech Stack. Cột của thư viện (`lockout_end`, `creation_date`...) giữ tên gốc đổi snake_case, không theo hậu tố `_at`.
- Khóa chính OpenIddict dùng `Guid` cho thống nhất với Identity.
- T13 chưa có trang đăng nhập (thuộc T15, T16): `/connect/authorize` chưa đăng nhập thì chuyển tới `/account/login`; test dùng endpoint đăng nhập giả chỉ có trong project test.

## Quyết định
Như mục "chọn" ở trên.

## Lý do
Đúng NFR-06, NFR-22 (khóa ngoài source), FR-05; không thêm thư viện ngoài Tech Stack.

## Đánh đổi / hệ quả
Instructor phải luôn có cả 2 vai trò (T21 phải gán thêm, không thay). Chạy API ngoài Development cần đủ 2 khóa, kể cả `docker run` thử.
Sau reverse proxy TLS, OpenIddict đòi HTTPS: cần forwarded headers ở T-deploy (ghi ở báo cáo T13).

## Bằng chứng
`backend/src/TeduMarket.Api/Identity/IdentityRegistration.cs`, `backend/src/TeduMarket.Infrastructure/Persistence/SnakeCaseNaming.cs`.
