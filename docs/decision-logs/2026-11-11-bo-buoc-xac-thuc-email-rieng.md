---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-27, 2026-11-11-student-dang-nhap-google-sso.md]
cards: []
---
# Không có bước xác thực email riêng

## Bối cảnh
Yêu cầu ban đầu có "xác thực email", nhưng Student đăng nhập bằng Google, email đã được Google xác thực.

## Các lựa chọn đã xem xét
- Thêm bước xác thực email riêng sau SSO: không chọn.
- Dựa vào xác thực của Google: được chọn.

## Quyết định
Không làm bước xác thực email riêng.

## Lý do
Chủ dự án trả lời "không cần" khi hỏi có cần bước xác thực riêng sau Google SSO.

## Đánh đổi / hệ quả
Tin vào email Google trả về. Email đăng ký vẫn gửi qua Resend; thời điểm và nội dung chưa chốt (PRD Q-17).

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 17, lượt 3).
