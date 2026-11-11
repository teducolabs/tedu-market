---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [NFR-17]
cards: []
---
# Dùng thư viện Mediator (martinothamar, MIT) thay MediatR

## Bối cảnh
Outline dùng MediatR cho CQRS. Ghi chú sản xuất của outline nêu MediatR từ v13 chuyển sang license thương mại, cần phương án thay thế.

## Các lựa chọn đã xem xét
- Giữ MediatR: không chọn vì vấn đề license.
- Tự viết mediator đơn giản: nêu trong ghi chú sản xuất; chủ dự án không chọn.
- Mediator của martinothamar (license MIT): được chọn.

## Quyết định
Dùng thư viện Mediator (martinothamar, MIT) thay MediatR.

## Lý do
Chủ dự án chốt vì MediatR có license chuyển sang thương mại. Mediator là license MIT.

## Đánh đổi / hệ quả
- Bài 0.4, 0.5 và mọi code dùng MediatR cần viết lại theo API của Mediator (đã học ở bài X, giờ thay bằng thư viện mới).
- Tên khái niệm Command/Query/Handler/Pipeline Behavior giữ nguyên.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 15); [docs/course/tedu-67.md](../course/tedu-67.md) (ghi chú sản xuất, điểm 3).
