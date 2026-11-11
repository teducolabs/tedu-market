# A3: Sinh `docs/architecture.md`

> Luồng A · Chạy sau: A2 · Model: `opus`/`opusplan` · Effort: `high`/`xhigh` · Session mới

```text
Đọc `docs/prd.md`, `docs/domain-glossary.md`, `docs/decision-logs/README.md`. Viết `docs/architecture.md`. Kiến trúc phải giải quyết đúng FR/NFR trong prd.md và hợp với ràng buộc thực tế đã nêu (team size, deadline, ràng buộc kỹ thuật), không phải kiến trúc "lý tưởng". Tên module, entity, bảng dùng đúng cột Code của glossary.

Cấu trúc (giữ đúng tiêu đề):
1. **Kiến trúc tổng quan** — vài đoạn văn + 1 diagram mermaid: thành phần chính, luồng dữ liệu.
2. **Tech Stack** — bảng `| Thành phần | Lựa chọn | Lý do (gắn NFR) | Decision log |`. Lựa chọn không có lý do gắn NFR thì không đưa vào.
3. **Module** — mỗi module 1 tiểu mục `### Module: <TênCode>`, không quá ~12 dòng: trách nhiệm, FR phụ trách, thuật ngữ glossary, phụ thuộc.
4. **Bảo mật** — authn/authz, xử lý secret, threat mặc định phải chống (injection, IDOR, rate limit...), map với NFR bảo mật.
5. **Performance** — target từ NFR và cách đạt (cache, index, async...).
6. **Data model tổng quan** — entity chính và quan hệ (mermaid erDiagram hoặc bảng), không liệt kê field. Tên kỹ thuật thuần túy không có trong glossary thì ghi rõ "tên kỹ thuật nội bộ".
7. **Rủi ro kỹ thuật** — trade-off, rủi ro khi scale, và mọi FR/NFR mà kiến trúc chưa đáp ứng được (không im lặng bỏ qua).

Định dạng: không quá ~250 dòng. Agent đọc file theo phần (`grep "Module: <Tên>"`), nên giữ đúng tiêu đề `### Module: <TênCode>`. Không chép lại nội dung của prd/glossary, chỉ dẫn mã FR/NFR và tên thuật ngữ.

Mỗi lựa chọn quan trọng có phương án thay thế hợp lý (database, framework, auth, kiểu kiến trúc, hosting...): tạo 1 decision log theo `docs/decision-logs/README.md`; architecture.md chỉ ghi quyết định + link, không chép lý do. Glossary thiếu thuật ngữ để đặt tên entity/module nghiệp vụ → bổ sung vào glossary trước.
```
