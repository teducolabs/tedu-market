# CLAUDE.md

Quy tắc bắt buộc cho mọi session. Chi tiết nằm trong `docs/`; file này chỉ chứa quy tắc và đường dẫn.

Kanban: {{CONG_CU_KANBAN}} · Mã thẻ: {{FORMAT_MA_THE}} · Cột: {{CAC_COT}} · MCP Kanban: {{MCP_KANBAN}}

## 1. Nạp ngữ cảnh trước mỗi task (đọc theo phần, không đọc cả file khi không cần)

- `docs/domain-glossary.md` và `docs/review-checklist.md` đã nạp sẵn ở cuối file này. Không Read lại.
- `docs/prd.md`: `grep -n "FR-xx\|NFR-xx" docs/prd.md` với các mã liên quan thẻ — mỗi mã là 1 dòng bảng.
- `docs/architecture.md`: `grep -n "Module: <Tên>" docs/architecture.md` rồi Read đúng đoạn đó (offset/limit).
- `docs/conventions.md`: chỉ đọc mục ứng với lớp/loại file sắp sửa.
- `docs/decision-logs/`: `grep -l` theo mã FR/NFR, tên module, thuật ngữ, mã thẻ; đọc các file `status: Active` khớp — không đi ngược quyết định đã chốt.
- Task giao diện: BẮT BUỘC đọc `docs/ux.md` mục 4, 5 và `### Luồng:` liên quan.
- Task build/deploy/release/CI-CD: BẮT BUỘC đọc toàn bộ `docs/deploy.md`.
- `docs/status.md` (nếu có): bảng Kanban + backlog + chi tiết thẻ. `grep -n "T<số>" docs/status.md` rồi Read đúng đoạn của thẻ (mô tả, acceptance criteria, FR/NFR, thẻ phụ thuộc). Không đọc cả file.
- Không dựa vào ký ức session trước.
- File bắt buộc (`prd.md`, `domain-glossary.md`, `decision-logs/README.md`, `architecture.md`, `ux.md`, `deploy.md`, `conventions.md`, `review-checklist.md` trong `docs/`) không tồn tại hoặc trống → DỪNG, báo người dùng, không tự suy diễn.

## 2. Nguồn task là thẻ Kanban

- Mỗi task gắn 1 mã thẻ; người dùng không đưa thì hỏi, trừ khi họ nói rõ là việc ngoài bảng.
- Phạm vi = mô tả + acceptance criteria của thẻ (có `docs/status.md` thì đọc ở đó; có MCP thì đọc thẻ trực tiếp; không thì dùng nội dung người dùng dán). AC thiếu → hỏi, không tự đoán.
- Mỗi lúc 1 thẻ. Không nhận thêm thẻ, không làm việc của thẻ khác dù thấy liên quan.
- Tiếp tục thẻ từ session trước: đọc nhánh của thẻ (git log, diff với nhánh chính) và mô tả PR nếu có, tóm tắt trạng thái để người dùng xác nhận rồi mới làm tiếp.
- Không tự chuyển cột thẻ khi chưa được yêu cầu, kể cả khi MCP cho phép.
- Khi được yêu cầu chuyển cột với `docs/status.md`: sửa cả bảng Kanban ở đầu file và checkbox của thẻ trong cùng một commit. Thẻ thêm mới ghi vào `docs/status.md` với mã tiếp theo, không chèn giữa.

## 3. Definition of Done

- Xong CHỈ KHI có và pass: (a) unit test cho logic mới/đổi, (b) integration test cho luồng liên quan, (c) e2e test nếu ảnh hưởng luồng user-facing.
- Sửa bug: bắt buộc có test tái hiện bug (fail trước khi sửa, pass sau khi sửa).
- Thiếu tầng nào (khi áp dụng được) → CHƯA xong; báo rõ thiếu tầng nào, vì sao. Không báo "hoàn thành".
- Đạt DoD = thẻ đủ điều kiện sang Review, không phải Done. Done do người review quyết định.

## 4. Sau khi xong task (đúng thứ tự)

1. Chạy test, xác nhận pass.
2. Tự review theo `docs/review-checklist.md`.
3. Thay đổi làm 1 mô tả trong `docs/` sai đi (FR, module, thuật ngữ, bước deploy...) → sửa file đó trong cùng nhánh. Tài liệu lệch code là lỗi.
4. Báo cáo đúng mẫu:
   - **Thẻ:** mã thẻ + tóm tắt việc đã làm
   - **Test:** tầng đã có và pass; tầng thiếu và lý do
   - **Decision log:** file đã tạo/cập nhật (kể cả `Proposed` đang chờ xác nhận)
   - **Tài liệu đã cập nhật:** file, mục
   - **Việc phát sinh cần tạo thẻ:** việc cố ý hoãn, nợ kỹ thuật, bug ngoài phạm vi, test còn thiếu — mỗi mục: tiêu đề đề xuất, lý do, vị trí, thẻ gốc. Người dùng đồng ý thì tạo thẻ (thêm vào `docs/status.md`, hoặc qua MCP nếu có). Không bao giờ hoãn việc âm thầm.
   - **Blocker:** vướng mắc cần người dùng quyết định

