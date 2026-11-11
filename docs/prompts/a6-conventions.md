# A6: Sinh `docs/conventions.md` + `docs/review-checklist.md`

> Luồng A · Chạy sau: A5 · Điền tay: công cụ Kanban + format mã thẻ · Model: `opus`/`opusplan` · Effort: `high`/`xhigh` · Session mới
>
> Tách 2 file: `review-checklist.md` ngắn, được `@import` vào CLAUDE.md để dùng sau mọi task; `conventions.md` chi tiết, agent chỉ đọc mục liên quan.

```text
Đọc trong `docs/prd.md` bảng NFR (độ tin cậy/chất lượng), `docs/domain-glossary.md`, trong `docs/architecture.md` các mục Tech Stack và Module, trong `docs/ux.md` mục 5. Viết 2 file chuẩn code để bạn tự review, cụ thể đến mức áp dụng máy móc được.

Thông tin điền tay: công cụ Kanban là [......], format mã thẻ là [ví dụ TEDU-123].

**File 1: `docs/conventions.md`** — chỉ cho stack trong architecture.md. Giữ đúng tiêu đề đánh số; mục nào backend và frontend khác nhau thì tách `### Backend` / `### Frontend`.
1. **Naming** — file, class, function, biến, bảng/cột DB, API endpoint, nhánh git, commit message; mỗi loại 1 ví dụ đúng và 1 ví dụ sai. Nhánh và commit BẮT BUỘC chứa mã thẻ (vd `feature/TEDU-123-dang-ky-hoc-vien`, `TEDU-123: thêm API đăng ký học viên`). Nêu quy tắc chuyển tên từ cột Code của glossary sang tên class/biến/bảng (PascalCase, camelCase, snake_case...).
2. **Cấu trúc thư mục** — cây chuẩn, lớp nào chứa gì, map với Module trong architecture.md.
3. **Coding style** — quy tắc nào cưỡng chế được bằng formatter/linter/analyzer thì ghi cấu hình công cụ, không mô tả bằng lời.
4. **Error handling** — cách throw/catch, format lỗi trả ra API, cấm silent fail.
5. **Testing** — đặt tên test, Arrange-Act-Assert, chiến lược mock, coverage tối thiểu cho unit/integration/e2e tương xứng với NFR độ tin cậy. Sửa bug bắt buộc có test tái hiện bug.
6. **Điều cấm tuyệt đối** — pattern bị cấm (vd business logic trong controller, frontend gọi DB trực tiếp, tên entity nghiệp vụ khác glossary...).
Không quá ~250 dòng.

**File 2: `docs/review-checklist.md`** — không quá 40 dòng, mỗi dòng `- [ ] <điều kiện trả lời được đúng/sai>`, dùng để tự chấm trước khi báo done. Tối thiểu gồm: không hardcode secret; không có TODO thiếu mã thẻ; không còn console.log/print debug; không viết tắt tùy tiện; tên nghiệp vụ khớp glossary; diff chỉ chứa thay đổi thuộc thẻ đang làm; đủ test theo conventions.md mục 5; mỗi Điều cấm tuyệt đối 1 dòng (chi tiết dẫn về conventions.md mục 6). Không giải thích, không ví dụ.

Không chép quy tắc quy trình cho agent (Kanban, DoD, báo cáo) vào 2 file này, vì đã có trong CLAUDE.md.
```
