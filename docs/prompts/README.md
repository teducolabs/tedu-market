# Bộ prompt khởi tạo & tái dựng tài liệu dự án cho Claude Code (v2.0)

> Nguồn: "TEDU - Bộ Prompt Khởi Tạo và Tái Dựng Tài Liệu Dự Án cho Claude Code v2.0" (Toan soạn, 2026-09-24, thay thế v1.0 ngày 2026-08-03).
> Áp dụng cho dự án mới (luồng A) và dự án đang vận hành (luồng B), quản lý công việc theo Kanban.

**Cách dùng:** mở repo của dự án cần làm tài liệu, chạy lần lượt theo checklist bên dưới. Mỗi file ở đây là một prompt độc lập: copy nguyên khối `text` và dán cho Claude Code. **Mỗi prompt chạy trong một session mới (hoặc sau `/clear`)**, và **chạy xong prompt nào thì commit file đó** rồi mới qua prompt kế tiếp, vì prompt sau sẽ đọc file của prompt trước. C1 và phần lớn C2 không cần prompt: copy từ [templates/](templates/).

## Bộ tài liệu đầu ra (trong repo dự án)

| File | Trả lời câu hỏi | Ghi chú |
|---|---|---|
| `docs/prd.md` | Làm gì, cho ai, đo thành công thế nào | Nguồn sự thật về yêu cầu. Mã FR/NFR không đổi giữa chừng vì thẻ Kanban và decision log tham chiếu tới |
| `docs/domain-glossary.md` | Mỗi khái niệm nghiệp vụ gọi tên gì | Bảng map duy nhất giữa tên hiển thị tiếng Việt và tên trong code |
| `docs/decision-logs/` | Tại sao lúc đó lại chọn thế | Mỗi quyết định 1 file. Không sửa nội dung log cũ, chỉ đổi trạng thái |
| `docs/architecture.md` | Làm bằng gì, chia module ra sao | |
| `docs/ux.md` | Giao diện trông và hoạt động ra sao | Cơ sở để agent sinh UI nhất quán |
| `docs/deploy.md` | Đưa lên môi trường thật thế nào | Agent phải đọc trước mọi việc build/deploy/release |
| `docs/conventions.md` | Viết code thế nào | Chi tiết, agent đọc theo mục |
| `docs/review-checklist.md` | Tự chấm gì trước khi báo xong | Tối đa 40 dòng, nạp sẵn vào mọi session qua CLAUDE.md |
| `CLAUDE.md` (gốc repo) | Luật chơi của agent | Copy từ [templates/CLAUDE.md](templates/CLAUDE.md). Buộc agent đọc và tuân theo các file trên trong mọi session |
| Bảng Kanban (ngoài repo) hoặc `docs/status.md` (tùy chọn, tạo bằng A7) | Đang làm gì, còn gì, việc gì bị hoãn hoặc bị chặn | Có `docs/status.md` thì đó là nơi duy nhất theo dõi tiến độ, không dùng song song bảng ngoài |

- **Luồng A, dự án mới:** viết tài liệu trước, code sau. Tài liệu là bản thiết kế, anh quyết định nội dung.
- **Luồng B, dự án đang vận hành:** code có trước, tài liệu được tái dựng từ code. Đây là bản mô tả hiện trạng (as-is) có gắn nhãn mức độ chắc chắn, và phải được người nắm hệ thống review trước khi agent coi đó là luật.

## Bước 0: trước khi chạy prompt nào

### 0.1 Cấu trúc thư mục

```
<gốc repo>/
├── CLAUDE.md                ← bắt buộc ở gốc repo
└── docs/
    ├── prd.md
    ├── domain-glossary.md
    ├── architecture.md
    ├── ux.md
    ├── deploy.md
    ├── conventions.md
    ├── review-checklist.md
    └── decision-logs/
        ├── README.md        ← mục đích, quy tắc, ngưỡng ghi
        ├── _template.md     ← mẫu 1 quyết định
        └── YYYY-MM-DD-<slug>.md
```

`CLAUDE.md` bắt buộc nằm ở gốc repo. Nếu đặt trong `docs/`, Claude Code sẽ không tự nạp và mọi ràng buộc trong đó mất hiệu lực mà không có cảnh báo nào. Tạo sẵn thư mục bằng lệnh:

```text
mkdir -p docs/decision-logs
```

Với luồng B: nếu repo đã có `docs/` thì vẫn dùng thư mục đó, vì các prompt luồng B đều đọc và hợp nhất chứ không ghi đè. Nếu đã có `CLAUDE.md`, prompt C2 sẽ hợp nhất.

### 0.2 Model

