---
date: 2026-11-25
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-41, NFR-22, 2026-11-11-admin-dau-tien-tao-bang-seeding.md, 2026-11-11-mat-khau-admin-seed-ghi-vao-secret-json.md]
cards: [T14]
---
# Seeding Admin chạy lúc API khởi động, ghi `secret.json` trước khi tạo tài khoản

## Bối cảnh
[Admin đầu tiên tạo bằng seeding](2026-11-11-admin-dau-tien-tao-bang-seeding.md) và [mật khẩu ghi vào file bí mật](2026-11-11-mat-khau-admin-seed-ghi-vao-secret-json.md) đã chốt; T14 cần chọn nơi chạy seeding và thứ tự thao tác.

## Các lựa chọn đã xem xét
- Nơi chạy: `IHostedService` trong API chạy một lần lúc khởi động chọn (file ghi vào volume secrets của container `api`, khớp deploy.md); migrator không chọn vì migrator là image riêng, không có volume secrets của `api`.
- Thứ tự: ghi `secret.json` trước, tạo Admin sau chọn (ghi file lỗi thì chưa có Admin, lần khởi động sau thử lại; không bao giờ có Admin mà mất mật khẩu); đảo thứ tự không chọn.
- Đường dẫn file: cấu hình `AdminSeed:SecretFilePath`, mặc định `<thư mục gốc ứng dụng>/secrets/secret.json` (`/app/secrets/secret.json` trong container); quyền file `600` trên Linux.
- Công tắc `AdminSeed:Enabled` (mặc định bật) chỉ để test tắt seeding ở các host không có DB.

## Quyết định
Như mục "chọn" ở trên.

## Lý do
Khớp deploy.md (volume secrets của `api`), không để mật khẩu lọt log, không thêm thư viện.

## Đánh đổi / hệ quả
- Chạy nhiều bản `api` song song lần đầu có thể ghi đè `secret.json` của nhau (compose chạy 1 bản nên chấp nhận).
- Phải bảo đảm migration đã áp trước khi `api` khởi động (deploy.md đã chạy migrator trước).
