# A2: Sinh `docs/domain-glossary.md`

> Luồng A · Chạy sau: A1 · Model: `opus`/`opusplan` · Effort: `high`/`xhigh` · Session mới
>
> File này được `@import` vào CLAUDE.md, tức là nằm trong context của mọi session, nên prompt ép format rất gọn.

```text
Đọc `docs/prd.md`, trích MỌI danh từ nghiệp vụ lặp lại từ 2 lần trở lên (thực thể, vai trò, trạng thái/quy trình, khái niệm riêng của dự án). Viết `docs/domain-glossary.md`: ngôn ngữ chung của dự án. Các file sinh sau và mọi code/UI copy phải dùng đúng tên ở đây, không dùng từ đồng nghĩa.

Cấu trúc:
1. Một dòng mục đích ở đầu file: nơi chốt DUY NHẤT 1 tên cho mỗi khái niệm nghiệp vụ, và là bảng map duy nhất giữa tên hiển thị và tên trong code.
2. Bảng `| Hiển thị | Code | Định nghĩa | KHÔNG gọi là | Liên quan |`, nhóm theo domain con (`## <Domain>`) nếu có nhiều domain.
   - Hiển thị / Code: vd "Học viên" / `Student`.
   - Định nghĩa: theo ngữ cảnh dự án, không quá 25 từ.
   - KHÔNG gọi là: từ đồng nghĩa dễ dùng nhầm (vd người dùng, user, khách hàng).
   - Liên quan: thuật ngữ khác trong bảng.
   - Đồng âm khác nghĩa giữa 2 domain: tách 2 dòng, ghi rõ ở cột Định nghĩa.

Định dạng: file này được nạp vào context ở MỌI session, nên mỗi thuật ngữ đúng 1 dòng bảng; ngoài dòng mục đích không viết văn xuôi; không chép quy tắc hành vi cho agent (đã có trong CLAUDE.md). Mục tiêu dưới ~150 dòng; vượt thì báo tôi.

PRD dùng nhiều tên cho cùng 1 khái niệm (vd lúc "Học viên" lúc "Người học") → hỏi tôi chốt 1 tên, không tự chọn rồi im lặng.
```
