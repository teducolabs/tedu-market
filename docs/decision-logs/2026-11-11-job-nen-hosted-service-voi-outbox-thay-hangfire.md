---
date: 2026-11-11
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-14, FR-32, FR-35, FR-52, NFR-02, NFR-05]
cards: []
---
# Job nền dùng BackgroundService + bảng Outbox, không dùng Hangfire

## Bối cảnh
Cần xử lý nền: tạo Enrollment và gửi email sau Paid (NFR-02, NFR-05), hết hạn đơn (FR-32, FR-52), mã hóa video bằng ffmpeg (FR-35). Outline mâu thuẫn giữa Hangfire và Hosted Service.

## Các lựa chọn đã xem xét
- Hangfire (PostgreSQL storage): không chọn, thêm thư viện và dashboard cần bảo vệ, thừa cho vài loại job.
- Message broker (RabbitMQ...): không chọn, thêm hạ tầng.
- BackgroundService đọc bảng OutboxMessage bằng FOR UPDATE SKIP LOCKED, job quét theo lịch cho hết hạn đơn: được chọn.

## Quyết định
Dùng BackgroundService + bảng OutboxMessage trong PostgreSQL cho mọi việc nền.

## Lý do
Ghi Paid và OutboxMessage cùng transaction (NFR-05); chu kỳ thăm dò vài giây đạt NFR-02; không thêm thành phần (NFR-14).

## Đánh đổi / hệ quả
Tự viết retry/backoff và theo dõi job lỗi; thăm dò DB liên tục tốn chút tải. Job ffmpeg chạy chung process API (giới hạn song song) có thể tranh CPU với request.

## Bằng chứng
Agent chọn tạm khi viết [docs/architecture.md](../architecture.md); chủ dự án chưa xác nhận. Outline TEDU-67 (bài 2.6, 2.7 và ghi chú sản xuất điểm 2).
