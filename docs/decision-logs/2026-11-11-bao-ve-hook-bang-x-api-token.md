---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-12, NFR-07, 2026-11-11-thanh-toan-chi-hien-thi-qr-hook-test.md]
cards: []
---
# Bảo vệ hook kích hoạt đơn bằng header X-API-Token

## Bối cảnh
Hook gọi thử kích hoạt đơn nếu mở công khai thì ai cũng tự kích hoạt đơn được. Outline 2.5 dùng chữ ký HMAC-SHA256 cho webhook cổng thật.

## Các lựa chọn đã xem xét
- Chữ ký HMAC-SHA256 theo outline: không chọn, vì hook chỉ là hook gọi thử, không phải cổng thật.
- Header `X-API-Token` được cấp riêng: được chọn.

## Quyết định
Hook kích hoạt đơn yêu cầu header `X-API-Token` với token được cấp riêng.

## Lý do
Chủ dự án chốt "cách bảo vệ hook qua header X-API-Token được cấp riêng".

## Đánh đổi / hệ quả
Token tĩnh yếu hơn chữ ký theo từng request (lộ token là đủ để kích hoạt đơn). Cần chốt ai giữ token, cấp và đổi thế nào (PRD Q-19). Khi tích hợp cổng thật phải thay bằng cơ chế của cổng đó.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 19).
