# Status — TEDU Market

Backlog ban đầu và bảng theo dõi tiến độ. Nguồn: [prd.md](prd.md), [architecture.md](architecture.md), [ux.md](ux.md), [deploy.md](deploy.md), [domain-glossary.md](domain-glossary.md). Ngày lập: 2026-11-11.

## Quy ước cập nhật

- Đổi trạng thái thẻ thì sửa **cả** checkbox ở phần Phase và checkbox acceptance criteria trong phần Chi tiết thẻ, trong cùng một lần sửa.
- Checkbox thẻ ở phần Phase: `[ ]` chưa xong · `[~]` đang ở Review (đạt Definition of Done, chờ người review) · `[x]` Xong (do người review quyết định).
- Acceptance criteria trong Chi tiết thẻ: tick `[x]` khi đã kiểm chứng; chưa kiểm chứng được thì để `[ ]` và ghi lý do ngay cuối dòng.
- Thẻ xong khi đạt Definition of Done của dự án và tất cả acceptance criteria quan sát được.
- Thẻ 🚀 cần môi trường staging thật, nên phụ thuộc T15 (Job `deploy-stg`).
- Thẻ chỉ bắt đầu khi mọi thẻ phụ thuộc đã Xong.
- Việc phát sinh trong lúc làm thì thêm thẻ mới ở cuối phase phù hợp, không nhét vào thẻ đang làm.

## Phase và checklist

### Phase 0 — Nền tảng code

_Sau T01, T02/T07 chạy song song; T03-T06 sau T02; T08-T10 sau T02+T07._

- [x] T01 Khởi tạo monorepo pnpm + Turborepo và solution .NET — epic: Nền tảng; phụ thuộc: —
- [x] T02 Khung backend: Minimal API `/api/v1`, Mediator, OpenAPI, ProblemDetails — epic: Nền tảng; phụ thuộc: T01
- [ ] T03 PostgreSQL 18, EF Core, Dapper, docker-compose dev và migration đầu — epic: Nền tảng; phụ thuộc: T02
- [ ] T04 Log có cấu trúc Serilog + Seq, CorrelationId, health live/ready — epic: Nền tảng; phụ thuộc: T02, T03
- [ ] T05 Khung test: xUnit, Testcontainers, phân loại Unit/Integration, coverage — epic: Nền tảng; phụ thuộc: T03
- [ ] T06 CORS, security headers, Output Cache và RateLimiter trong bộ nhớ — epic: Nền tảng; phụ thuộc: T02
- [ ] T07 Package `ui`: Tailwind v4, shadcn/ui, design tokens, component pattern — epic: Nền tảng; phụ thuộc: T01
- [ ] T08 API client dùng chung sinh bằng openapi-ts — epic: Nền tảng; phụ thuộc: T02
- [ ] T09 Khung Web học viên (Next.js 16): layout, header, responsive, SEO nền — epic: Nền tảng; phụ thuộc: T07, T08
- [ ] T10 Khung App quản trị (React Vite): sidebar theo vai trò, layout, responsive — epic: Nền tảng; phụ thuộc: T07, T08

### Phase 1 — Hạ tầng và CI/CD

_Chạy song song với Phase 2 sau T11. Mọi thẻ đánh dấu 🚀 (cần deploy thật) phụ thuộc T15._

- [ ] T11 CI cho pull request: unit, integration và ngưỡng coverage — epic: Hạ tầng/CI-CD; phụ thuộc: T05
- [ ] T12 Dockerfile api/web/admin/migrator và job build đẩy GHCR theo SHA — epic: Hạ tầng/CI-CD; phụ thuộc: T03, T09, T10, T11
- [ ] T13 Dựng VPS: Docker, user deploy, Nginx 3 subdomain, TLS, volume Video — epic: Hạ tầng/CI-CD; phụ thuộc: —
- [ ] T14 docker-compose.prod.yml, `.env.example`, secrets, project `tedu-stg` và `tedu-prod` — epic: Hạ tầng/CI-CD; phụ thuộc: T12, T13
- [ ] T15 Job `deploy-stg`, bundle migration và smoke test staging — epic: Hạ tầng/CI-CD; phụ thuộc: T14
- [ ] T16 Job `deploy-prod` có người duyệt và script rollback — epic: Hạ tầng/CI-CD; phụ thuộc: T15
- [ ] T17 Backup và restore: pg_dump hằng ngày, đồng bộ Video và MinIO ra ngoài VPS — epic: Hạ tầng/CI-CD; phụ thuộc: T14
- [ ] T18 Giám sát: probe ngoài VPS, alert, đĩa, TLS, backup — epic: Hạ tầng/CI-CD; phụ thuộc: T15, T17

### Phase 2 — Identity

_T19 trước; T20-T24 song song sau T19/T23; UI T25-T26 sau backend tương ứng._

- [ ] T19 Identity: OpenIddict + ASP.NET Core Identity, Code + PKCE, vai trò, policy — epic: Identity; phụ thuộc: T03, T06
- [ ] T20 Seeding Admin: mật khẩu ngẫu nhiên ghi `secret.json` — epic: Identity; phụ thuộc: T19
- [ ] T21 Đăng nhập Admin bằng email + mật khẩu, đổi mật khẩu, đăng xuất thu hồi token — epic: Identity; phụ thuộc: T19, T20
- [ ] T22 Google external login, tạo Học viên ở lần đăng nhập đầu — epic: Identity; phụ thuộc: T19
- [ ] T23 Outbox và BackgroundService: OutboxMessage, thăm dò ~2 giây, SKIP LOCKED — epic: Identity; phụ thuộc: T03
- [ ] T24 Notification: gửi email qua Resend từ Outbox, email chào mừng — epic: Identity; phụ thuộc: T22, T23
- [ ] T25 Web học viên: Đăng nhập bằng Google (OIDC server session) — epic: Identity; phụ thuộc: T09, T22
- [ ] T26 App quản trị: đăng nhập OIDC, sidebar theo vai trò, menu tài khoản, Đổi mật khẩu — epic: Identity; phụ thuộc: T10, T21
- [ ] T27 🚀 Xác nhận đăng nhập Google và Admin trên staging — epic: Identity; phụ thuộc: T15, T25, T26

### Phase 3 — Instructor

_T28 rồi T29; T30-T31 sau T26._

- [ ] T28 Duyệt Instructor: gán role và tạo Hồ sơ Instructor — epic: Instructor; phụ thuộc: T19
- [ ] T29 Hồ sơ Instructor: Admin sửa/xóa, Instructor sửa của mình — epic: Instructor; phụ thuộc: T28
- [ ] T30 App quản trị: Instructor (danh sách, Duyệt Instructor, sửa/xóa Hồ sơ Instructor) — epic: Instructor; phụ thuộc: T26, T28, T29
- [ ] T31 App quản trị: Hồ sơ Instructor của mình — epic: Instructor; phụ thuộc: T26, T29

### Phase 4 — Course và catalog

_Backend T32-T41 theo chuỗi phụ thuộc; UI T42-T48 song song sau backend tương ứng._

- [ ] T32 Category: CRUD — epic: Course; phụ thuộc: T19
- [ ] T33 Khóa học: tạo/sửa ở trạng thái Chưa đăng, kiểm tra sở hữu — epic: Course; phụ thuộc: T28, T32
- [ ] T34 Bài học: thêm, sửa, xóa, sắp xếp lại — epic: Course; phụ thuộc: T33
- [ ] T35 Thumbnail khóa học: MinIO, presigned URL — epic: Course; phụ thuộc: T03, T33
- [ ] T36 Gửi duyệt và Gửi lại: Chưa đăng → Chờ duyệt (khóa học) — epic: Course; phụ thuộc: T34
- [ ] T37 Duyệt khóa học: Duyệt, Từ chối kèm Lý do từ chối, Admin đăng không cần duyệt — epic: Course; phụ thuộc: T36
- [ ] T38 LastApprovedVersion (snapshot JSONB) và sửa khóa Đã đăng phải duyệt lại — epic: Course; phụ thuộc: T37
- [ ] T39 Bài học xem thử: đánh dấu và bỏ đánh dấu — epic: Course; phụ thuộc: T34
- [ ] T40 Truy vấn công khai: danh sách, lọc Category, sắp xếp, chi tiết (Dapper) — epic: Course; phụ thuộc: T06, T38
- [ ] T41 Tìm kiếm full-text PostgreSQL, gõ không dấu vẫn khớp — epic: Course; phụ thuộc: T40
- [ ] T42 App quản trị: Category (CRUD) — epic: Course; phụ thuộc: T26, T32
- [ ] T43 App quản trị: Khóa học (danh sách, form tạo/sửa, thumbnail, modal sửa khóa Đã đăng) — epic: Course; phụ thuộc: T26, T33, T35, T38
- [ ] T44 App quản trị: Bài học (thêm/sửa/xóa, kéo thả, Xem thử) — epic: Course; phụ thuộc: T34, T39, T43
- [ ] T45 App quản trị: Gửi duyệt, badge trạng thái, banner Lý do từ chối, Gửi lại — epic: Course; phụ thuộc: T36, T37, T44
- [ ] T46 App quản trị: Duyệt khóa học (hàng đợi, Duyệt/Từ chối) — epic: Course; phụ thuộc: T26, T37
- [ ] T47 Web học viên: Danh sách khóa học (lọc, sắp xếp, tìm kiếm) — epic: Course; phụ thuộc: T08, T09, T41
- [ ] T48 Web học viên: Chi tiết khóa học (SEO, nhãn Xem thử) — epic: Course; phụ thuộc: T09, T40

### Phase 5 — Video: tải lên và xử lý

