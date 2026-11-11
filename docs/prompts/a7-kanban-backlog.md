# A7 (tùy chọn): Tạo `docs/status.md` làm backlog và theo dõi tiến độ

> Luồng A · Chạy sau: C2 · Model: `opus`/`opusplan` · Effort: `high`/`xhigh` · Session mới
>
> Prompt này rà toàn bộ tài liệu để chia việc cho đủ, không sót mảng hạ tầng/UI, rồi ghi kết quả vào `docs/status.md` trong repo. `docs/status.md` là nơi duy nhất theo dõi tiến độ, trình bày theo kiểu bảng Kanban bằng Markdown. Không dùng công cụ ngoài (MCP Kanban, file CSV import, dịch vụ bên thứ ba).

```text
Nhiệm vụ: tạo file `docs/status.md`, chứa backlog ban đầu và dùng để theo dõi tiến độ. Chỉ dùng công cụ sẵn có của Claude Code (đọc/ghi file); KHÔNG gọi MCP hay công cụ Kanban bên ngoài, KHÔNG xuất CSV.

Đọc (glossary và review-checklist đã nạp qua CLAUDE.md): `docs/prd.md`; mục Module và Data model của `docs/architecture.md`; mục 2-3 của `docs/ux.md`; mục 1-6 của `docs/deploy.md`; mục 5 của `docs/conventions.md`. Với `docs/decision-logs/` chỉ đọc front matter (`head -12`), không đọc nội dung.

Yêu cầu:
- Bao trùm mọi mảng, không chỉ backend: có thẻ dựng hạ tầng/CI-CD theo deploy.md, đứng trước mọi thẻ cần deploy thật; mỗi tính năng user-facing có thẻ UI riêng theo ux.md.
- Mỗi thẻ đủ nhỏ để xong trong 1 phiên với agent và đạt DoD. Thẻ quá lớn thì tách.
- Mỗi thẻ: mã thẻ (T01, T02...), tiêu đề (đúng thuật ngữ glossary), mô tả 1-3 câu, acceptance criteria dạng hành vi quan sát được, mã FR/NFR, thẻ phụ thuộc, epic.
- Sắp theo thứ tự phụ thuộc (nền tảng trước, vd Identity trước Billing), không theo thứ tự mã FR. Chia thành các phase, mỗi phase gom các thẻ có thể làm song song.
- Mọi FR/NFR phải có ít nhất 1 thẻ.

Cấu trúc `docs/status.md` (link tới tài liệu khác dùng đường dẫn tương đối trong `docs/`, vd `[prd.md](prd.md)`):
1. Bảng tiến độ kiểu Kanban ở đầu file, 4 cột: Backlog | Đang làm | Review | Xong. Mỗi ô liệt kê mã thẻ và tiêu đề ngắn. Ban đầu mọi thẻ nằm ở Backlog.
2. Danh sách phase, mỗi phase có checklist `- [ ] T01 Tiêu đề` kèm epic và thẻ phụ thuộc.
3. Chi tiết từng thẻ (mô tả, acceptance criteria, FR/NFR) đặt dưới checklist hoặc ở mục riêng cuối file.
4. Bảng đối chiếu FR/NFR → thẻ, và FR/NFR chưa có thẻ kèm lý do.
5. Quy ước cập nhật: khi đổi trạng thái thẻ, sửa cả bảng Kanban và checkbox trong cùng một commit.

Sau khi ghi file, tóm tắt trong chat: số phase, số thẻ, FR/NFR chưa có thẻ (nếu có). Không commit; chờ tôi xác nhận.
```
