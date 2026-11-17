---
date: 2026-11-17
status: Active
decided_on: 2026-11-17
reconstructed: false
supersedes: []
superseded_by:
related: [NFR-03, NFR-17]
cards: [T09]
---
# Duyệt đề xuất mặc định của ux.md và làm thẳng trên master

## Bối cảnh
ux.md chưa có brand guideline nên design tokens và một số hành vi (định dạng tiền/ngày, polling ~3 giây, `/` là Danh sách khóa học, sắp xếp Đơn, thông báo AdminApp dưới 640px) đánh `[ĐỀ XUẤT - CẦN DUYỆT]`, chặn T09/T10. Quy trình mỗi thẻ một nhánh `feature/T<số>-...` kèm PR làm chậm dự án nhỏ.

## Các lựa chọn đã xem xét
- Giữ đề xuất ở trạng thái chờ duyệt và chờ brand: không chọn, chặn việc dựng khung UI.
- Giữ nhánh theo thẻ: không chọn theo yêu cầu của chủ dự án.

## Quyết định
Chủ dự án duyệt toàn bộ đề xuất mặc định trong ux.md (bỏ nhãn đề xuất). Làm thẳng trên `master`, không tạo nhánh theo thẻ; mỗi commit bắt đầu bằng mã thẻ `T<số>: ...`.

## Lý do
Dự án nhỏ, một nhóm; giá trị thị giác có thể đổi sau qua token tập trung ở `packages/ui`.

## Đánh đổi / hệ quả
Không còn PR review cho từng thẻ; chất lượng dựa vào CI chạy khi push `master` và tự review theo `docs/review-checklist.md`. Đổi brand sau này chỉ cần sửa token.

## Bằng chứng
Yêu cầu trực tiếp của chủ dự án trong phiên làm T09, 2026-11-17.