_Phát video nằm ở Phase 8 vì cần Enrollment._

- [ ] T49 Tải lên Video: chỉ mp4 ≤ 1 GB, kiểm tra magic bytes, trạng thái xử lý — epic: Video; phụ thuộc: T23, T34
- [ ] T50 Worker ffmpeg: HLS AES-128, giới hạn song song, Sẵn sàng/Lỗi — epic: Video; phụ thuộc: T49
- [ ] T51 App quản trị: Upload video (tiến độ %, hủy, kiểm tra khi chọn, badge trạng thái) — epic: Video; phụ thuộc: T44, T49

### Phase 6 — Cart, Coupon và Enrollment nền

- [ ] T52 Enrollment: thực thể, unique (Học viên, Khóa học), truy vấn sở hữu và Khóa học của tôi — epic: Enrollment; phụ thuộc: T03, T33
- [ ] T53 Coupon: CRUD cho Admin và tính giá — epic: Cart/Coupon; phụ thuộc: T19, T33
- [ ] T54 Giỏ hàng: thêm, xóa khóa, áp Coupon, chặn khóa đã mua — epic: Cart/Coupon; phụ thuộc: T52, T53
- [ ] T55 App quản trị: Coupon (CRUD) — epic: Cart/Coupon; phụ thuộc: T26, T53
- [ ] T56 Web học viên: Giỏ hàng, Coupon, Thêm vào giỏ, Vào học — epic: Cart/Coupon; phụ thuộc: T25, T48, T54

### Phase 7 — Order và Payment

_T57 → T58/T61/T65 → T59 → T60/T62/T63 → T64 → T66; UI T67-T69 sau backend._

- [ ] T57 Checkout idempotent → Đơn Mới (hạn 15 phút), chặn khóa đã mua, rate limit — epic: Order; phụ thuộc: T06, T54
- [ ] T58 Mã QR VietQR, Nội dung chuyển khoản duy nhất, truy vấn trạng thái Đơn — epic: Payment; phụ thuộc: T57
- [ ] T59 Hook thanh toán: X-API-Token, Đã thanh toán đúng 1 lần, ghi Payment và Outbox cùng transaction — epic: Payment; phụ thuộc: T23, T58
- [ ] T60 Hook: tiền thừa/thiếu → Chênh lệch tiền, Đơn sang Đang chờ — epic: Payment; phụ thuộc: T59
- [ ] T61 Job Hết hạn sau 15 phút và tự Đã hủy sau 1 giờ — epic: Order; phụ thuộc: T23, T57
- [ ] T62 Hook cho Đơn Hết hạn/Đã hủy: ghi nhận, sang Đang chờ, trả 200 — epic: Payment; phụ thuộc: T59, T61
- [ ] T63 Enrollment từ Outbox (idempotent) và email hoàn thành đơn — epic: Enrollment; phụ thuộc: T24, T52, T59
- [ ] T64 Xử lý đơn (OrderResolution): Admin chuyển sang Đã thanh toán/Đã hủy, thu hồi Enrollment — epic: Order; phụ thuộc: T59, T63
- [ ] T65 Lịch sử đơn và danh sách Đơn cho Admin (backend) — epic: Order; phụ thuộc: T57
- [ ] T66 Integration test race: Đã thanh toán trùng/đồng thời, Enrollment không trùng, crash giữa chừng — epic: Order; phụ thuộc: T59, T60, T62, T63
- [ ] T67 Web học viên: Checkout và Thanh toán (Mã QR, đếm ngược, polling, các trạng thái) — epic: Payment; phụ thuộc: T25, T56, T58
- [ ] T68 Web học viên: Khóa học của tôi và Lịch sử đơn — epic: Enrollment; phụ thuộc: T25, T52, T65
- [ ] T69 App quản trị: Đơn (danh sách, chi tiết, Xử lý đơn) — epic: Order; phụ thuộc: T26, T64, T65

### Phase 8 — Học và phát video

- [ ] T70 Phát video: token phát, manifest/segment/key qua API, Nginx X-Accel-Redirect — epic: Learning; phụ thuộc: T13, T38, T50, T52
- [ ] T71 Báo lỗi video: Học viên gửi, Admin nhận (backend) — epic: Learning; phụ thuộc: T50, T52
- [ ] T72 Web học viên: Trang học (player HLS, danh sách Bài học, trạng thái, Gửi báo lỗi) — epic: Learning; phụ thuộc: T68, T70, T71
- [ ] T73 App quản trị: Báo lỗi video — epic: Learning; phụ thuộc: T26, T71

### Phase 9 — Doanh thu Instructor

- [ ] T74 Tỷ lệ chia chung (cấu hình) và Tỷ lệ chia riêng (đặt/xóa) — epic: Revenue; phụ thuộc: T28
- [ ] T75 Khoản chia: sao chép tỷ lệ vào Đơn và ghi Khoản chia chờ duyệt khi Đã thanh toán — epic: Revenue; phụ thuộc: T59, T64, T74
- [ ] T76 Duyệt Khoản chia và tính Số dư — epic: Revenue; phụ thuộc: T75
- [ ] T77 Phiếu chi: Admin chọn số tiền, không vượt Số dư — epic: Revenue; phụ thuộc: T76
- [ ] T78 Doanh thu: truy vấn dashboard theo Instructor và khoảng thời gian (backend) — epic: Revenue; phụ thuộc: T75, T76
- [ ] T79 App quản trị: Doanh thu — epic: Revenue; phụ thuộc: T26, T78
- [ ] T80 App quản trị: Tỷ lệ chia riêng trên màn hình Instructor — epic: Revenue; phụ thuộc: T30, T74
- [ ] T81 App quản trị: Khoản chia chờ duyệt — epic: Revenue; phụ thuộc: T26, T76
- [ ] T82 App quản trị: Phiếu chi (Tạo phiếu chi) — epic: Revenue; phụ thuộc: T26, T77

### Phase 10 — Chất lượng và go-live (🚀 cần staging)

_Các thẻ này chạy sau khi tính năng liên quan đã lên staging._

- [ ] T83 Bộ test phân quyền API: vai trò, IDOR, URL file video — epic: Chất lượng & vận hành; phụ thuộc: T64, T70, T77
- [ ] T84 🚀 Bộ E2E và job `e2e` trong CI — epic: Chất lượng & vận hành; phụ thuộc: T11, T63, T67, T72
- [ ] T85 🚀 Lighthouse SEO và LCP trên staging — epic: Chất lượng & vận hành; phụ thuộc: T15, T47, T48
- [ ] T86 🚀 Load test: danh sách/chi tiết, 500 người dùng đồng thời, video — epic: Chất lượng & vận hành; phụ thuộc: T15, T40, T70
- [ ] T87 🚀 Đo Đã thanh toán → Enrollment và cảnh báo Outbox tồn đọng — epic: Chất lượng & vận hành; phụ thuộc: T15, T63
- [ ] T88 🚀 Diễn tập khôi phục khi mất VPS — epic: Chất lượng & vận hành; phụ thuộc: T16, T17
- [ ] T89 🚀 Thử nghiệm với ≥ 3 Instructor thật — epic: Chất lượng & vận hành; phụ thuộc: T15, T31, T45, T51
- [ ] T90 🚀 Kiểm tra hạn mức Resend gói free và cảnh báo — epic: Chất lượng & vận hành; phụ thuộc: T15, T24
- [ ] T91 🚀 Go-live production lần đầu — epic: Chất lượng & vận hành; phụ thuộc: T16, T18, T85, T88

## Chi tiết thẻ

### T01 Khởi tạo monorepo pnpm + Turborepo và solution .NET

- **Epic:** Nền tảng · **FR/NFR:** NFR-13, NFR-17 · **Phụ thuộc:** —
- **Mô tả:** Tạo monorepo có `apps/student-web`, `apps/admin-app`, `packages/ui`, `packages/api-client` và solution .NET 10 với 4 project Domain, Application, Infrastructure, Api. Thêm `.gitignore` chặn `.env*`, `secret.json`.
- **Acceptance criteria:**
  - [x] `pnpm install` và `dotnet build` chạy sạch trên máy mới (đã kiểm trên máy dev; chưa thử trên máy hoàn toàn mới)
  - [x] `pnpm turbo build` chạy được trên các package rỗng; `.env*` và `secret.json` không bị git theo dõi

### T02 Khung backend: Minimal API `/api/v1`, Mediator, OpenAPI, ProblemDetails

- **Epic:** Nền tảng · **FR/NFR:** FR-26, NFR-12, NFR-13, NFR-17 · **Phụ thuộc:** T01
- **Mô tả:** Dựng pipeline Mediator (martinothamar), phiên bản API theo URL, xuất OpenAPI schema, middleware lỗi ProblemDetails và validation trả 400 theo từng trường.
- **Acceptance criteria:**
  - [x] Gọi endpoint mẫu với dữ liệu sai trả 400 có lỗi theo từng trường
  - [x] Lỗi bất kỳ khác trả ProblemDetails, không lộ stack trace; `/openapi/v1.json` truy cập được

### T03 PostgreSQL 18, EF Core, Dapper, docker-compose dev và migration đầu

- **Epic:** Nền tảng · **FR/NFR:** NFR-12, NFR-17 · **Phụ thuộc:** T02
- **Mô tả:** Thêm `docker-compose.dev.yml` (PostgreSQL 18, MinIO, Seq), DbContext EF Core cho phía ghi, kết nối Dapper cho phía đọc, migration đầu tiên.
- **Acceptance criteria:**
  - `docker compose -f docker-compose.dev.yml up` rồi `dotnet ef database update` tạo được schema
  - Một query mẫu qua Dapper và một lệnh ghi qua EF Core chạy được

