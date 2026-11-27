---
date: 2026-11-27
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-17, FR-01, Category, Course]
cards: [T25]
---
# Category chỉ có tên; tên duy nhất không phân biệt hoa thường; xóa chưa có ràng buộc Khóa học

## Bối cảnh
T25 làm CRUD Category nhưng PRD/glossary chưa nói Category gồm trường nào, tên có được trùng không, danh sách có phân trang không, và xóa Category đang có Khóa học thì sao. Module Course chưa có bảng Khóa học nên chưa kiểm tra được ràng buộc.

## Các lựa chọn đã xem xét
- Trường: chỉ `Name` (≤ 100 ký tự, bắt buộc): được chọn (agent đề xuất, FR-01 chỉ lọc theo Category). Thêm slug/mô tả/thứ tự: không chọn vì chưa có FR nào cần.
- Trùng tên: cấm, so sánh không phân biệt hoa thường, kiểm tra ở Handler (lỗi 400 gắn vào ô `name`) và bảo vệ bằng unique index trên cột kỹ thuật `normalized_name` (viết hoa bất biến của tên): được chọn. Index trực tiếp trên `name` (phân biệt hoa thường): không chọn. `ILIKE`/`lower()` trong truy vấn: không chọn vì phụ thuộc locale của DB với chữ tiếng Việt.
- Danh sách: một endpoint công khai `GET /api/v1/categories` trả mảng đủ, xếp theo tên, cache `Catalog`, xóa cache khi ghi: được chọn (số Category nhỏ). Phân trang 20 dòng như bảng khác: không chọn.
- Xóa: xóa cứng, chưa kiểm tra Khóa học: được chọn tạm vì chưa có bảng Khóa học. Chặn xóa hoặc gỡ Category khỏi Khóa học: cần chủ dự án quyết khi làm T26.

## Quyết định
`Category` có `CategoryId`, `Name`, `NormalizedName` (kỹ thuật nội bộ), `CreatedAt`, `UpdatedAt`. Tên trùng (không phân biệt hoa thường) → 400 `errors.name`. `DELETE` xóa cứng.

## Lý do
FR-17 chỉ yêu cầu tạo/sửa/xóa và danh sách cập nhật; chọn tối thiểu để khỏi implement FR ngoài thẻ.

## Đánh đổi / hệ quả
- Khi có bảng Khóa học (T26+), phải quyết quy tắc xóa Category đang được dùng (khóa ngoại RESTRICT hay gỡ Category khỏi khóa); đến lúc đó `DELETE` có thể trả 409.
- Hai request tạo cùng tên đồng thời: một bên chạm unique index và trả 500 thay vì lỗi trường (hiếm, Admin là người duy nhất ghi).

## Bằng chứng
