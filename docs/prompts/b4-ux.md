# B4: Tái dựng `docs/ux.md`

> Luồng B · Chạy sau: B3 · Model: `opus` · Effort: `xhigh` · Session mới

```text
Đọc `docs/_discovery/03-capabilities.md` (chỉ phần `## Frontend`), `05-vocabulary.md`, `10-anomalies.md`, `12-open-questions.md`; `docs/prd.md` (Đối tượng người dùng, bảng FR); `docs/domain-glossary.md`; phần frontend trong Tech Stack của `docs/architecture.md`. Sau đó đọc trực tiếp code frontend: file theme/design token (biến CSS, cấu hình Tailwind, theme UI library), component dùng chung, layout, file i18n, và 3-5 màn hình tiêu biểu. Viết `docs/ux.md` ghi nhận hệ thống giao diện đang có, để màn hình mới hoặc màn hình được sửa trông và hoạt động giống phần còn lại; không thiết kế lại. Hệ thống không có frontend (chỉ API) thì ghi rõ điều đó, chỉ làm mục 3 (luồng + lỗi trả về) và mục 7 (message lỗi API), rồi dừng.

Cấu trúc (giữ đúng tiêu đề đánh số):
1. **Nguyên tắc thiết kế** — suy ra từ UI hiện tại và persona; gắn [SUY LUẬN] để tôi duyệt.
2. **Information Architecture** — cây điều hướng thật (từ router/menu); mỗi màn hình: tên theo glossary + mã FR.
3. **Luồng chính** — mỗi luồng 1 tiểu mục `### Luồng: <Tên>`: các bước, trạng thái loading/empty/error/success ĐANG có. Thiếu trạng thái nào thì ghi vào mục 9, không mô tả như thể đã có.
4. **Design tokens** — bảng giá trị thật từ code (màu, font, spacing, breakpoint). Cùng 1 vai trò có nhiều giá trị thì giá trị chuẩn là giá trị trong file theme/token; giá trị hardcode lệch ghi vào mục 9. Không có file token tập trung thì ghi rõ, đề xuất baseline từ giá trị phổ biến nhất, đánh [ĐỀ XUẤT - CẦN DUYỆT].
5. **Component pattern** — bảng component dùng chung (tên, đường dẫn, khi nào dùng) và pattern lặp lại (form, table, modal, toast, empty state, skeleton). Màn hình mới BẮT BUỘC dùng component có sẵn; 2 component cùng làm 1 việc thì ghi cái nào là chuẩn cho code mới.
6. **Accessibility** — mức thực tế đang có; khoảng trống ghi vào mục 9.
7. **Tone of voice** — rút từ text trong file i18n (xưng hô, mức trang trọng).
8. **Responsive** — hành vi thật theo breakpoint trong code.
9. **Khoảng trống UX** — chỗ lệch chuẩn hoặc thiếu trạng thái, mỗi dòng có vị trí. Đây là nguyên liệu tạo thẻ, không sửa ngay.

Định dạng: không quá ~250 dòng, ưu tiên bảng. Khi làm UI, agent chỉ đọc mục 4, 5 và luồng liên quan, nên mỗi mục phải tự đứng được. Không chép quy tắc hành vi cho agent (đã có trong CLAUDE.md).
```