### T04 Log có cấu trúc Serilog + Seq, CorrelationId, health live/ready

- **Epic:** Nền tảng · **FR/NFR:** NFR-14, NFR-15 · **Phụ thuộc:** T02, T03
- **Mô tả:** Ghi log có cấu trúc, gắn CorrelationId vào mọi request và vào ProblemDetails, thêm `/health/live` và `/health/ready` (kiểm tra PostgreSQL, MinIO).
- **Acceptance criteria:**
  - Tìm trong Seq theo một CorrelationId thấy toàn bộ log của request đó
  - `/health/ready` trả 200 khi PostgreSQL và MinIO sống, trả lỗi khi một trong hai tắt; log không chứa token hay mật khẩu

### T05 Khung test: xUnit, Testcontainers, phân loại Unit/Integration, coverage

- **Epic:** Nền tảng · **FR/NFR:** NFR-16 · **Phụ thuộc:** T03
- **Mô tả:** Tạo project test, phân loại `Category=Unit` và `Category=Integration`, Testcontainers PostgreSQL thật, thu thập coverage.
- **Acceptance criteria:**
  - `dotnet test --filter Category=Unit` và `--filter Category=Integration` chạy độc lập
  - Integration test tự dựng PostgreSQL bằng container và dọn sau khi chạy

### T06 CORS, security headers, Output Cache và RateLimiter trong bộ nhớ

- **Epic:** Nền tảng · **FR/NFR:** NFR-01, NFR-09, NFR-10 · **Phụ thuộc:** T02
- **Mô tả:** Cấu hình CORS chỉ cho origin của App quản trị và Web học viên, security headers, Output Cache (có tag để xóa cache) và policy RateLimiter 10 request/phút/người dùng.
- **Acceptance criteria:**
  - Request từ origin lạ bị chặn CORS; response có security headers
  - Endpoint gắn policy rate limit trả 429 ở request thứ 11 trong 1 phút

### T07 Package `ui`: Tailwind v4, shadcn/ui, design tokens, component pattern

- **Epic:** Nền tảng · **FR/NFR:** NFR-23 · **Phụ thuộc:** T01
- **Mô tả:** Dựng `packages/ui` theo ux.md mục 4-6: tokens màu/chữ/spacing, Form, Table, Modal, Toast, Empty state, Skeleton, Badge trạng thái, Banner, tiêu chuẩn accessibility.
- **Acceptance criteria:**
  - Trang mẫu hiển thị đủ các component ở trạng thái Loading/Empty/Error/Success
  - Badge mọi trạng thái có chữ đúng glossary; focus ring 2px `primary`, contrast chữ ≥ 4.5:1

### T08 API client dùng chung sinh bằng openapi-ts

- **Epic:** Nền tảng · **FR/NFR:** NFR-13 · **Phụ thuộc:** T02
- **Mô tả:** Sinh `packages/api-client` từ OpenAPI schema bằng `@hey-api/openapi-ts`, chạy qua Turborepo.
- **Acceptance criteria:**
  - Đổi một endpoint ở backend rồi chạy generate thì client trong cả hai app đổi theo
  - Hai app import được cùng một client

### T09 Khung Web học viên (Next.js 16): layout, header, responsive, SEO nền

- **Epic:** Nền tảng · **FR/NFR:** NFR-03, NFR-11, NFR-17 · **Phụ thuộc:** T07, T08
- **Mô tả:** Next.js 16 App Router với Header (logo, ô tìm kiếm, Giỏ hàng, Khóa học của tôi, tài khoản), font Inter, `next/image`, layout mobile-first theo ux.md mục 8.
- **Acceptance criteria:**
  - Header hiển thị đúng ở 4 breakpoint `sm/md/lg/xl`
  - Trang mẫu render phía server và có thẻ metadata

### T10 Khung App quản trị (React Vite): sidebar theo vai trò, layout, responsive

- **Epic:** Nền tảng · **FR/NFR:** NFR-17, NFR-23 · **Phụ thuộc:** T07, T08
- **Mô tả:** Dựng SPA React Vite với sidebar theo ux.md mục 2 (ẩn/hiện theo vai trò), layout desktop-first; dưới 640px hiện thông báo "Vui lòng dùng máy tính hoặc máy tính bảng để quản trị".
- **Acceptance criteria:**
  - Sidebar hiển thị đúng mục theo vai trò Admin hoặc Instructor (dữ liệu giả)
  - Dưới 640px hiện thông báo kèm nút Đăng xuất

### T11 CI cho pull request: unit, integration và ngưỡng coverage

- **Epic:** Hạ tầng/CI-CD · **FR/NFR:** NFR-16 · **Phụ thuộc:** T05
- **Mô tả:** GitHub Actions chạy `unit` rồi `integration` (needs), coverage Domain + Application ≥ 70% mới pass, không `continue-on-error`.
- **Acceptance criteria:**
  - PR có unit test lỗi thì job `integration` bị skipped
  - Coverage dưới 70% làm job `unit` fail

### T12 Dockerfile api/web/admin/migrator và job build đẩy GHCR theo SHA

- **Epic:** Hạ tầng/CI-CD · **FR/NFR:** NFR-16 · **Phụ thuộc:** T03, T09, T10, T11
- **Mô tả:** Viết Dockerfile cho `api` (có ffmpeg), `web` (Next.js standalone), `admin`, `migrator` (EF bundle); job `build` push GHCR tag = commit SHA, không dùng `latest`.
- **Acceptance criteria:**
  - Sau khi merge `master`, 4 image có tag SHA trên GHCR
  - `docker run` image `api` thấy được `ffmpeg -version`

### T13 Dựng VPS: Docker, user deploy, Nginx 3 subdomain, TLS, volume Video

- **Epic:** Hạ tầng/CI-CD · **FR/NFR:** NFR-09, NFR-14 · **Phụ thuộc:** —
- **Mô tả:** Cài Docker, tạo user `deploy` không sudo, thư mục `/srv/tedu-market`, Nginx cho www/admin/api và bản `-stg`, chứng chỉ TLS, chuyển HTTP sang HTTPS, location `internal` cho thư mục Video (X-Accel-Redirect), giới hạn upload 1 GB, firewall.
- **Acceptance criteria:**
  - Cả 6 domain trả HTTPS hợp lệ, HTTP chuyển hướng sang HTTPS
  - Truy cập trực tiếp đường dẫn thư mục Video từ ngoài trả 404/403

### T14 docker-compose.prod.yml, `.env.example`, secrets, project `tedu-stg` và `tedu-prod`

- **Epic:** Hạ tầng/CI-CD · **FR/NFR:** NFR-14, NFR-22 · **Phụ thuộc:** T12, T13
- **Mô tả:** Viết compose chung cho staging/production, healthcheck mỗi service, volume riêng, Seq/MinIO console chỉ bind 127.0.0.1, `.env.example` chỉ chứa tên biến, `.env.stg` đặt trên VPS (`chmod 600`).
- **Acceptance criteria:**
  - `docker compose -p tedu-stg ... up -d --wait` lên xanh với image SHA
  - Staging và production dùng DB, volume, secret tách biệt; repo không chứa giá trị secret

### T15 Job `deploy-stg`, bundle migration và smoke test staging

- **Epic:** Hạ tầng/CI-CD · **FR/NFR:** NFR-09, NFR-14, NFR-16 · **Phụ thuộc:** T14
- **Mô tả:** Job tự chạy trên `master` sau CI xanh: pre-check, chạy migrator, pull, `up -d --wait`, smoke test (health, danh sách khóa, HTTPS, hook sai token, manifest không token) theo deploy.md mục 6.
- **Acceptance criteria:**
  - Merge `master` thì staging chạy đúng SHA mới và smoke test pass
  - Smoke test fail thì pipeline đỏ và kích hoạt bước rollback

### T16 Job `deploy-prod` có người duyệt và script rollback

- **Epic:** Hạ tầng/CI-CD · **FR/NFR:** NFR-14 · **Phụ thuộc:** T15
- **Mô tả:** `workflow_dispatch` với Environment `production` bắt buộc reviewer; lưu `.tag-prod.prev`; script rollback quay image trong ≤ 15 phút theo deploy.md mục 7.
- **Acceptance criteria:**
  - Job không tự chạy, chỉ chạy sau khi reviewer duyệt
  - Chạy thử rollback trên staging quay về SHA trước và smoke test pass

### T17 Backup và restore: pg_dump hằng ngày, đồng bộ Video và MinIO ra ngoài VPS

- **Epic:** Hạ tầng/CI-CD · **FR/NFR:** NFR-19 · **Phụ thuộc:** T14
- **Mô tả:** Viết `scripts/backup.sh` và `scripts/restore.sh` (kiểm tra `pg_restore --list`), lịch chạy hằng ngày, đồng bộ thư mục Video và MinIO tới `BACKUP_REMOTE`.
- **Acceptance criteria:**
  - Backup hằng ngày chạy xong và có bản trên nơi lưu ngoài VPS
  - Restore vào môi trường staging khôi phục đúng dữ liệu mẫu

### T18 Giám sát: probe ngoài VPS, alert, đĩa, TLS, backup

- **Epic:** Hạ tầng/CI-CD · **FR/NFR:** NFR-14, NFR-15, NFR-19 · **Phụ thuộc:** T15, T17
- **Mô tả:** Probe `/health/live` và `/health/ready` mỗi phút từ ngoài VPS, alert khi fail 3 lần, cảnh báo đĩa/CPU/RAM > 80%, TLS còn < 14 ngày, thiếu backup. Kênh alert chưa chốt: hỏi chủ dự án trước khi cấu hình.
- **Acceptance criteria:**
  - Tắt container `api` thì alert gửi đi sau 3 lần fail liên tiếp
  - Thiếu backup trong ngày phát cảnh báo mức production

