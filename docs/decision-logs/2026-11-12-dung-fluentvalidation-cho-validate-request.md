---
date: 2026-11-12
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-26, NFR-17]
cards: [T02]
---
# Dùng FluentValidation cho validate request trong pipeline Mediator

## Bối cảnh
FR-26 yêu cầu lỗi 400 theo từng trường. conventions §4 nói thư viện validate chưa chốt và thêm thì phải ghi decision log. T02 cần chọn để dựng `ValidationBehavior`.

## Các lựa chọn đã xem xét
- FluentValidation (+ `FluentValidation.DependencyInjectionExtensions`): Apache-2.0, quen thuộc, lộ trình bài 0.5 trong khóa học dùng sẵn; được chọn (chủ dự án xác nhận 2026-11-12).
- DataAnnotations: gắn vào command, khó viết quy tắc phụ thuộc nhiều trường, khó test riêng.
- Tự viết `IValidator` đơn giản: không thêm phụ thuộc nhưng phải tự bảo trì.

## Quyết định
Dùng FluentValidation; mỗi command có `<Command>Validator`, chạy trong `ValidationBehavior` của Mediator, gom lỗi thành map `tên trường camelCase → mảng thông điệp`.

## Lý do
Khớp conventions §2 (`<..>CommandValidator`) và §4; test được độc lập với Handler.

## Đánh đổi / hệ quả
- Thêm 1 thư viện ngoài Tech Stack, đã được chủ dự án xác nhận.
- Nếu bị loại: thay `ValidationBehavior` và các Validator, contract lỗi 400 không đổi.

## Bằng chứng
`backend/src/TeduMarket.Application/Common/Behaviors/ValidationBehavior.cs`; [conventions.md](../conventions.md) §4.