Chỉ để `TODO(<MÃ-THẺ>): ...` trong code khi thẻ đã tồn tại; chưa có thẻ thì đưa vào mục Việc phát sinh.

## 5. Decision log

- Phải chọn giữa nhiều cách hợp lý (thư viện, lệch khỏi `docs/architecture.md`, vấn đề chưa có tiền lệ, bỏ qua best practice, đổi contract API/schema DB) → tạo file mới trong `docs/decision-logs/` theo `docs/decision-logs/README.md`, `cards` = mã thẻ đang làm. Tạo TRƯỚC khi báo hoàn thành.
- Chọn tạm khi người dùng chưa xác nhận → `status: Proposed`, nêu trong báo cáo.
- Thay thế quyết định cũ: file mới có `supersedes`; file cũ chỉ đổi `status` và `superseded_by`.
- Việc dưới ngưỡng trong README thì không ghi.

## 6. Thuật ngữ

- Mọi tên biến/class/entity/bảng/label UI cho khái niệm nghiệp vụ PHẢI truy về đúng 1 dòng trong glossary. Glossary là bảng map DUY NHẤT giữa tên hiển thị và tên code; không tự dịch, không viết tắt khác đi.
- Khái niệm mới: thêm 1 dòng vào glossary (đúng các cột) TRƯỚC khi dùng trong code/UI. Nhiều cách gọi hợp lý → decision log.
- Thấy code/UI dùng tên khác glossary: báo và hỏi nên sửa code hay glossary. Không sửa hàng loạt.

## 7. Giao diện

Màn hình/component mới khớp nguyên tắc, token, pattern trong `docs/ux.md`; dùng component có sẵn trước khi tạo mới. Cần pattern chưa có: bổ sung vào `docs/ux.md` trước (kèm decision log nếu đáng kể).

## 8. Deploy/release

Làm đúng tuần tự trong `docs/deploy.md`. TUYỆT ĐỐI KHÔNG tự deploy production, KHÔNG chạy lệnh tác động tới dữ liệu hoặc hạ tầng production (migration, script sửa dữ liệu, xóa cache, restart service) khi chưa có xác nhận rõ ràng của người dùng trong phiên hiện tại — kể cả khi test đã pass và pipeline cho phép chạy tự động.

## 9. Mâu thuẫn

Hai file trong `docs/` (hoặc tài liệu với code) mô tả khác nhau cho cùng 1 vấn đề, hoặc không thể đáp ứng 1 file vì lý do kỹ thuật → DỪNG và hỏi. Không tự chọn 1 bên, không sửa tài liệu để hợp thức hóa, không âm thầm sửa code cho khớp tài liệu (hay ngược lại).
Thứ tự ưu tiên tạm khi chưa hỏi kịp: `prd.md`, `domain-glossary.md` > `architecture.md`, `ux.md`, `deploy.md` > `conventions.md`. Quyết định tạm phải ghi decision log `Proposed` và được xác nhận lại.

## 10. Mục chưa rõ

Task chạm mục [CẦN LÀM RÕ], [CẦN XÁC NHẬN], [ĐỀ XUẤT - CẦN DUYỆT] chưa giải quyết trong `docs/` → hỏi trước khi code.

## 11. Phạm vi

Không implement thêm FR hay việc ngoài thẻ hiện tại, dù "tiện làm luôn".

<!-- BẮT ĐẦU MỤC 12 — chỉ giữ cho dự án đang vận hành; dự án mới thì xóa từ dòng này đến dòng KẾT THÚC MỤC 12 -->
## 12. Bảo trì hệ thống đang chạy production

- Diff tối thiểu: chỉ đổi những gì thẻ cần. Không format lại file, không đổi tên, không refactor ngoài phạm vi kể cả khi code xấu — đưa vào Việc phát sinh.
- Code mới theo "Chuẩn cho code mới" trong `docs/conventions.md`. Sửa trong code legacy thì giữ nhất quán với xung quanh, trừ Điều cấm tuyệt đối. Không nhân rộng pattern legacy.
- Đổi hành vi ở khu vực chưa có test: viết characterization test ghi nhận hành vi hiện tại, chạy pass, rồi mới sửa.
- Không đổi contract API công khai, schema DB, format message queue, tên biến môi trường/cấu hình khi thẻ không yêu cầu rõ. Nếu thẻ yêu cầu: phải tương thích ngược, migration expand → migrate → contract theo `docs/deploy.md`, và có decision log.
- Hotspot và khu vực nhạy cảm trong `docs/architecture.md` (mục Rủi ro & nợ kỹ thuật): trình bày kế hoạch và chờ xác nhận trước khi sửa.
- Hành vi lạ không có trong tài liệu: không tự coi là bug để sửa; báo người dùng, đề xuất thẻ.
- Hiểu tên cũ qua cột "Tên cũ" của glossary; code mới luôn dùng tên chuẩn.
<!-- KẾT THÚC MỤC 12 -->

## Quy tắc riêng của dự án

<!-- Hợp nhất từ CLAUDE.md cũ nếu có. Không có thì xóa mục này. -->

## Tài liệu nạp sẵn

@docs/domain-glossary.md
@docs/review-checklist.md