### T19 Identity: OpenIddict + ASP.NET Core Identity, Code + PKCE, vai trò, policy

- **Epic:** Identity · **FR/NFR:** FR-04, FR-05, NFR-06, NFR-08 · **Phụ thuộc:** T03, T06
- **Mô tả:** Dựng Authorization Server chạy chung trong API, vai trò Student/Instructor/Admin, policy theo role ở endpoint và mẫu kiểm tra sở hữu trong Handler.
- **Acceptance criteria:**
  - Client mẫu đăng nhập được bằng Authorization Code + PKCE và nhận token
  - Gọi endpoint sai vai trò bị từ chối (401/403)

### T20 Seeding Admin: mật khẩu ngẫu nhiên ghi `secret.json`

- **Epic:** Identity · **FR/NFR:** FR-41, NFR-22 · **Phụ thuộc:** T19
- **Mô tả:** Lần chạy đầu tạo Admin `support@tedu.com.vn` với mật khẩu ngẫu nhiên, ghi vào `secret.json` ngoài source control, mật khẩu lưu dạng băm.
- **Acceptance criteria:**
  - Chạy lần đầu sinh `secret.json` và đăng nhập được bằng mật khẩu đó
  - Chạy lại không tạo thêm Admin; `secret.json` không nằm trong git

### T21 Đăng nhập Admin bằng email + mật khẩu, đổi mật khẩu, đăng xuất thu hồi token

- **Epic:** Identity · **FR/NFR:** FR-04, FR-42 · **Phụ thuộc:** T19, T20
- **Mô tả:** Trang đăng nhập email + mật khẩu cho Admin, endpoint đổi mật khẩu, đăng xuất thu hồi token.
- **Acceptance criteria:**
  - Admin đổi mật khẩu rồi lần đăng nhập sau chỉ mật khẩu mới có hiệu lực
  - Sau đăng xuất, token cũ gọi API bị từ chối

### T22 Google external login, tạo Học viên ở lần đăng nhập đầu

- **Epic:** Identity · **FR/NFR:** FR-03 · **Phụ thuộc:** T19
- **Mô tả:** Đăng nhập Google qua OpenIddict; lần đầu tạo tài khoản Học viên, các lần sau dùng lại; người chưa đăng nhập gọi API cần đăng nhập thì bị từ chối.
- **Acceptance criteria:**
  - Đăng nhập Google lần đầu tạo đúng 1 tài khoản Học viên; lần hai không tạo thêm
  - Người dùng hủy ở Google quay về với lỗi rõ ràng, không tạo tài khoản

### T23 Outbox và BackgroundService: OutboxMessage, thăm dò ~2 giây, SKIP LOCKED

- **Epic:** Identity · **FR/NFR:** NFR-02, NFR-05 · **Phụ thuộc:** T03
- **Mô tả:** Dựng bảng `OutboxMessage`, BackgroundService xử lý theo lô bằng `FOR UPDATE SKIP LOCKED`, retry, đánh dấu đã xử lý.
- **Acceptance criteria:**
  - Message ghi trong transaction chỉ xuất hiện sau khi commit
  - Hai instance xử lý song song không xử lý trùng một message

### T24 Notification: gửi email qua Resend từ Outbox, email chào mừng

- **Epic:** Identity · **FR/NFR:** FR-27, NFR-18 · **Phụ thuộc:** T22, T23
- **Mô tả:** Module Notification gửi email Resend từ Outbox có retry; Học viên đăng nhập Google lần đầu nhận email chào mừng có liên kết tới `/courses`, không có bước xác thực email riêng.
- **Acceptance criteria:**
  - Đăng nhập Google lần đầu sinh đúng 1 email chào mừng; lần sau không gửi
  - Resend lỗi thì message ở lại Outbox và gửi lại, không chặn đăng nhập

### T25 Web học viên: Đăng nhập bằng Google (OIDC server session)

- **Epic:** Identity · **FR/NFR:** FR-03, NFR-06 · **Phụ thuộc:** T09, T22
- **Mô tả:** Next.js làm OIDC client phía server, token trong cookie session httpOnly mã hóa (không localStorage), nút Đăng nhập bằng Google, quay lại đúng trang trước đó, chặn trang cần đăng nhập.
- **Acceptance criteria:**
  - Vào Giỏ hàng khi chưa đăng nhập chuyển sang Google rồi quay lại Giỏ hàng
  - Hủy ở Google: về trang cũ và hiện banner "Đăng nhập chưa hoàn tất. Bạn thử lại nhé"

### T26 App quản trị: đăng nhập OIDC, sidebar theo vai trò, menu tài khoản, Đổi mật khẩu

- **Epic:** Identity · **FR/NFR:** FR-04, FR-05, FR-42, NFR-06 · **Phụ thuộc:** T10, T21
- **Mô tả:** `react-oidc-context` Code + PKCE, token giữ trong bộ nhớ, sidebar theo vai trò, menu Đăng xuất, Đổi mật khẩu chỉ hiện cho Admin.
- **Acceptance criteria:**
  - Instructor không thấy mục chỉ của Admin; Admin đổi được mật khẩu trong UI
  - Đăng xuất xong quay lại đăng nhập và token bị thu hồi

### T27 🚀 Xác nhận đăng nhập Google và Admin trên staging

- **Epic:** Identity · **FR/NFR:** FR-03, NFR-06, NFR-09 · **Phụ thuộc:** T15, T25, T26
- **Mô tả:** Đăng ký Google redirect URI cho domain staging, nạp secret, kiểm tra thực tế hai luồng đăng nhập qua HTTPS.
- **Acceptance criteria:**
  - Đăng nhập Google thật trên `www-stg` thành công
  - Admin seed đăng nhập vào `admin-stg` bằng mật khẩu từ `secret.json` trên VPS

### T28 Duyệt Instructor: gán role và tạo Hồ sơ Instructor

- **Epic:** Instructor · **FR/NFR:** FR-40 · **Phụ thuộc:** T19
- **Mô tả:** Admin duyệt một người dùng đã đăng ký thành Instructor: gán role, tạo `InstructorProfile`; danh sách người dùng/Instructor cho Admin.
- **Acceptance criteria:**
  - Sau khi duyệt, người đó đăng nhập App quản trị với quyền Instructor
  - Người không phải Admin gọi API duyệt bị từ chối

### T29 Hồ sơ Instructor: Admin sửa/xóa, Instructor sửa của mình

- **Epic:** Instructor · **FR/NFR:** FR-18, FR-43 · **Phụ thuộc:** T28
- **Mô tả:** API sửa/xóa Hồ sơ Instructor; Instructor chỉ sửa hồ sơ của chính mình.
- **Acceptance criteria:**
  - Instructor sửa hồ sơ người khác bị từ chối
  - Hồ sơ sửa xong hiển thị đúng trên khóa học liên quan

### T30 App quản trị: Instructor (danh sách, Duyệt Instructor, sửa/xóa Hồ sơ Instructor)

- **Epic:** Instructor · **FR/NFR:** FR-18, FR-40 · **Phụ thuộc:** T26, T28, T29
- **Mô tả:** Màn hình Instructor cho Admin theo ux.md mục 2 với bảng, lọc và modal xác nhận.
- **Acceptance criteria:**
  - Admin duyệt Instructor trong UI rồi thấy Hồ sơ Instructor mới
  - Đủ trạng thái Loading/Empty/Error/Success

### T31 App quản trị: Hồ sơ Instructor của mình

- **Epic:** Instructor · **FR/NFR:** FR-43, NFR-23 · **Phụ thuộc:** T26, T29
- **Mô tả:** Instructor tự sửa hồ sơ bằng form, lỗi theo từng trường.
- **Acceptance criteria:**
  - Instructor lưu hồ sơ thấy toast "Đã lưu"
  - Lỗi theo trường hiện đúng dưới ô nhập

### T32 Category: CRUD

- **Epic:** Course · **FR/NFR:** FR-17 · **Phụ thuộc:** T19
- **Mô tả:** API tạo/sửa/xóa Category chỉ cho Admin.
- **Acceptance criteria:**
  - Admin tạo, sửa, xóa được; Instructor/Học viên bị từ chối
  - Danh sách Category công khai phản ánh thay đổi

### T33 Khóa học: tạo/sửa ở trạng thái Chưa đăng, kiểm tra sở hữu

- **Epic:** Course · **FR/NFR:** FR-19, FR-22, NFR-08 · **Phụ thuộc:** T28, T32
- **Mô tả:** Instructor/Admin tạo và sửa Khóa học (thông tin, giá, Category); Instructor chỉ sửa khóa của mình, Admin sửa mọi khóa.
- **Acceptance criteria:**
  - Khóa mới ở trạng thái Chưa đăng
  - Instructor sửa khóa của người khác bị từ chối kể cả khi gọi thẳng API

### T34 Bài học: thêm, sửa, xóa, sắp xếp lại

- **Epic:** Course · **FR/NFR:** FR-20 · **Phụ thuộc:** T33
- **Mô tả:** API quản lý Bài học trong Khóa học với thứ tự; quyền sở hữu như T33.
- **Acceptance criteria:**
  - Sắp xếp lại thì thứ tự lưu và trả đúng
  - Bài học của khóa người khác không sửa được

### T35 Thumbnail khóa học: MinIO, presigned URL

