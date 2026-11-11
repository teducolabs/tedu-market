# Phụ lục: Chuyển dự án đang dùng bộ v1.0 sang v2.0

> Áp dụng cho dự án đã khởi tạo bằng bộ v1.0 (có `docs/decisions.md` và `docs/status.md`).
> Thứ tự: [C1](c1-decision-logs.md) → prompt dưới đây → chạy lại [C2](c2-claude-md.md). C2 sẽ tự hợp nhất với CLAUDE.md cũ.

```text
Repo này đang dùng bộ tài liệu phiên bản cũ: có `docs/decisions.md` (nhật ký quyết định gộp 1 file) và `docs/status.md` (phase + checklist + Deferred Work + Blocker). Nhiệm vụ: chuyển sang cấu trúc mới. Chỉ thao tác trên tài liệu, không sửa code.

1. Nếu chưa có `docs/decision-logs/README.md` và `docs/decision-logs/_template.md`, dừng lại và báo tôi chạy Prompt C1 trước.

2. Tách `docs/decisions.md`: mỗi entry thành 1 file trong `docs/decision-logs/` theo format trong README. Ngày trong tên file là ngày entry được ghi: entry cũ có ngày thì dùng ngày đó; entry không có ngày thì dùng ngày của commit đầu tiên đưa entry đó vào (git log -S hoặc git blame). Giữ nguyên nội dung, chỉ chuyển sang format mới — không thêm, không bớt, không viết lại lý do. Trạng thái giữ như cũ; các tham chiếu Superseded trỏ sang tên file mới. Entry cũ không đủ các mục theo format mới thì để trống mục thiếu, không tự điền.

3. Từ `docs/status.md`, xuất ra chat (KHÔNG tạo file trong repo) danh sách thẻ Kanban đề xuất gồm: (a) các hạng mục chưa tick, (b) toàn bộ Deferred Work, (c) Blocker/Risk còn mở. Mỗi thẻ: tiêu đề, mô tả, mã FR/NFR liên quan, phase gốc, và với Deferred Work thì giữ nguyên lý do hoãn và mốc quay lại. Hạng mục đã tick nhưng ghi chú là thiếu test (nếu có) cũng đưa vào danh sách. Có MCP tới công cụ Kanban thì chỉ tạo thẻ khi tôi xác nhận.

4. Chỉ sau khi tôi xác nhận đã chuyển xong lên bảng Kanban: xóa `docs/decisions.md` và `docs/status.md`, rồi tìm và sửa mọi tham chiếu tới 2 file này trong docs/ (trừ nội dung các decision log đã Active — chỉ sửa link hỏng) và báo tôi danh sách chỗ đã sửa. Không tự sửa CLAUDE.md — tôi sẽ chạy lại Prompt C2.

5. CLAUDE.md mới nạp sẵn `docs/review-checklist.md` và `docs/domain-glossary.md` vào mọi session. Nếu chưa có `docs/review-checklist.md`: tạo từ mục Quy tắc review và Điều cấm tuyệt đối của `docs/conventions.md`, tối đa 40 dòng, mỗi dòng `- [ ] <điều kiện đúng/sai>`, không giải thích. Nếu `docs/domain-glossary.md` dài hơn ~150 dòng hoặc có văn xuôi/quy tắc ngoài bảng thuật ngữ: báo tôi và đề xuất cách rút gọn (mỗi thuật ngữ 1 dòng bảng), chưa tự sửa.
```
