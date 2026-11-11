---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-14, FR-27, FR-28, NFR-18]
cards: []
---
# Gửi email bằng Resend (gói free)

## Bối cảnh
Hệ thống cần gửi email đăng ký, quên mật khẩu và hoàn thành đơn; outline chỉ nói "gửi email xác nhận" mà không chọn dịch vụ.

## Các lựa chọn đã xem xét
- Dịch vụ khác (SMTP riêng, SendGrid...): chủ dự án không nêu.
- Resend gói free: được chọn.

## Quyết định
Tích hợp Resend gói free để gửi email đăng ký, quên mật khẩu và hoàn thành đơn.

## Lý do
Chủ dự án chốt "tích hợp Resend free". Không nêu lý do thêm (ràng buộc ngân sách: N/A theo input).

## Đánh đổi / hệ quả
Gói free có hạn mức gửi; cần kiểm tra hạn mức hiện hành của Resend so với số email dự kiến trước khi chạy thật.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 10).