- **Epic:** Course · **FR/NFR:** FR-23 · **Phụ thuộc:** T03, T33
- **Mô tả:** Cấp presigned PUT giới hạn kiểu và dung lượng; lưu khóa thumbnail vào Khóa học.
- **Acceptance criteria:**
  - Tải ảnh trực tiếp lên MinIO bằng URL cấp và thumbnail hiển thị trên khóa
  - File sai kiểu hoặc quá dung lượng bị từ chối

### T36 Gửi duyệt và Gửi lại: Chưa đăng → Chờ duyệt (khóa học)

- **Epic:** Course · **FR/NFR:** FR-21, FR-51 · **Phụ thuộc:** T34
- **Mô tả:** Gửi duyệt khóa có ≥ 1 Bài học thì sang Chờ duyệt (khóa học); khóa Bị từ chối bấm Gửi lại thì về Chờ duyệt (khóa học).
- **Acceptance criteria:**
  - Gửi duyệt khóa chưa có Bài học bị từ chối kèm lý do
  - Gửi lại khóa Bị từ chối đưa khóa về Chờ duyệt (khóa học)

### T37 Duyệt khóa học: Duyệt, Từ chối kèm Lý do từ chối, Admin đăng không cần duyệt

- **Epic:** Course · **FR/NFR:** FR-36, FR-49 · **Phụ thuộc:** T36
- **Mô tả:** Admin Duyệt thì khóa thành Đã đăng; Từ chối bắt buộc có Lý do từ chối; Admin tự đăng thì bỏ qua bước duyệt.
- **Acceptance criteria:**
  - Từ chối không có lý do trả 400 theo trường
  - Khóa chưa Duyệt không xuất hiện ở danh sách công khai

### T38 LastApprovedVersion (snapshot JSONB) và sửa khóa Đã đăng phải duyệt lại

- **Epic:** Course · **FR/NFR:** FR-22, FR-49 · **Phụ thuộc:** T37
- **Mô tả:** Lưu snapshot bản đã duyệt; Instructor sửa khóa Đã đăng thì khóa về Chờ duyệt (khóa học); Admin sửa không cần duyệt; xóa cache công khai khi duyệt.
- **Acceptance criteria:**
  - Instructor sửa khóa Đã đăng làm khách không còn thấy khóa đó
  - Bản công khai và Học viên đã mua luôn đọc từ LastApprovedVersion

### T39 Bài học xem thử: đánh dấu và bỏ đánh dấu

- **Epic:** Course · **FR/NFR:** FR-45 · **Phụ thuộc:** T34
- **Mô tả:** Admin hoặc Instructor sở hữu khóa đánh dấu Bài học xem thử.
- **Acceptance criteria:**
  - Bài học đã đánh dấu trả cờ xem thử trong chi tiết khóa
  - Người không sở hữu khóa đánh dấu bị từ chối

### T40 Truy vấn công khai: danh sách, lọc Category, sắp xếp, chi tiết (Dapper)

- **Epic:** Course · **FR/NFR:** FR-01, FR-02, NFR-01 · **Phụ thuộc:** T06, T38
- **Mô tả:** Một câu SQL Dapper cho danh sách phân trang và chi tiết (kèm Bài học, Instructor), chỉ khóa Đã đăng, index, Output Cache 30-60 giây.
- **Acceptance criteria:**
  - `/api/v1/courses` chỉ trả khóa Đã đăng, phân trang, lọc, sắp xếp theo giá/ngày
  - Khóa chờ duyệt lại hoặc bị từ chối không xuất hiện

### T41 Tìm kiếm full-text PostgreSQL, gõ không dấu vẫn khớp

- **Epic:** Course · **FR/NFR:** FR-44, NFR-01 · **Phụ thuộc:** T40
- **Mô tả:** Cột search vector (GIN) với `unaccent` và `websearch_to_tsquery` có tham số; tìm trong tiêu đề, mô tả, nội dung khóa.
- **Acceptance criteria:**
  - Gõ "lap trinh" khớp khóa "Lập trình"
  - Chuỗi chứa ký tự SQL không gây lỗi hay injection

### T42 App quản trị: Category (CRUD)

- **Epic:** Course · **FR/NFR:** FR-17 · **Phụ thuộc:** T26, T32
- **Mô tả:** Bảng + form + modal xóa theo ux.md.
- **Acceptance criteria:**
  - Tạo/sửa/xóa Category trong UI thấy cập nhật ngay
  - Đủ 4 trạng thái Loading/Empty/Error/Success

### T43 App quản trị: Khóa học (danh sách, form tạo/sửa, thumbnail, modal sửa khóa Đã đăng)

- **Epic:** Course · **FR/NFR:** FR-19, FR-22, FR-23, NFR-23 · **Phụ thuộc:** T26, T33, T35, T38
- **Mô tả:** Danh sách khóa theo vai trò, form chia nhóm, nút Lưu dính đáy, chọn thumbnail, modal cảnh báo khi sửa khóa Đã đăng.
- **Acceptance criteria:**
  - Instructor chỉ thấy khóa của mình; khóa mới Chưa đăng
  - Sửa khóa Đã đăng hiện modal nêu hệ quả rồi mới lưu

### T44 App quản trị: Bài học (thêm/sửa/xóa, kéo thả, Xem thử)

- **Epic:** Course · **FR/NFR:** FR-20, FR-45, NFR-23 · **Phụ thuộc:** T34, T39, T43
- **Mô tả:** Quản lý Bài học trong khóa, kéo thả sắp xếp, bật Xem thử.
- **Acceptance criteria:**
  - Kéo thả đổi thứ tự và giữ sau khi tải lại
  - Bật Xem thử hiện nhãn Xem thử

### T45 App quản trị: Gửi duyệt, badge trạng thái, banner Lý do từ chối, Gửi lại

- **Epic:** Course · **FR/NFR:** FR-21, FR-49, FR-51 · **Phụ thuộc:** T36, T37, T44
- **Mô tả:** Checklist trước Gửi duyệt, badge Chưa đăng/Chờ duyệt (khóa học)/Đã đăng/Bị từ chối, banner Lý do từ chối kèm nút Gửi lại.
- **Acceptance criteria:**
  - Khóa chưa có Bài học hiện lý do chưa Gửi duyệt được
  - Khóa Bị từ chối hiện banner Lý do từ chối và nút Gửi lại

### T46 App quản trị: Duyệt khóa học (hàng đợi, Duyệt/Từ chối)

- **Epic:** Course · **FR/NFR:** FR-36, FR-49 · **Phụ thuộc:** T26, T37
- **Mô tả:** Màn hình hàng đợi Chờ duyệt (khóa học), xem đủ khóa và Bài học, modal Từ chối bắt buộc Lý do từ chối.
- **Acceptance criteria:**
  - Từ chối để trống lý do hiện lỗi dưới ô, modal không đóng
  - Khóa đã được Admin khác xử lý hiện toast lỗi và làm mới hàng

### T47 Web học viên: Danh sách khóa học (lọc, sắp xếp, tìm kiếm)

- **Epic:** Course · **FR/NFR:** FR-01, FR-44, NFR-03, NFR-11 · **Phụ thuộc:** T08, T09, T41
- **Mô tả:** Màn hình `/courses` với lưới thẻ responsive, phân trang, lọc Category, sắp xếp, ô tìm kiếm.
- **Acceptance criteria:**
  - Lọc/tìm không ra hiện "Không tìm thấy khóa học phù hợp" kèm "Xóa bộ lọc"
  - Render phía server, LCP đạt ngưỡng trên mạng giả lập

### T48 Web học viên: Chi tiết khóa học (SEO, nhãn Xem thử)

- **Epic:** Course · **FR/NFR:** FR-02, NFR-03, NFR-11 · **Phụ thuộc:** T09, T40
- **Mô tả:** Màn hình `/courses/[slug]` với metadata riêng mỗi khóa, thông tin, giá, Bài học (nhãn Xem thử), Instructor, trang không tìm thấy; nút mua nối vào ở T56.
- **Acceptance criteria:**
  - Mỗi khóa có `<title>` và description riêng trong HTML trả về
  - Slug không tồn tại hoặc khóa chưa Đã đăng hiện trang không tìm thấy

### T49 Tải lên Video: chỉ mp4 ≤ 1 GB, kiểm tra magic bytes, trạng thái xử lý

- **Epic:** Video · **FR/NFR:** FR-35, NFR-21 · **Phụ thuộc:** T23, T34
- **Mô tả:** Endpoint upload kiểm tra đuôi, magic bytes mp4 và 1 GB (Kestrel + Nginx), tên file GUID, tạo `VideoProcessingStatus` = đang xử lý.
- **Acceptance criteria:**
  - File không phải mp4 hoặc > 1 GB bị từ chối
  - Upload hợp lệ tạo Video ở trạng thái đang xử lý

### T50 Worker ffmpeg: HLS AES-128, giới hạn song song, Sẵn sàng/Lỗi

- **Epic:** Video · **FR/NFR:** FR-35, NFR-20, NFR-21 · **Phụ thuộc:** T49
- **Mô tả:** BackgroundService gọi ffmpeg bằng danh sách tham số (không qua shell) có timeout, segment 2-4 giây, mã hóa AES-128, ghi ra thư mục ngoài wwwroot; lỗi thì đặt trạng thái Lỗi.
- **Acceptance criteria:**
  - Upload mp4 hợp lệ thì thành Sẵn sàng với playlist và key
  - File hỏng thành Lỗi và không làm sập API

### T51 App quản trị: Upload video (tiến độ %, hủy, kiểm tra khi chọn, badge trạng thái)

