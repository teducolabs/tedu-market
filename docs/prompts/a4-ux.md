# A4: Sinh `docs/ux.md`

> Luồng A · Chạy sau: A3 · Model: `opus`/`opusplan` · Effort: `high`/`xhigh` · Session mới

```text
Đọc `docs/prd.md` (Đối tượng người dùng, FR), `docs/domain-glossary.md`, và trong `docs/architecture.md` chỉ phần frontend của Tech Stack. Viết `docs/ux.md`: cơ sở để bạn sinh UI nhất quán khi implement. Mọi label, tên màn hình, tên trạng thái dùng đúng cột Hiển thị của glossary. Chưa có brand guideline thì đề xuất baseline theo persona và đánh [ĐỀ XUẤT - CẦN DUYỆT].

Cấu trúc (giữ đúng tiêu đề đánh số):
1. **Nguyên tắc thiết kế** — 3-5 nguyên tắc gắn với persona (vd người dùng không rành kỹ thuật → ít bước, tránh thuật ngữ chuyên môn). Không viết chung chung kiểu "đẹp và dễ dùng".
2. **Information Architecture** — cây điều hướng; mỗi màn hình: tên theo glossary + mã FR.
3. **Luồng chính** — mỗi luồng 1 tiểu mục `### Luồng: <Tên>`: các bước; trạng thái loading/empty/error/success bắt buộc phải có.
4. **Design tokens** — bảng: màu (primary/secondary/success/warning/error), typography scale, spacing scale, breakpoint.
5. **Component pattern** — form, table, modal, toast, empty state, skeleton: hành vi mặc định, khi nào dùng loại nào.
6. **Accessibility tối thiểu** — contrast, keyboard, label cho input, alt text. Chỉ ghi mức tối thiểu, không chép checklist WCAG.
7. **Tone of voice** — xưng hô, mức trang trọng, ngôn ngữ; áp dụng cho label, thông báo lỗi, thông báo hệ thống.
8. **Responsive** — hành vi tối thiểu theo breakpoint nếu cần đa nền tảng.

Định dạng: không quá ~250 dòng, ưu tiên bảng và gạch đầu dòng. Khi làm UI, agent chỉ đọc mục 4, 5 và luồng liên quan, nên mỗi mục phải tự đứng được. Không chép quy tắc hành vi cho agent (đã có trong CLAUDE.md).

Thuật ngữ hiển thị chưa có trong glossary → bổ sung vào glossary trước. Lựa chọn đáng kể (UI library, design system nền) → decision log.
```
