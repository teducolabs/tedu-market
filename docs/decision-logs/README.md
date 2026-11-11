# Decision logs

Tra cứu "tại sao lúc đó lại chọn thế". KHÔNG dùng để theo dõi tiến độ (việc của bảng Kanban hoặc `docs/status.md`) và KHÔNG mô tả kiến trúc hiện tại (việc của `docs/architecture.md`). Mỗi quyết định 1 file.

## Tên file: `YYYY-MM-DD-<slug>.md`

- `YYYY-MM-DD` = ngày ghi log (ngày tạo file), không lùi ngày. Ngày quyết định thực tế khác (vd log tái dựng từ lịch sử git) thì ghi ở `decided_on`.
- `<slug>`: kebab-case, tiếng Việt không dấu, tối đa ~6 từ, mô tả nội dung quyết định (vd `2026-09-24-chon-postgresql-lam-database-chinh.md`). Cấm slug chung chung: `update`, `fix`, `decision-1`.
- Nhiều quyết định cùng ngày: mỗi quyết định 1 file, phân biệt bằng slug.
- KHÔNG đổi tên file sau khi đã merge (file khác và thẻ Kanban link tới nó).

## Format

Copy `_template.md`. Ý nghĩa các mục:

- **Bối cảnh** — vấn đề cần quyết định, vì sao phải quyết lúc đó.
- **Các lựa chọn đã xem xét** — kể cả lựa chọn bị loại và lý do loại.
- **Quyết định** — chọn gì, 1 câu.
- **Lý do** — gắn với NFR/ràng buộc cụ thể trong `docs/prd.md` hoặc `docs/architecture.md` nếu có.
- **Đánh đổi / hệ quả** — mất gì, rủi ro gì.
- **Bằng chứng** — bắt buộc với log tái dựng (hash commit, link PR, đường dẫn file, tài liệu cũ); log thường có thể trống.

## Trạng thái

- `Proposed` — agent chọn tạm để không bị kẹt, người dùng chưa xác nhận. Phải chuyển `Active` hoặc `Deprecated` trước khi merge.
- `Active` — đang hiệu lực.
- `Superseded` — bị quyết định khác thay thế (có `superseded_by`).
- `Deprecated` — không còn áp dụng, không có quyết định thay thế.

## Thay thế quyết định

Không xóa file, không sửa nội dung quyết định đã `Active`. Tạo file mới có `supersedes`; ở file cũ chỉ sửa `status: Superseded` và `superseded_by`. Ngoại lệ duy nhất: sửa lỗi chính tả, link hỏng.

## Log tái dựng (`reconstructed: true`)

Chỉ tạo khi có bằng chứng đã từng có quyết định. Lý do chỉ ghi những gì bằng chứng nói; không có thì ghi "Không rõ — cần người nắm lịch sử xác nhận". Mục Bằng chứng không được trống.

## Khi nào cần ghi

PHẢI ghi: chọn công nghệ/thư viện chính; đổi kiến trúc; đổi convention đã thống nhất; chọn tên chuẩn cho khái niệm có nhiều cách gọi; chọn giải pháp khi có nhiều cách hợp lý; bỏ qua best practice vì lý do thực tế; đổi contract API công khai hoặc schema DB.
KHÔNG ghi: tên biến, format code, việc chi tiết không ảnh hưởng người khác.

## Tra cứu (không duy trì mục lục viết tay)

```bash
ls docs/decision-logs                                # theo thời gian
grep -l "status: Active" docs/decision-logs/*.md     # đang hiệu lực
grep -l "FR-03" docs/decision-logs/*.md              # theo FR/NFR, module, thuật ngữ
grep -l "TEDU-123" docs/decision-logs/*.md           # theo mã thẻ
```
