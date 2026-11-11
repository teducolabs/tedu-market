---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-12, FR-13, 2026-11-11-hook-phan-hoi-200-400-500.md]
cards: []
---
# Body phản hồi lỗi của hook có errorCode và errorMessage

## Bối cảnh
Hook trả 400 cho nhiều nguyên nhân khác nhau (sai số tiền, sai token...), đối tác cần phân biệt.

## Các lựa chọn đã xem xét
- Chỉ mã HTTP, không body: không chọn.
- Body lỗi có `errorCode` và `errorMessage`: được chọn.

## Quyết định
Body phản hồi lỗi của hook có hai trường `errorCode` và `errorMessage`.

## Lý do
Chủ dự án chốt "thêm error code trong body nữa và errorMessage nữa". Giải quyết hệ quả ở [log mã phản hồi](2026-11-11-hook-phan-hoi-200-400-500.md): 400 gộp nhiều nguyên nhân.

## Đánh đổi / hệ quả
Cần danh sách `errorCode` cố định (do architecture định nghĩa). Tên trường tôi lấy theo cách viết `errorCode`/`errorMessage` (câu gốc gõ "errorMesssage", coi là lỗi gõ).

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 31, lượt 9).
