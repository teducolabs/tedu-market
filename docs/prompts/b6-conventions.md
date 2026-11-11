# B6: Tái dựng `docs/conventions.md` + `docs/review-checklist.md`

> Luồng B · Chạy sau: B5 · Điền tay: công cụ Kanban + format mã thẻ · Model: `opus` · Effort: `xhigh` · Session mới
>
> Tách 2 file: `review-checklist.md` ngắn, được `@import` vào CLAUDE.md để dùng sau mọi task; `conventions.md` chi tiết, agent chỉ đọc mục liên quan.

```text
Đọc `docs/_discovery/06-code-patterns.md`, `07-tests.md`; `docs/domain-glossary.md`; mục Tech Stack và Module của `docs/architecture.md`; mục 5 của `docs/ux.md`. Mở trực tiếp đủ file mẫu ở mỗi lớp để kiểm chứng mẫu hình, không chỉ tin số liệu discovery. Đọc cấu hình linter/formatter/analyzer đang có (.editorconfig, eslint, prettier, Directory.Build.props, analyzer rules...). Viết 2 file chuẩn code để agent tự review, cụ thể đến mức trả lời được đúng/sai máy móc.

Thông tin điền tay: công cụ Kanban là [......], format mã thẻ là [ví dụ TEDU-123].

Code hiện tại thường không nhất quán, nên tách rõ, không trộn:
- **Chuẩn cho code mới** — bắt buộc cho file, class, hàm, endpoint, màn hình MỚI. Mặc định theo mẫu hình chiếm đa số (kèm số liệu); chọn khác đa số chỉ khi tôi xác nhận, và phải có decision log.
- **Biến thể legacy được chấp nhận** — mẫu hình thiểu số đang tồn tại, kèm vị trí. Không nhân rộng sang code mới; sửa trong code legacy thì giữ nhất quán với xung quanh, trừ Điều cấm tuyệt đối; không tự chuyển legacy sang chuẩn mới khi đang làm thẻ khác.

**File 1: `docs/conventions.md`** — giữ đúng tiêu đề đánh số; mỗi mục tách `### Chuẩn` / `### Legacy` khi áp dụng.
1. **Naming** — file, class, method, biến, bảng/cột, API endpoint, nhánh git, commit message. Nhánh và commit BẮT BUỘC chứa mã thẻ. Tên nghiệp vụ truy về cột Code của glossary. Ví dụ đúng/sai lấy từ chính code hiện tại.
2. **Cấu trúc thư mục** — cây thật, quy tắc đặt file mới vào đâu.
3. **Coding style** — đã có cấu hình linter/formatter thì tham chiếu cấu hình đó, không chép bằng lời; chưa có thì ghi vào cuối file mục "Việc cần tạo thẻ", không tự thêm cấu hình.
4. **Error handling** — cách đang làm, chuẩn cho code mới, cấm silent fail.
5. **Testing** — theo framework đang có, cộng 2 quy tắc bảo trì: (a) sửa bug phải có test tái hiện bug, fail trước pass sau; (b) sửa hành vi ở khu vực chưa có test thì viết characterization test TRƯỚC khi đổi. Đặt mức coverage cho code mới và code bị sửa, không đặt cho toàn repo, vì mục tiêu không đạt được sẽ bị bỏ qua.
6. **Điều cấm tuyệt đối** — áp dụng cả code mới lẫn khi sửa code cũ; gồm: cấm nhân rộng pattern legacy, cấm đổi tên hàng loạt theo glossary ngoài thẻ refactor.
Không quá ~250 dòng.

**File 2: `docs/review-checklist.md`** — không quá 40 dòng, mỗi dòng `- [ ] <điều kiện đúng/sai>`, không giải thích, không ví dụ. Tối thiểu gồm: diff chỉ chứa thay đổi thuộc thẻ (không format lại cả file, không đổi tên, không refactor ngoài phạm vi); không đổi contract API công khai, schema DB, format message, tên biến môi trường khi thẻ không yêu cầu; code mới theo Chuẩn, không theo Legacy; có test tái hiện bug / characterization test khi áp dụng; không có TODO thiếu mã thẻ; không còn log debug; không hardcode secret; tên nghiệp vụ khớp glossary; mỗi Điều cấm tuyệt đối 1 dòng (chi tiết dẫn về conventions.md mục 6).

Không chép quy tắc quy trình cho agent (Kanban, DoD, báo cáo) vào 2 file này, vì đã có trong CLAUDE.md.
```
