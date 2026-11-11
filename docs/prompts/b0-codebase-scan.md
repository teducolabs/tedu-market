# B0: Quét codebase → `docs/_discovery/`

> Luồng B · Chạy sau: C1 · Input: [b-input-context.md](b-input-context.md) · Model: `opus` (repo lớn: `opus[1m]`/`fable`) · Effort: `xhigh` · Session mới
>
> Kết quả quét được ghi thành **mỗi mục 1 file** (`01-…` đến `12-…`) để B1–B7 chỉ đọc đúng mục mình cần. Nếu gộp thành 1 file lớn, bước nào cũng phải đọc cả file.
>
> **Sau bước này phải DỪNG.** Đọc bản quét, viết câu trả lời vào `docs/_discovery/12-open-questions.md` (thêm dòng "Trả lời: ..." dưới mỗi câu) rồi commit. Câu nào chưa trả lời được thì để nguyên, các file sau sẽ giữ nhãn `[CẦN XÁC NHẬN]` tương ứng.

```text
Bạn đang làm việc trên một hệ thống ĐANG CHẠY PRODUCTION. Bước này chỉ ĐỌC và GHI NHẬN: không sửa, format hay refactor file code/config nào; không chạy migration; không chạy lệnh ghi vào database hay gọi dịch vụ bên ngoài. Được phép: đọc file, liệt kê thư mục, grep/ripgrep, đọc lịch sử git (log, blame, show), build và chạy test local nếu không tác động tới dữ liệu thật. Không chắc lệnh nào an toàn thì hỏi tôi.

=== INPUT BỐI CẢNH ===
[dán nội dung đã điền ở mục Input bối cảnh vào đây]
=== HẾT INPUT ===

Đọc trước tài liệu có sẵn: README*, docs/, CLAUDE.md cũ, docs/_discovery/legacy-docs/ nếu có. Sau đó quét codebase theo phạm vi trong input. Repo lớn thì chia theo khu vực (mỗi thành phần triển khai độc lập hoặc mỗi module lớn) và giao cho subagent quét song song; subagent trả về bản tóm tắt có đường dẫn file, không trả nội dung file. Không đọc tuần tự cả repo trong 1 context.

Ghi kết quả vào thư mục tạm `docs/_discovery/` (sẽ xóa ở B7), mỗi mục 1 file theo đúng tên dưới đây. Mọi nhận định gắn nhãn [TỪ CODE] (kèm đường dẫn), [SUY LUẬN] hoặc [CẦN XÁC NHẬN]. Viết dạng bảng/gạch đầu dòng, mỗi mục 1 dòng, không văn xuôi.

- `01-repo-map.md` — cây thư mục cấp 2-3, mỗi thư mục chính 1 dòng vai trò; các thành phần triển khai độc lập và entry point.
- `02-tech-stack.md` — ngôn ngữ, framework, phiên bản (lấy từ file project/lock file, không đoán), thư viện chính theo nhóm (ORM, auth, logging, test, UI...). Ghi thư viện hết hỗ trợ và chỗ lệch phiên bản giữa các thành phần.
- `03-capabilities.md` — mọi hành vi nhìn thấy từ code: API endpoint (method + route + handler), route/màn hình frontend, background job/cron, consumer message queue, webhook, tích hợp bên thứ ba. Mỗi mục 1 dòng kèm đường dẫn. Đây là nguyên liệu cho FR nên phải đủ, không lấy mẫu. Tách tiêu đề `## Backend`, `## Frontend`, `## Job & tích hợp`.
- `04-data-model.md` — entity/bảng chính và quan hệ (từ ORM model/migration/schema). Ghi riêng bảng không còn code nào đọc/ghi (nghi là chết).
- `05-vocabulary.md` — tên class/entity/enum/bảng, key i18n, label UI lặp lại. Đánh dấu các nhóm nghi là cùng 1 khái niệm nhưng nhiều tên (vd `Student`, `Learner`, `Member`).
- `06-code-patterns.md` — cấu trúc lớp/thư mục, xử lý lỗi, validate, truy cập DB, naming. Mỗi mẫu hình có số liệu phổ biến và danh sách ngoại lệ (vd "18/22 controller qua MediatR, 4 gọi DbContext trực tiếp: <file>"). Không kết luận "convention là X" khi mới thấy 1-2 file.
- `07-tests.md` — framework, số lượng xấp xỉ theo tầng unit/integration/e2e, khu vực có/không có test, test bị skip. Chạy test local được an toàn thì chạy và ghi kết quả.
- `08-build-infra.md` — Dockerfile, docker-compose, pipeline CI/CD, script deploy, IaC, biến môi trường được đọc trong code. Chỉ ghi TÊN biến, không chép giá trị secret; thấy secret bị commit thì ghi vị trí vào `11-risks.md`.
- `09-decision-traces.md` — git log/blame trên các file then chốt (file project, DI/startup, pipeline, migration đầu của các bảng lớn) để tìm thay đổi lớn: đổi thư viện, database, cách auth, tách/gộp module. Mỗi dấu vết: commit hash, ngày, thay đổi gì, lý do nếu commit/PR/comment có nêu, không có thì "không rõ lý do". Không tự điền lý do.
- `10-anomalies.md` — tài liệu cũ nói A mà code làm B; 2 nơi xử lý cùng việc theo 2 cách; code chết; feature flag bỏ quên; TODO/FIXME/HACK quan trọng (tóm tắt).
- `11-risks.md` — bảo mật (secret trong repo, endpoint thiếu kiểm tra quyền, SQL ghép chuỗi...), vận hành (không health check, migration không đảo ngược được...), bảo trì (hotspot: file đổi nhiều theo git log mà không có test). Mỗi rủi ro 1 dòng kèm vị trí. Đây là danh sách để tạo thẻ, không phải việc để sửa ngay.
- `12-open-questions.md` — gom mọi [CẦN XÁC NHẬN], đánh số Q-01, Q-02...; mỗi câu ghi nên hỏi ai (nghiệp vụ hay hạ tầng) và ảnh hưởng tới file tài liệu nào.

Kết thúc: báo tôi số câu hỏi mở và 3-5 điểm bạn ít tự tin nhất. Dừng tại đây, không tự chạy tiếp bước tạo tài liệu.
```
