---
date: 2026-11-26
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-27, NFR-05, NFR-18, NFR-22]
cards: [T18]
---
# Gửi email Resend qua HTTP và Email chào mừng ở T18

## Bối cảnh
T18 gửi Email chào mừng qua Resend từ Outbox. Tài liệu chốt dùng Resend gói free ([log](2026-11-11-gui-email-bang-resend.md)) nhưng chưa nói cách gọi, cách ghi message vào Outbox, cấu hình, hay xử lý khi chưa có khóa API.

## Các lựa chọn đã xem xét
- Gọi Resend: REST `POST /emails` bằng `HttpClient` có tên `resend` được chọn (cùng cách với health check MinIO, không thêm thư viện, đúng 6.16); SDK `Resend` của NuGet không chọn vì ngoài Tech Stack.
- Chống gửi trùng: Outbox giao tối thiểu một lần nên gửi kèm header `Idempotency-Key` = `OutboxMessageId`; giao lại cùng message không sinh email thứ hai ở Resend (Resend giữ khóa 24 giờ).
- Thời điểm ghi message: `GoogleAccountService` ghi `OutboxMessage` ngay trong transaction tạo tài khoản Học viên, nên chỉ lần tạo tài khoản duy nhất mới có email; đăng nhập lần sau và đăng nhập bị từ chối không ghi. Gửi lỗi nằm ở job nền nên không chặn đăng nhập.
- Ranh giới layer: `IEmailSender`, `IStudentWebLinks`, `SendWelcomeEmailOutboxHandler` ở Application; `ResendEmailSender`, `StudentWebLinks` ở Infrastructure.
- Cấu hình: `Resend:ApiKey` (secret), `Resend:FromAddress`, `Resend:BaseUrl` (mặc định `https://api.resend.com/`), `Notification:StudentWebUrl` (không bí mật, gốc liên kết `/courses`).
- Thiếu `Resend:ApiKey` hoặc `Resend:FromAddress`: handler ném `EmailDeliveryException`, message ở lại `Pending`, retry theo backoff rồi `Failed` sau 5 lần (cùng đường lỗi của Outbox) thay vì bỏ qua âm thầm. Hệ quả: môi trường dev không cấu hình Resend sẽ thấy log lỗi mức Error cho mỗi Học viên mới.

## Quyết định
Như mục "chọn" ở trên.

## Lý do
FR-27 (gửi ngay sau khi tạo tài khoản, không bước xác thực riêng), NFR-05 (ghi cùng transaction), NFR-18 (Resend), NFR-22 (khóa ngoài source).

## Đánh đổi / hệ quả
- Tự bọc lỗi HTTP và theo dõi thay đổi API Resend thay vì dùng SDK.
- Hạn mức gói free chưa kiểm tra ở thẻ này (thẻ T92).
- Nội dung email do agent soạn tạm theo FR-27 ("do PO soạn" ở prd.md); cần PO xác nhận.

## Bằng chứng
[docs/prd.md](../prd.md) FR-27, NFR-18; test `WelcomeEmailTests`, `SendWelcomeEmailOutboxHandlerTests`.