Chọn bằng `/model <alias>`. Ghi theo alias vì mỗi alias trỏ tới phiên bản nào là tùy provider và thay đổi theo thời gian. Muốn cố định cho cả team thì dùng `ANTHROPIC_DEFAULT_OPUS_MODEL` / `ANTHROPIC_DEFAULT_SONNET_MODEL`. Chi tiết: https://code.claude.com/docs/en/model-config.

| Việc | Model | Vì sao |
|---|---|---|
| Luồng A (A1–A7) | `opus` hoặc `opusplan` | Đây là quyết định nền tảng, sai ở bước này thì kéo sai toàn bộ phía sau |
| C1 / C2 | C1 không cần model; C2 dùng `sonnet` | Chỉ copy template, điền chỗ trống và hợp nhất |
| Luồng B (B0–B7) | `opus`, repo lớn thì cân nhắc `opus[1m]` hoặc `fable` | Phải đọc nhiều file và suy ra ý đồ từ code |
| Implement 1 thẻ Kanban theo pattern có sẵn | `sonnet` (hoặc để `opusplan`) | Chỉ cần thực thi đúng chuẩn |
| Sửa bug khi đã rõ nguyên nhân | `sonnet` | |
| Test boilerplate, thao tác file hàng loạt | `haiku` | Việc cơ học |
| Debug khó, phát sinh quyết định kiến trúc | `opus` (`fable` nếu điều tra dài) | |
| Deploy/release production | `opus` | Sai sót có hậu quả thật |

### 0.3 Effort (`/effort <level>`)

- Luồng A: `high` hoặc `xhigh`.
- Luồng B: `xhigh`, nhất là B0 và B2.
- Implement 1 thẻ: `high`; việc lặp lại thuần túy thì hạ xuống `medium`.
- Giao nhiều thẻ liên tiếp: cân nhắc `ultracode`, nhưng chỉ khi các thẻ đã tách rõ và có acceptance criteria. Agent vẫn phải báo cáo và đạt DoD cho từng thẻ.
- Debug khó: `xhigh`, vẫn bí thì thử `max`. Haiku không hỗ trợ effort.

## Luồng A: dự án mới (checklist thứ tự)

- [ ] 0. Bước 0 ở trên: `mkdir -p docs/decision-logs`, chọn model/effort
- [ ] 1. [C1: copy template `docs/decision-logs/`](c1-decision-logs.md)
- [ ] 2. Điền [Input Requirement](a0-input-requirement.md), rồi chạy [A1: `docs/prd.md`](a1-prd.md)
- [ ] 3. [A2: `docs/domain-glossary.md`](a2-domain-glossary.md)
- [ ] 4. [A3: `docs/architecture.md`](a3-architecture.md)
- [ ] 5. [A4: `docs/ux.md`](a4-ux.md)
- [ ] 6. [A5: `docs/deploy.md`](a5-deploy.md)
- [ ] 7. [A6: `docs/conventions.md` + `docs/review-checklist.md`](a6-conventions.md)
- [ ] 8. [C2: `CLAUDE.md`](c2-claude-md.md): copy template, tự điền 4 chỗ trống, xóa mục 12
- [ ] 9. (tùy chọn) [A7: `docs/status.md` backlog ban đầu + bảng Kanban](a7-kanban-backlog.md)

## Luồng B: dự án đang vận hành (checklist thứ tự)

Đọc [nguyên tắc luồng B](b-principles.md) trước khi chạy. Làm toàn bộ trên một nhánh riêng (ví dụ `docs/bootstrap`) và chỉ merge sau khi người nắm hệ thống đã review.

- [ ] 0. Bước 0 ở trên, tạo nhánh `docs/bootstrap`
- [ ] 1. Điền [Input bối cảnh](b-input-context.md)
- [ ] 2. [C1: copy template `docs/decision-logs/`](c1-decision-logs.md)
- [ ] 3. [B0: quét codebase](b0-codebase-scan.md) → **DỪNG**, trả lời câu hỏi mở trong `docs/_discovery/12-open-questions.md` rồi commit
- [ ] 4. [B1: `docs/domain-glossary.md`](b1-domain-glossary.md) (glossary đi trước PRD)
- [ ] 5. [B2: `docs/prd.md`](b2-prd.md)
- [ ] 6. [B3: `docs/architecture.md`](b3-architecture.md)
- [ ] 7. [B4: `docs/ux.md`](b4-ux.md)
- [ ] 8. [B5: `docs/deploy.md`](b5-deploy.md)
- [ ] 9. [B6: `docs/conventions.md` + `docs/review-checklist.md`](b6-conventions.md)
- [ ] 10. [C2: `CLAUDE.md`](c2-claude-md.md), chọn loại "Dự án đang vận hành"
- [ ] 11. [B7: đóng quá trình tái dựng](b7-close.md)

