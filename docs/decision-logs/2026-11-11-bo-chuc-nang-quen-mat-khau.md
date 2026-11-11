---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-27, FR-28, NFR-18, 2026-11-11-student-dang-nhap-google-sso.md, 2026-11-11-gui-email-bang-resend.md]
cards: []
---
# Bỏ chức năng quên mật khẩu

## Bối cảnh
Đăng nhập chỉ qua Google SSO nên hệ thống không lưu mật khẩu, nhưng yêu cầu ban đầu có "quên mật khẩu" và email quên mật khẩu qua Resend.

## Các lựa chọn đã xem xét
- Giữ quên mật khẩu cho một nhóm dùng email + mật khẩu: không chọn.
- Bỏ chức năng: được chọn.

## Quyết định
Bỏ chức năng quên mật khẩu; chỉ đăng nhập bằng SSO.

## Lý do
Chủ dự án chốt "bỏ chức năng quên mật khẩu vì chỉ cần login qua SSO".

## Đánh đổi / hệ quả
- FR-28 giữ mã nhưng đánh dấu đã bỏ. Email quên mật khẩu bị loại khỏi phạm vi Resend (NFR-18); phần quyết định Resend trong [log gốc](2026-11-11-gui-email-bang-resend.md) còn lại không đổi.
- Không có cách vào hệ thống nếu mất quyền truy cập Google Account.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 17).
