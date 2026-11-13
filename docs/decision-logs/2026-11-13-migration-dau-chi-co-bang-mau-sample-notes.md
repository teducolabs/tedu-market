---
date: 2026-11-13
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [NFR-12, NFR-17]
cards: [T03]
---
# Migration đầu chỉ chứa bảng mẫu `sample_notes`; bảng EF theo cột snake_case khai báo tay

## Bối cảnh
T03 yêu cầu "migration đầu" và chứng minh 1 lệnh ghi EF Core + 1 query Dapper chạy được, nhưng chưa có module nghiệp vụ nào (các thẻ Course, Identity... làm sau).

## Các lựa chọn đã xem xét
- Migration rỗng (chỉ bảng lịch sử): không chứng minh được ghi/đọc.
- Dùng bảng nghiệp vụ thật (vd `categories`): lấn sang thẻ khác, trái CLAUDE.md mục 11.
- Bảng mẫu `sample_notes` (cùng tinh thần `Sample` của T02): chọn; bị xóa bằng migration mới khi có bảng nghiệp vụ thật.
- Thư viện `EFCore.NamingConventions` để tự snake_case: không chọn, ngoài Tech Stack (checklist 6.16); khai báo `ToTable`/`HasColumnName` tay.

## Quyết định
Migration đầu tạo bảng `sample_notes`; bảng lịch sử migration tên `migrations_history`; EF Configuration khai báo tên bảng/cột snake_case thủ công.

## Lý do
Đủ cho acceptance criteria T03 mà không lấn phạm vi; tuân conventions mục 1 (snake_case, `timestamptz`) không thêm thư viện.

## Đánh đổi / hệ quả
Có 1 migration phải dọn bằng migration sau (không sửa migration đã merge, 6.15). Endpoint `/api/v1/samples/notes` chỉ map ở Development.

## Bằng chứng
`backend/src/TeduMarket.Infrastructure/Persistence/`.