- **Epic:** Video · **FR/NFR:** FR-35, NFR-23 · **Phụ thuộc:** T44, T49
- **Mô tả:** Chọn file thì báo lỗi ngay nếu sai định dạng/dung lượng; thanh tiến độ %, nút Hủy, badge Đang xử lý/Sẵn sàng/Lỗi; upload đứt hiện "Tải lên bị gián đoạn" kèm "Tải lại".
- **Acceptance criteria:**
  - Chọn file avi hoặc > 1 GB hiện "Chỉ nhận video mp4, tối đa 1 GB." trước khi upload
  - Sau upload badge Đang xử lý rồi tự đổi sang Sẵn sàng hoặc Lỗi

### T52 Enrollment: thực thể, unique (Học viên, Khóa học), truy vấn sở hữu và Khóa học của tôi

- **Epic:** Enrollment · **FR/NFR:** FR-15, NFR-04 · **Phụ thuộc:** T03, T33
- **Mô tả:** Bảng Enrollment với unique index, truy vấn "đã sở hữu khóa X chưa" và danh sách Khóa học của tôi.
- **Acceptance criteria:**
  - Chèn trùng (Học viên, Khóa học) bị chặn bởi unique index
  - Truy vấn Khóa học của tôi chỉ trả khóa của chính Học viên

### T53 Coupon: CRUD cho Admin và tính giá

- **Epic:** Cart/Coupon · **FR/NFR:** FR-07, FR-31 · **Phụ thuộc:** T19, T33
- **Mô tả:** Admin tạo Coupon (% giảm, thời hạn, giới hạn khóa); hàm tính giá cuối và lỗi rõ ràng khi hết hạn hoặc không áp dụng.
- **Acceptance criteria:**
  - Coupon hết hạn trả lỗi nêu lý do cụ thể
  - Chỉ Admin tạo/sửa được Coupon

### T54 Giỏ hàng: thêm, xóa khóa, áp Coupon, chặn khóa đã mua

- **Epic:** Cart/Coupon · **FR/NFR:** FR-06, FR-07, FR-29, FR-30 · **Phụ thuộc:** T52, T53
- **Mô tả:** Mỗi Học viên một Giỏ hàng; thêm/xóa khóa; áp Coupon tính Giá cuối; khóa đã có Enrollment bị từ chối.
- **Acceptance criteria:**
  - Thêm khóa đã mua bị từ chối
  - Xóa khóa làm Giá cuối tính lại

### T55 App quản trị: Coupon (CRUD)

- **Epic:** Cart/Coupon · **FR/NFR:** FR-31 · **Phụ thuộc:** T26, T53
- **Mô tả:** Bảng + form Coupon cho Admin.
- **Acceptance criteria:**
  - Tạo Coupon trong UI rồi áp được ở Giỏ hàng
  - Đủ 4 trạng thái Loading/Empty/Error/Success

### T56 Web học viên: Giỏ hàng, Coupon, Thêm vào giỏ, Vào học

- **Epic:** Cart/Coupon · **FR/NFR:** FR-06, FR-07, FR-29, FR-30 · **Phụ thuộc:** T25, T48, T54
- **Mô tả:** Nút Thêm vào giỏ trên Chi tiết khóa học (Khách bấm thì Đăng nhập bằng Google), Giỏ hàng với ô Coupon, nút Vào học khi đã có Enrollment.
- **Acceptance criteria:**
  - Toast "Đã thêm vào giỏ hàng" kèm link, số trên header cập nhật
  - Khóa đã mua hiện nút Vào học thay nút mua

### T57 Checkout idempotent → Đơn Mới (hạn 15 phút), chặn khóa đã mua, rate limit

- **Epic:** Order · **FR/NFR:** FR-08, FR-09, FR-30, NFR-10 · **Phụ thuộc:** T06, T54
- **Mô tả:** Checkout chuyển Giỏ hàng thành Đơn trạng thái Mới với `OrderItem` giữ Giá cuối; cùng idempotency key chỉ tạo 1 Đơn; từ chối khóa đã mua hoặc đang trong Đơn Mới/Đang chờ còn hiệu lực; nhóm checkout giới hạn 10 request/phút.
- **Acceptance criteria:**
  - Checkout hai lần cùng idempotency key chỉ tạo đúng 1 Đơn
  - Request thứ 11 trong 1 phút trả 429

### T58 Mã QR VietQR, Nội dung chuyển khoản duy nhất, truy vấn trạng thái Đơn

- **Epic:** Payment · **FR/NFR:** FR-10, FR-11 · **Phụ thuộc:** T57
- **Mô tả:** Sinh Mã QR theo VietQR từ tài khoản trong `appsettings.json` kèm số tiền và Nội dung chuyển khoản duy nhất; endpoint trả trạng thái hiện tại của Đơn.
- **Acceptance criteria:**
  - Mã QR chứa đúng số tiền và Nội dung chuyển khoản của Đơn
  - Hai Đơn không bao giờ trùng Nội dung chuyển khoản

### T59 Hook thanh toán: X-API-Token, Đã thanh toán đúng 1 lần, ghi Payment và Outbox cùng transaction

- **Epic:** Payment · **FR/NFR:** FR-12, NFR-04, NFR-05, NFR-07, NFR-10, NFR-22 · **Phụ thuộc:** T23, T58
- **Mô tả:** Hook xác thực `X-API-Token` (so sánh thời gian cố định), đủ tiền thì UPDATE có điều kiện sang Đã thanh toán, ghi Payment và OutboxMessage cùng transaction, trả 200/400/500 kèm `errorCode`/`errorMessage`, giới hạn theo IP.
- **Acceptance criteria:**
  - Thiếu hoặc sai token trả 400 `INVALID_TOKEN` và không đổi dữ liệu
  - Gọi hook trùng nhiều lần vẫn chỉ 1 lần Đã thanh toán và 1 OutboxMessage

### T60 Hook: tiền thừa/thiếu → Chênh lệch tiền, Đơn sang Đang chờ

- **Epic:** Payment · **FR/NFR:** FR-13 · **Phụ thuộc:** T59
- **Mô tả:** Hook báo sai số tiền thì ghi `AmountDiscrepancy`, Đơn sang Đang chờ, trả 400 `AMOUNT_MISMATCH`.
- **Acceptance criteria:**
  - Số tiền thiếu hoặc thừa không tạo Enrollment
  - Đơn Đang chờ hiện Chênh lệch tiền cho Admin

### T61 Job Hết hạn sau 15 phút và tự Đã hủy sau 1 giờ

- **Epic:** Order · **FR/NFR:** FR-32, FR-52 · **Phụ thuộc:** T23, T57
- **Mô tả:** BackgroundService quét mỗi 30 giây theo index `(Status, ExpiresAt)`: Đơn Mới quá 15 phút thành Hết hạn; Hết hạn quá 1 giờ chưa xử lý thành Đã hủy.
- **Acceptance criteria:**
  - Đơn Mới quá 15 phút chuyển Hết hạn
  - Đơn Hết hạn quá 1 giờ chuyển Đã hủy

### T62 Hook cho Đơn Hết hạn/Đã hủy: ghi nhận, sang Đang chờ, trả 200

- **Epic:** Payment · **FR/NFR:** FR-53 · **Phụ thuộc:** T59, T61
- **Mô tả:** Hook báo tiền cho Đơn Hết hạn hoặc Đã hủy (hoặc Mới quá hạn) ghi Payment, đổi Đơn sang Đang chờ, trả 200, không tạo Enrollment.
- **Acceptance criteria:**
  - Đơn Hết hạn nhận hook thành Đang chờ và hook trả 200
  - Không có Enrollment nào được tạo

### T63 Enrollment từ Outbox (idempotent) và email hoàn thành đơn

- **Epic:** Enrollment · **FR/NFR:** FR-14, NFR-02, NFR-04, NFR-05 · **Phụ thuộc:** T24, T52, T59
- **Mô tả:** Job Outbox tạo Enrollment cho từng khóa trong Đơn đúng 1 lần và gửi email qua Resend (Mã đơn, danh sách khóa, tổng tiền, liên kết `/my-courses`).
- **Acceptance criteria:**
  - Từ Đã thanh toán đến Enrollment ≤ 30 giây p95 trên môi trường test
  - Xử lý lại cùng message không tạo Enrollment trùng

### T64 Xử lý đơn (OrderResolution): Admin chuyển sang Đã thanh toán/Đã hủy, thu hồi Enrollment

- **Epic:** Order · **FR/NFR:** FR-14, FR-33 · **Phụ thuộc:** T59, T63
- **Mô tả:** Admin chuyển Đơn ở bất kỳ trạng thái sang Đã thanh toán (tạo Enrollment) hoặc Đã hủy (thu hồi Enrollment nếu có); lưu người thực hiện.
- **Acceptance criteria:**
  - Chuyển Đang chờ sang Đã thanh toán tạo Enrollment
  - Hủy Đơn đã có Enrollment thu hồi quyền học

### T65 Lịch sử đơn và danh sách Đơn cho Admin (backend)

- **Epic:** Order · **FR/NFR:** FR-24, FR-37 · **Phụ thuộc:** T57
- **Mô tả:** Học viên xem đơn của mình; Admin xem mọi Đơn có lọc trạng thái, Đang chờ và Hết hạn xếp trước.
- **Acceptance criteria:**
  - Học viên không xem được đơn của người khác
  - Admin thấy cả Đơn thiếu/thừa tiền kèm Chênh lệch tiền

### T66 Integration test race: Đã thanh toán trùng/đồng thời, Enrollment không trùng, crash giữa chừng

