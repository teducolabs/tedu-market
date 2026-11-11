---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-04, FR-41, FR-42, NFR-22]
cards: []
---
# Admin đầu tiên tạo bằng seeding, mật khẩu ngẫu nhiên

## Bối cảnh
Admin gán role Instructor, nhưng cần một Admin ban đầu; không có đăng ký tự do cho Admin.

## Các lựa chọn đã xem xét
- Admin tự đăng ký hoặc đăng nhập Google rồi gán role bằng tay trong DB: không chọn.
- Tạo bằng seeding với mật khẩu ngẫu nhiên: được chọn.

## Quyết định
Tạo Admin đầu tiên qua seeding, mật khẩu ngẫu nhiên; chủ dự án đổi mật khẩu sau.

## Lý do
Chủ dự án chốt "tạo admin đầu tiên qua seeding, mật khẩu random tôi sẽ đổi sau".

## Đánh đổi / hệ quả
- Admin dùng tài khoản mật khẩu, khác Student/Instructor dùng Google; cần chức năng đổi mật khẩu (FR-42) và lưu mật khẩu dạng băm (NFR-22).
- Mật khẩu ngẫu nhiên hiển thị ở đâu khi seed chưa chốt (PRD Q-25); in vào log có thể lộ mật khẩu.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 18, lượt 3).
