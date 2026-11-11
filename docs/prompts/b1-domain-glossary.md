# B1: Tái dựng `docs/domain-glossary.md`

> Luồng B · Chạy sau: B0 (đã trả lời câu hỏi mở) · Model: `opus` · Effort: `xhigh` · Session mới
>
> Khác luồng A, ở đây glossary đi trước PRD. Với dự án đang chạy, tên gọi đã có sẵn trong code, DB và UI, nên việc cần làm là ghi nhận tên đang dùng và chỉ ra chỗ lệch.
>
> Glossary được `@import` vào CLAUDE.md (nằm trong context mọi session), nên danh sách "nợ thuật ngữ" (dài, chỉ dùng để tạo thẻ) được ghi ra file tạm riêng, không để trong glossary.

```text
Đọc `docs/_discovery/04-data-model.md`, `05-vocabulary.md`, `12-open-questions.md` (kể cả câu trả lời), `docs/decision-logs/README.md`, và tài liệu cũ nếu có. Viết `docs/domain-glossary.md` cho hệ thống đang vận hành: ghi nhận tên đang dùng, chốt tên chuẩn, lập danh sách chỗ lệch. KHÔNG đổi tên bất kỳ thứ gì trong code. Đã có `docs/domain-glossary.md` thì hợp nhất, không ghi đè.

**File 1: `docs/domain-glossary.md`**
1. Một dòng mục đích ở đầu file: nơi chốt DUY NHẤT 1 tên cho mỗi khái niệm nghiệp vụ, áp dụng cho code và UI MỚI; là bảng map duy nhất giữa tên hiển thị và tên code. Nếu hệ thống có file i18n/resource, thêm 1 dòng: file i18n chứa text hiển thị thực tế, glossary chốt tên, 2 nơi phải khớp.
2. Bảng `| Hiển thị | Code | Định nghĩa | KHÔNG gọi là | Tên cũ | Liên quan |`, nhóm theo domain con (`## <Domain>`) nếu có.
   - Code: lấy tên đang dùng phổ biến nhất trong code, không lấy tên "đẹp hơn" do bạn nghĩ ra (đổi tên là refactor có chi phí thật, cần thẻ và decision log riêng).
   - Định nghĩa: theo hành vi code đang thể hiện, không quá 25 từ, kèm nhãn [TỪ CODE]/[SUY LUẬN]/[CẦN XÁC NHẬN].
   - KHÔNG gọi là: tên đồng nghĩa cấm dùng trong code/UI mới.
   - Tên cũ: tên khác còn tồn tại trong code/DB/UI cho cùng khái niệm (chỉ tên, không ghi vị trí). Cột này giúp agent đọc hiểu code cũ mà không nhân rộng tên cũ.
   - Đồng âm khác nghĩa giữa 2 domain: tách dòng, ghi rõ.
Định dạng: mỗi thuật ngữ đúng 1 dòng bảng, không văn xuôi ngoài dòng mục đích, không chép quy tắc hành vi cho agent (đã có trong CLAUDE.md). Mục tiêu dưới ~150 dòng; vượt thì báo tôi.

**File 2: `docs/_discovery/13-terminology-debt.md`** (file tạm, B7 chuyển thành thẻ Kanban) — mỗi dòng: vị trí, tên đang dùng, tên chuẩn, mức ảnh hưởng (chỉ nội bộ code / lộ ra API công khai / lộ ra UI / tên bảng hoặc cột DB).

Cùng 1 khái niệm có nhiều tên mà không xác định được tên chuẩn (dùng ngang nhau, hoặc tên phổ biến nhất lại sai nghĩa) → hỏi tôi, không tự chọn. Mỗi lần tôi chốt tên chuẩn khác tên phổ biến nhất trong code → tạo decision log.
```