- **Epic:** Order · **FR/NFR:** NFR-04, NFR-05 · **Phụ thuộc:** T59, T60, T62, T63
- **Mô tả:** Bộ test với PostgreSQL thật: 20 hook đồng thời cho một Đơn, mô phỏng crash giữa Paid và Outbox, test checkout đồng thời.
- **Acceptance criteria:**
  - 20 hook đồng thời chỉ cho 1 lần Đã thanh toán và 1 Enrollment
  - Crash sau commit vẫn tạo đủ Enrollment khi job chạy lại

### T67 Web học viên: Checkout và Thanh toán (Mã QR, đếm ngược, polling, các trạng thái)

- **Epic:** Payment · **FR/NFR:** FR-08, FR-09, FR-10, FR-11, FR-13, FR-32, FR-53 · **Phụ thuộc:** T25, T56, T58
- **Mô tả:** Màn hình Thanh toán chỉ có Mã QR, số tiền, Nội dung chuyển khoản (nút sao chép), Mã đơn, đếm ngược 15 phút, polling ~3 giây; xử lý Đã thanh toán, Hết hạn, Đang chờ, lỗi 429.
- **Acceptance criteria:**
  - Đơn Đã thanh toán hiện "Thanh toán thành công" rồi nút Vào học
  - Hết hạn và Đang chờ hiện đúng thông điệp ở ux.md

### T68 Web học viên: Khóa học của tôi và Lịch sử đơn

- **Epic:** Enrollment · **FR/NFR:** FR-15, FR-37 · **Phụ thuộc:** T25, T52, T65
- **Mô tả:** Màn hình `/my-courses` và Lịch sử đơn (bảng từ `md` lên, thẻ dọc dưới `md`).
- **Acceptance criteria:**
  - Chỉ thấy khóa/đơn của chính mình
  - Chưa có khóa hiện "Bạn chưa có khóa học nào" kèm "Xem khóa học"

### T69 App quản trị: Đơn (danh sách, chi tiết, Xử lý đơn)

- **Epic:** Order · **FR/NFR:** FR-24, FR-33 · **Phụ thuộc:** T26, T64, T65
- **Mô tả:** Bảng Đơn có lọc, chi tiết có Mã đơn, Học viên, khóa, Nội dung chuyển khoản, Chênh lệch tiền; modal nêu hệ quả trước khi chuyển trạng thái.
- **Acceptance criteria:**
  - Modal Đã hủy nêu việc thu hồi Enrollment
  - Đơn vừa đổi trạng thái hiện toast "Đơn vừa được cập nhật, vui lòng xem lại"

### T70 Phát video: token phát, manifest/segment/key qua API, Nginx X-Accel-Redirect

- **Epic:** Learning · **FR/NFR:** FR-16, FR-34, FR-46, NFR-08, NFR-20, NFR-21 · **Phụ thuộc:** T13, T38, T50, T52
- **Mô tả:** API cấp token phát sau khi kiểm tra Enrollment (bản LastApprovedVersion), Bài học xem thử hoặc chủ khóa/Admin; mọi request manifest/segment/key qua API rồi Nginx phục vụ file.
- **Acceptance criteria:**
  - Chưa có Enrollment gọi thẳng API hoặc URL file video bị từ chối
  - Bài học xem thử phát được cho Khách; video bắt đầu ≤ 3 giây p95

### T71 Báo lỗi video: Học viên gửi, Admin nhận (backend)

- **Epic:** Learning · **FR/NFR:** FR-50 · **Phụ thuộc:** T50, T52
- **Mô tả:** Học viên có Enrollment gửi Báo lỗi video kèm Bài học; Admin xem danh sách.
- **Acceptance criteria:**
  - Người chưa có Enrollment gửi bị từ chối
  - Báo lỗi xuất hiện trong danh sách của Admin kèm Bài học

### T72 Web học viên: Trang học (player HLS, danh sách Bài học, trạng thái, Gửi báo lỗi)

- **Epic:** Learning · **FR/NFR:** FR-16, FR-34, FR-35, FR-46, FR-50, NFR-20 · **Phụ thuộc:** T68, T70, T71
- **Mô tả:** Video + danh sách Bài học, trạng thái Đang xử lý và Lỗi kèm nút Gửi báo lỗi, Khách xem thử không cần đăng nhập, điều khiển bằng bàn phím.
- **Acceptance criteria:**
  - Video Lỗi hiện "Video này đang gặp sự cố" và Gửi báo lỗi xong nút thành "Đã gửi báo lỗi"
  - Khóa đang chờ duyệt lại vẫn học được và không hiện banner

### T73 App quản trị: Báo lỗi video

- **Epic:** Learning · **FR/NFR:** FR-50 · **Phụ thuộc:** T26, T71
- **Mô tả:** Danh sách báo lỗi kèm Bài học liên quan.
- **Acceptance criteria:**
  - Báo lỗi mới của Học viên xuất hiện trong danh sách
  - Đủ 4 trạng thái Loading/Empty/Error/Success

### T74 Tỷ lệ chia chung (cấu hình) và Tỷ lệ chia riêng (đặt/xóa)

- **Epic:** Revenue · **FR/NFR:** FR-38, FR-39 · **Phụ thuộc:** T28
- **Mô tả:** Đọc Tỷ lệ chia chung từ cấu hình; Admin đặt/xóa Tỷ lệ chia riêng cho Instructor, chỉ có tác dụng với Đơn hoàn thành sau lúc đổi.
- **Acceptance criteria:**
  - Đặt Tỷ lệ chia riêng không đổi Đơn đã Đã thanh toán
  - Xóa thì quay về Tỷ lệ chia chung

### T75 Khoản chia: sao chép tỷ lệ vào Đơn và ghi Khoản chia chờ duyệt khi Đã thanh toán

- **Epic:** Revenue · **FR/NFR:** FR-38 · **Phụ thuộc:** T59, T64, T74
- **Mô tả:** Khi Đơn Đã thanh toán (từ hook hoặc Admin), cùng transaction sao chép tỷ lệ vào Order và ghi `RevenueShare` PendingShare cho từng khóa.
- **Acceptance criteria:**
  - Mỗi khóa trong Đơn Đã thanh toán sinh đúng 1 Khoản chia chờ duyệt
  - Đổi tỷ lệ sau đó không thay đổi Đơn cũ

### T76 Duyệt Khoản chia và tính Số dư

- **Epic:** Revenue · **FR/NFR:** FR-47 · **Phụ thuộc:** T75
- **Mô tả:** Admin duyệt Khoản chia thì cộng vào Số dư (tổng khoản chia đã duyệt trừ Phiếu chi).
- **Acceptance criteria:**
  - Duyệt xong Số dư Instructor tăng đúng số tiền
  - Khoản chia không thể duyệt hai lần

### T77 Phiếu chi: Admin chọn số tiền, không vượt Số dư

- **Epic:** Revenue · **FR/NFR:** FR-48 · **Phụ thuộc:** T76
- **Mô tả:** Tạo Phiếu chi (Instructor, số tiền, thời điểm, người tạo); vượt Số dư bị từ chối.
- **Acceptance criteria:**
  - Số tiền vượt Số dư bị từ chối
  - Phiếu chi hợp lệ trừ vào Số dư và lưu người tạo

### T78 Doanh thu: truy vấn dashboard theo Instructor và khoảng thời gian (backend)

- **Epic:** Revenue · **FR/NFR:** FR-25 · **Phụ thuộc:** T75, T76
- **Mô tả:** Admin thấy mọi Instructor; Instructor chỉ thấy của mình kèm Khoản chia và Số dư.
- **Acceptance criteria:**
  - Instructor gọi dữ liệu của người khác bị từ chối
  - Số liệu khớp tổng Đơn Đã thanh toán trong khoảng chọn

### T79 App quản trị: Doanh thu

- **Epic:** Revenue · **FR/NFR:** FR-25 · **Phụ thuộc:** T26, T78
- **Mô tả:** Màn hình Doanh thu với bộ lọc thời gian và bảng theo Instructor.
- **Acceptance criteria:**
  - Admin chọn khoảng thời gian thấy số liệu đúng
  - Instructor chỉ thấy phần của mình, kèm Khoản chia và Số dư

### T80 App quản trị: Tỷ lệ chia riêng trên màn hình Instructor

- **Epic:** Revenue · **FR/NFR:** FR-39 · **Phụ thuộc:** T30, T74
- **Mô tả:** Đặt/xóa Tỷ lệ chia riêng trong màn hình Instructor.
- **Acceptance criteria:**
  - Đặt tỷ lệ riêng hiển thị trên Instructor đó
  - Xóa thì hiển thị Tỷ lệ chia chung

### T81 App quản trị: Khoản chia chờ duyệt

- **Epic:** Revenue · **FR/NFR:** FR-47 · **Phụ thuộc:** T26, T76
- **Mô tả:** Danh sách và nút Duyệt Khoản chia.
- **Acceptance criteria:**
  - Duyệt xong hàng rời khỏi danh sách, toast "Đã duyệt Khoản chia"
  - Không có khoản chờ hiện "Không có Khoản chia chờ duyệt"

### T82 App quản trị: Phiếu chi (Tạo phiếu chi)

- **Epic:** Revenue · **FR/NFR:** FR-48 · **Phụ thuộc:** T26, T77
- **Mô tả:** Chọn Instructor (hiện Số dư), nhập số tiền, modal xác nhận, ghi chú "Kế toán chuyển khoản ngoài hệ thống".
- **Acceptance criteria:**
  - Số tiền vượt Số dư hiện lỗi dưới ô và chặn gửi
  - Tạo xong Số dư cập nhật tại chỗ

### T83 Bộ test phân quyền API: vai trò, IDOR, URL file video

