---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-41, NFR-22, 2026-11-11-admin-dau-tien-tao-bang-seeding.md]
cards: []
---
# Mật khẩu Admin seed ghi vào file bí mật ngoài source

## Bối cảnh
[Admin đầu tiên tạo bằng seeding](2026-11-11-admin-dau-tien-tao-bang-seeding.md) với mật khẩu ngẫu nhiên; chưa rõ mật khẩu được đưa ra ở đâu.

## Các lựa chọn đã xem xét
- In ra log/console: không chọn (log có thể lộ mật khẩu).
- Ghi vào file bí mật bị loại khỏi source control (kiểu `secret.json`): được chọn.

## Quyết định
Mật khẩu ngẫu nhiên được ghi vào một file bí mật nằm ngoài source control.

## Lý do
Chủ dự án chốt "đưa vào file gì đó ignore khỏi source kiểu secret.json". Tránh lộ mật khẩu trong repo.

## Đánh đổi / hệ quả
Cần `.gitignore` đúng và chủ dự án giữ file đó; mất file trước khi đổi mật khẩu thì phải seed lại.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 25, lượt 4).
