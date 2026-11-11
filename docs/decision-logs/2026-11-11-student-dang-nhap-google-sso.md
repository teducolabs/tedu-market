---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-03, FR-27, FR-28, NFR-06]
cards: []
---
# Student đăng nhập chỉ bằng Google SSO

## Bối cảnh
Outline TEDU-67 chỉ nói "đăng nhập Student" (ASP.NET Core Identity + OpenIddict), không nêu cách đăng ký hay nguồn danh tính. PRD cần chốt Student vào hệ thống bằng cách nào.

## Các lựa chọn đã xem xét
- Email + mật khẩu tự quản (Identity local): không được chọn; chủ dự án nói Student chỉ cần SSO Google.
- Google SSO: được chọn.
- Thêm mạng xã hội khác: chủ dự án không nêu.

## Quyết định
Student đăng nhập bằng tài khoản Google (SSO), không có đăng ký email/mật khẩu cho Student.

## Lý do
Chủ dự án chốt "Student chỉ cần login SSO qua Google Account email". Không nêu lý do thêm.

## Đánh đổi / hệ quả
- Yêu cầu "có xác thực email" và "quên mật khẩu" của chủ dự án không khớp với SSO thuần: cần chốt áp dụng cho nhóm nào (PRD Q-17).
- Student phụ thuộc Google; không có Google Account thì không mua được.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 5); [docs/prd.md](../prd.md).
