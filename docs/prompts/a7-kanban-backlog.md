# A7 (tùy chọn): Đề xuất backlog ban đầu cho bảng Kanban

> Luồng A · Chạy sau: C2 · Model: `opus`/`opusplan` · Effort: `high`/`xhigh` · Session mới
>
> v1.0 dùng `status.md` để chia phase và checklist. Prompt này giữ lại bước rà toàn bộ tài liệu để chia việc cho đủ, không sót mảng hạ tầng/UI, nhưng đầu ra là thẻ Kanban chứ không phải file trong repo.

```text
Nhiệm vụ: đề xuất backlog ban đầu cho bảng Kanban. KHÔNG tạo file theo dõi tiến độ trong repo.

Đọc (glossary và review-checklist đã nạp qua CLAUDE.md): `docs/prd.md`; mục Module và Data model của `docs/architecture.md`; mục 2-3 của `docs/ux.md`; mục 1-6 của `docs/deploy.md`; mục 5 của `docs/conventions.md`. Với `docs/decision-logs/` chỉ đọc front matter (`head -12`), không đọc nội dung.

Yêu cầu:
- Bao trùm mọi mảng, không chỉ backend: có thẻ dựng hạ tầng/CI-CD theo deploy.md, đứng trước mọi thẻ cần deploy thật; mỗi tính năng user-facing có thẻ UI riêng theo ux.md.
- Mỗi thẻ đủ nhỏ để xong trong 1 phiên với agent và đạt DoD. Thẻ quá lớn thì tách.
- Mỗi thẻ: tiêu đề (đúng thuật ngữ glossary), mô tả 1-3 câu, acceptance criteria dạng hành vi quan sát được, mã FR/NFR, thẻ phụ thuộc, epic.
- Sắp theo thứ tự phụ thuộc (nền tảng trước, vd Identity trước Billing), không theo thứ tự mã FR.
- Mọi FR/NFR phải có ít nhất 1 thẻ. Cuối danh sách: bảng đối chiếu FR/NFR → thẻ, và FR/NFR chưa có thẻ kèm lý do.

Xuất ra chat dạng bảng, mỗi thẻ 1 dòng. Tôi yêu cầu thì xuất thêm file tạm `backlog-import.csv` theo định dạng import của công cụ Kanban (KHÔNG commit). Có MCP tới công cụ Kanban thì chỉ tạo thẻ sau khi tôi xác nhận danh sách.
```