- **Epic:** Chất lượng & vận hành · **FR/NFR:** FR-05, NFR-08, NFR-21 · **Phụ thuộc:** T64, T70, T77
- **Mô tả:** Integration test theo ma trận quyền trong architecture.md mục 4, gọi thẳng API và URL file video.
- **Acceptance criteria:**
  - Mọi ô "—" trong ma trận đều bị từ chối
  - 0 lần xem video không xem thử khi chưa có Enrollment

### T84 🚀 Bộ E2E và job `e2e` trong CI

- **Epic:** Chất lượng & vận hành · **FR/NFR:** NFR-16 · **Phụ thuộc:** T11, T63, T67, T72
- **Mô tả:** Playwright: đăng nhập → mua → gọi hook → Enrollment → phát video; thêm job `e2e` sau `integration`.
- **Acceptance criteria:**
  - Luồng mua trọn vẹn pass trong CI
  - E2E fail thì không build/deploy

### T85 🚀 Lighthouse SEO và LCP trên staging

- **Epic:** Chất lượng & vận hành · **FR/NFR:** NFR-03, NFR-11 · **Phụ thuộc:** T15, T47, T48
- **Mô tả:** Chạy Lighthouse sau `deploy-stg` cho `/courses` và `/courses/[slug]`; dưới ngưỡng thì chặn production.
- **Acceptance criteria:**
  - Điểm SEO ≥ 90
  - LCP ≤ 2,5 giây (p75, mạng di động giả lập)

### T86 🚀 Load test: danh sách/chi tiết, 500 người dùng đồng thời, video

- **Epic:** Chất lượng & vận hành · **FR/NFR:** NFR-01, NFR-12, NFR-20 · **Phụ thuộc:** T15, T40, T70
- **Mô tả:** Chạy tải 100 request/giây lên API công khai và 500 phiên đồng thời; đo thời gian bắt đầu phát video.
- **Acceptance criteria:**
  - p95 danh sách và chi tiết ≤ 300 ms
  - Video bắt đầu ≤ 3 giây p95; ghi nhận giới hạn băng thông VPS

### T87 🚀 Đo Đã thanh toán → Enrollment và cảnh báo Outbox tồn đọng

- **Epic:** Chất lượng & vận hành · **FR/NFR:** NFR-02 · **Phụ thuộc:** T15, T63
- **Mô tả:** Đo p95 trên staging, thêm cảnh báo Outbox tồn đọng > 30 giây.
- **Acceptance criteria:**
  - p95 ≤ 30 giây
  - Chặn consumer thử thì cảnh báo phát ra

### T88 🚀 Diễn tập khôi phục khi mất VPS

- **Epic:** Chất lượng & vận hành · **FR/NFR:** NFR-19 · **Phụ thuộc:** T16, T17
- **Mô tả:** Dựng VPS mới từ backup theo deploy.md mục 7 và đo thời gian thực tế.
- **Acceptance criteria:**
  - Khôi phục xong trong RTO ≤ 4 giờ, dữ liệu mất ≤ RPO 24 giờ
  - Ghi lại thời gian thực tế vào tài liệu deploy

### T89 🚀 Thử nghiệm với ≥ 3 Instructor thật

- **Epic:** Chất lượng & vận hành · **FR/NFR:** NFR-23 · **Phụ thuộc:** T15, T31, T45, T51
- **Mô tả:** Mỗi Instructor tự làm luồng cập nhật hồ sơ, tạo khóa và Bài học, upload video, Gửi duyệt trên staging.
- **Acceptance criteria:**
  - ≥ 3 Instructor hoàn thành ở lần thử đầu, không cần hỗ trợ trực tiếp
  - Các điểm vướng được ghi lại thành thẻ mới

### T90 🚀 Kiểm tra hạn mức Resend gói free và cảnh báo

- **Epic:** Chất lượng & vận hành · **FR/NFR:** NFR-18 · **Phụ thuộc:** T15, T24
- **Mô tả:** Xác minh hạn mức thực tế của Resend, thêm cảnh báo khi gần chạm hạn mức.
- **Acceptance criteria:**
  - Có số hạn mức đối chiếu với dự báo 10.000 tài khoản
  - Cảnh báo phát ra khi gần hạn mức

### T91 🚀 Go-live production lần đầu

- **Epic:** Chất lượng & vận hành · **FR/NFR:** NFR-09, NFR-14, NFR-22 · **Phụ thuộc:** T16, T18, T85, T88
- **Mô tả:** Deploy production khi người dùng xác nhận rõ ràng: nạp secret thật, token hook production, đổi mật khẩu Admin seed, kiểm tra uptime.
- **Acceptance criteria:**
  - Smoke test production pass, HTTPS hợp lệ
  - Mật khẩu Admin seed đã được đổi; không có secret trong repo

## Đối chiếu FR/NFR → thẻ

| Mã | Thẻ |
|---|---|
| FR-01 | T40, T47 |
| FR-02 | T40, T48 |
| FR-03 | T22, T25, T27 |
| FR-04 | T19, T21, T26 |
| FR-05 | T19, T26, T83 |
| FR-06 | T54, T56 |
| FR-07 | T53, T54, T56 |
| FR-08 | T57, T67 |
| FR-09 | T57, T67 |
| FR-10 | T58, T67 |
| FR-11 | T58, T67 |
| FR-12 | T59 |
| FR-13 | T60, T67 |
| FR-14 | T63, T64 |
| FR-15 | T52, T68 |
| FR-16 | T70, T72 |
| FR-17 | T32, T42 |
| FR-18 | T29, T30 |
| FR-19 | T33, T43 |
| FR-20 | T34, T44 |
| FR-21 | T36, T45 |
| FR-22 | T33, T38, T43 |
| FR-23 | T35, T43 |
| FR-24 | T65, T69 |
| FR-25 | T78, T79 |
| FR-26 | T02 |
| FR-27 | T24 |
| FR-28 | **chưa có thẻ** |
| FR-29 | T54, T56 |
| FR-30 | T54, T56, T57 |
| FR-31 | T53, T55 |
| FR-32 | T61, T67 |
| FR-33 | T64, T69 |
| FR-34 | T70, T72 |
| FR-35 | T49, T50, T51, T72 |
| FR-36 | T37, T46 |
| FR-37 | T65, T68 |
| FR-38 | T74, T75 |
| FR-39 | T74, T80 |
| FR-40 | T28, T30 |
| FR-41 | T20 |
| FR-42 | T21, T26 |
| FR-43 | T29, T31 |
| FR-44 | T41, T47 |
| FR-45 | T39, T44 |
| FR-46 | T70, T72 |
| FR-47 | T76, T81 |
| FR-48 | T77, T82 |
| FR-49 | T37, T38, T45, T46 |
| FR-50 | T71, T72, T73 |
| FR-51 | T36, T45 |
| FR-52 | T61 |
| FR-53 | T62, T67 |
| NFR-01 | T06, T40, T41, T86 |
| NFR-02 | T23, T63, T87 |
| NFR-03 | T09, T47, T48, T85 |
| NFR-04 | T52, T59, T63, T66 |
| NFR-05 | T23, T59, T63, T66 |
| NFR-06 | T19, T25, T26, T27 |
| NFR-07 | T59 |
| NFR-08 | T19, T33, T70, T83 |
| NFR-09 | T06, T13, T15, T27, T91 |
| NFR-10 | T06, T57, T59 |
| NFR-11 | T09, T47, T48, T85 |
| NFR-12 | T02, T03, T86 |
| NFR-13 | T01, T02, T08 |
| NFR-14 | T04, T13, T14, T15, T16, T18, T91 |
| NFR-15 | T04, T18 |
| NFR-16 | T05, T11, T12, T15, T84 |
| NFR-17 | T01, T02, T03, T09, T10 |
| NFR-18 | T24, T90 |
| NFR-19 | T17, T18, T88 |
| NFR-20 | T50, T70, T72, T86 |
| NFR-21 | T49, T50, T70, T83 |
| NFR-22 | T14, T20, T59, T91 |
| NFR-23 | T07, T10, T31, T43, T44, T51, T89 |

### FR/NFR chưa có thẻ

| Mã | Lý do |
|---|---|
| FR-28 | Đã bỏ khỏi phạm vi ngày 2026-11-11 (Quên mật khẩu), PRD giữ mã để không đánh lại số |

### Vấn đề mở ảnh hưởng thẻ

- **T64/T75**: hủy Đơn đã Đã thanh toán thì Khoản chia đã sinh xử lý thế nào (hủy khoản chia chờ duyệt? Số dư có âm?) chưa có quy tắc (architecture.md mục 7). Cần chủ dự án quyết trước khi làm T64.
- **T59**: payload hook, mã giao dịch ngân hàng và cộng dồn tiền chuyển bù chưa định nghĩa; hiện mỗi lần gọi xét riêng. Cần chốt khi tích hợp đối tác thật.
- **T18**: kênh nhận alert chưa chốt, hỏi chủ dự án trước khi cấu hình.
- **T07, T09, T10**: design tokens trong ux.md đang `[ĐỀ XUẤT - CẦN DUYỆT]`; log chọn Tailwind + shadcn/ui, kiến trúc modular monolith, 1 VPS, Outbox, MinIO, HLS đang `Proposed`.
- **T86, T51**: 500 người xem video đồng thời phụ thuộc băng thông VPS; upload 1 GB không resume (architecture.md mục 7).
- `docs/conventions.md` (mục 5) và `CLAUDE.md` chưa có trong repo; Definition of Done chưa được định nghĩa ở nơi nào nên quy ước ở trên chỉ nêu tạm. (Template `CLAUDE.md` đã có ở `docs/prompts/templates/`, còn file ở gốc repo thì chưa tạo.)
