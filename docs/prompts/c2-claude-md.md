# C2: Sinh `CLAUDE.md`

> Dùng chung cho cả 2 luồng, chạy sau khi đã có đủ file trong `docs/` (kể cả `review-checklist.md` do A6/B6 sinh) · Model: `sonnet` là đủ (chỉ điền và hợp nhất) · Effort: `medium`
>
> Các quy tắc trong `CLAUDE.md` là văn bản cố định, nằm sẵn ở [templates/CLAUDE.md](templates/CLAUDE.md). Copy template rồi chỉ sửa chỗ trống, không để model viết lại cả file, vì viết lại vừa tốn token đầu ra vừa có thể làm lệch câu chữ quy tắc.

## Cách chạy

**Bước 1: copy template** (ở gốc repo, `<KIT>` là thư mục `docs/prompts` của bộ prompt):

```text
# Repo CHƯA có CLAUDE.md:
cp <KIT>/templates/CLAUDE.md CLAUDE.md
# Repo ĐÃ có CLAUDE.md (cần hợp nhất):
cp <KIT>/templates/CLAUDE.md CLAUDE.v2.md
```

**Bước 2:** dự án mới, chưa có CLAUDE.md cũ: tự điền 4 chỗ `{{...}}` ở dòng đầu file và xóa khối mục 12 là xong, không cần chạy prompt. Các trường hợp còn lại thì dán prompt sau:

```text
=== THÔNG TIN ĐIỀN TAY ===
- Loại dự án: [Dự án mới | Dự án đang vận hành]
- Công cụ Kanban: [Jira | Linear | GitHub Projects | Trello | ...]
- Format mã thẻ: [ví dụ TEDU-123]
- Các cột trên bảng: [ví dụ Backlog → To Do → In Progress → Review → Done]
- MCP tới công cụ Kanban: [Có — tên server | Chưa]
=== HẾT ===

Nhiệm vụ: hoàn thiện CLAUDE.md từ template đã copy sẵn. Chỉ dùng Edit trên các chỗ cần đổi, KHÔNG viết lại cả file, KHÔNG diễn đạt lại các quy tắc có sẵn.

1. File làm việc: `CLAUDE.v2.md` nếu tồn tại, không thì `CLAUDE.md` (ở gốc repo).
2. Thay 4 chỗ `{{...}}` ở đầu file bằng thông tin trên.
3. Loại "Dự án mới": xóa khối từ `<!-- BẮT ĐẦU MỤC 12` đến `<!-- KẾT THÚC MỤC 12 -->`. Loại "Dự án đang vận hành": giữ nội dung mục 12, chỉ xóa 2 dòng comment đánh dấu.
4. Kiểm tra bằng `ls` (không Read nội dung) rằng 8 file bắt buộc nêu ở mục 1 đều tồn tại trong `docs/`. Thiếu file nào thì báo tôi.
5. Nếu có `CLAUDE.v2.md` (tức là đã có CLAUDE.md cũ): đọc CLAUDE.md cũ, chia các quy tắc cũ thành 3 nhóm: (a) đã có trong template → bỏ; (b) còn đúng và chưa có → chép vào mục "Quy tắc riêng của dự án", viết gọn dạng chỉ dẫn; (c) mâu thuẫn với template → liệt kê cho tôi chọn, không tự bỏ. Sau khi tôi chọn xong: ghi đè `CLAUDE.md` bằng nội dung `CLAUDE.v2.md` rồi xóa `CLAUDE.v2.md`.
6. Mục "Quy tắc riêng của dự án" vẫn trống thì xóa cả tiêu đề mục đó.
7. Báo số dòng của CLAUDE.md. Vượt 200 dòng thì đề xuất chuyển phần nào xuống docs/.
```

## Vì sao template viết như vậy (tối ưu token và cache)

- `docs/domain-glossary.md` và `docs/review-checklist.md` được nạp bằng `@import` ở cuối CLAUDE.md. Task nào cũng cần 2 file này, nên chúng nằm trong phần prefix được prompt cache dùng lại **giữa các session**. Nếu để agent Read ở đầu mỗi task thì session nào cũng phải ghi cache lại từ đầu, và còn tốn thêm lượt gọi tool.
- Các file khác chỉ đọc theo phần (grep mã FR, `Module: <Tên>`) để không nạp cả file vào context.
- CLAUDE.md không chứa ngày tháng hay trạng thái task. Nội dung ổn định thì cache mới hit được giữa các session.