Độ tin cậy của các file tái dựng, từ cao xuống thấp: deploy, architecture → conventions → glossary → ux → prd. Thời gian review nên tỉ lệ nghịch với độ tin cậy, tức là PRD cần review kỹ nhất.

## Tối ưu token và prompt caching (v2.1)

Chi phí lớn nhất không nằm ở prompt sinh tài liệu (chạy một lần) mà ở **việc agent đọc tài liệu trước mỗi task** trong suốt đời dự án. Bộ prompt được thiết kế để giảm chi phí đó:

- **Nạp sẵn qua CLAUDE.md:** `domain-glossary.md` và `review-checklist.md` được `@import`. Task nào cũng cần 2 file này, nên chúng nằm trong phần prefix (system prompt + CLAUDE.md) mà prompt cache dùng lại giữa các session. Nếu để agent Read ở đầu mỗi task thì session nào cũng phải ghi cache lại từ đầu. Vì vậy 2 file này bị giới hạn kích thước chặt (glossary dưới ~150 dòng, checklist tối đa 40 dòng).
- **Đọc theo phần:** mỗi FR/NFR là 1 dòng bảng bắt đầu bằng mã nên `grep "FR-03"` đủ thay cho việc đọc cả PRD; module trong architecture có tiêu đề cố định `### Module: <Tên>`; luồng trong ux có tiêu đề `### Luồng: <Tên>`.
- **Giới hạn độ dài** cho từng file; không chép nội dung giữa các file; quy tắc hành vi của agent chỉ nằm trong CLAUDE.md, không lặp lại trong docs/.
- **CLAUDE.md ổn định:** không chứa ngày tháng hay trạng thái task, để cache hit được giữa các session. Sửa các file được `@import` sẽ làm mất cache một lần ở session kế tiếp, chấp nhận được.
- **Template tĩnh** cho phần cố định (decision-logs, CLAUDE.md) thay vì để model chép lại.
- **Luồng B:** discovery tách mỗi mục 1 file, các bước sau chỉ đọc đúng file cần; B7 dùng grep thay vì đọc toàn bộ docs/.
- **Mỗi prompt một session mới:** chạy nối A1→A7 trong một session thì context phình ra, và lượt gọi tool nào cũng phải gửi lại toàn bộ lịch sử.

## Dự án đang dùng bộ v1.0

Chạy C1 → [prompt chuyển v1.0 sang v2.0](appendix-migrate-v1-to-v2.md) → C2.

## Thay đổi so với v1.0

| Hạng mục | v1.0 | v2.0 |
|---|---|---|
| Phạm vi | Chỉ dự án mới | Thêm luồng B cho dự án đang vận hành |
| Nhật ký quyết định | 1 file `docs/decisions.md` | Thư mục `docs/decision-logs/`, mỗi quyết định 1 file `YYYY-MM-DD-<slug>.md` |
| Theo dõi tiến độ | `docs/status.md` | Bỏ `status.md`, tiến độ nằm trên bảng Kanban; hoặc tạo lại `docs/status.md` bằng A7 theo dạng bảng Kanban Markdown (tùy chọn) |
| Prompt dùng chung | Không có | C1 (decision-logs) và C2 (CLAUDE.md) |
| Model | Ghi cứng tên phiên bản | Ghi theo alias |
| Tối ưu token (v2.1) | Không có | Template tĩnh cho C1/C2, `review-checklist.md` tách riêng, glossary và checklist nạp qua `@import`, đọc theo phần, giới hạn độ dài, discovery tách file |

Bỏ `status.md` kiểu v1.0 thì ba thứ phải có chỗ mới (nếu dùng `docs/status.md` của A7 thì chúng nằm ngay trong file đó):
- **Nguồn task** chuyển thành thẻ Kanban, và mỗi task agent làm phải gắn với một mã thẻ.
- **Deferred Work và Blocker** chuyển thành thẻ mới hoặc thẻ gắn cờ chặn. Agent phải liệt kê chúng ở cuối báo cáo của mỗi task.
- **Điểm tiếp tục giữa các session** dựa vào thẻ, nhánh git của thẻ và mô tả PR.

Điểm yếu cần biết: bảng Kanban nằm ngoài repo. Nếu Claude Code chưa kết nối MCP tới công cụ Kanban, anh phải dán nội dung thẻ vào prompt, còn agent chỉ liệt kê những thẻ cần tạo. Nên kết nối MCP nếu công cụ có hỗ trợ.
