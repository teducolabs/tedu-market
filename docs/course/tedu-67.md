# TEDU-67: Xây dựng ứng dụng bán khóa học trực tuyến với ASP.NET Core 10 — Clean Architecture, CQRS, OpenIddict & React/Next.js

| Mục | Nội dung |
|---|---|
| Mã | TEDU-67 |
| Trạng thái | Đang làm — dự kiến mở bán cuối tháng 11/2026 |
| Hình thức | Trả phí |
| Thuộc lộ trình | [Lộ trình .NET Backend](../../../dotnet-backend-roadmap.md) — **có trong combo** |
| Link trang khóa học | https://tedu.com.vn/khoa-hoc/xay-dung-website-hoc-truc-tuyen-voi-aspnet-core-2026-67.html |
| Giá | 1.296.000đ |
| Thời lượng / số bài | 10 module (Module 0–9) · 72 bài · ~49 giờ (2.960 phút, cộng theo outline) |
| Phiên bản công nghệ | .NET 10 (LTS) · React 19 (Vite) · Next.js 15 (App Router) · PostgreSQL 17 |
| Số học viên | TODO (chưa mở bán) |

> Tên cũ trên trang khóa học / slug URL: "Xây dựng website học trực tuyến với ASP.NET Core – 2026".

## Giới thiệu

Xây dựng ứng dụng bán khóa học trực tuyến với ASP.NET Core 10, Clean Architecture, CQRS, OpenIddict & React/Next.js.

100% thực hành, lý thuyết lồng ghép trong từng bài, không có bài "chỉ lý thuyết" đứng riêng quá 1 buổi. Dự án xuyên suốt: **TEDU Market** — nền tảng bán khóa học trực tuyến (không phải LMS quản lý nội bộ).

## Đối tượng và yêu cầu đầu vào

- Developer đã biết C# và ASP.NET Core cơ bản (Controller, EF Core CRUD đơn giản) — phù hợp học viên đã hoàn thành khóa "Xây dựng website học trực tuyến với ASP.NET Core" hoặc tương đương.
- Muốn nâng trình độ lên kiến trúc production thật (Clean Architecture, CQRS, OAuth2 chuẩn, full-stack với React/Next.js), không dừng ở mức "chạy được".
- Muốn có 1 project full-stack hoàn chỉnh (backend + 2 frontend) làm portfolio.

**Không phù hợp cho:** người chưa từng làm Web API — nên học khóa nền tảng ASP.NET Core trước.

## Kết quả đầu ra

1. Xây được toàn bộ 1 hệ thống bán khóa học trực tuyến production-ready — không phải project demo rời rạc từng kỹ thuật.
2. Dùng Clean Architecture + CQRS làm khung xương ngay từ bài đầu tiên, không phải "học layered trước, refactor sau".
3. Xây Authorization Server thật bằng OpenIddict — hiểu và tự cấu hình OAuth2/OIDC, không chỉ copy-paste JWT.
4. Generate API client type-safe bằng openapi-ts, dùng chung cho 2 frontend app trong 1 monorepo.
5. Viết Unit Test và Integration Test bảo vệ logic nghiệp vụ.
6. Deploy toàn bộ hệ thống (1 backend + 2 frontend) lên production với Docker, Nginx multi-app, CI/CD.

## Dự án thực hành — TEDU Market

Nền tảng bán khóa học trực tuyến. Khác "Mini LMS" ở khóa nền tảng: trọng tâm là hành trình thương mại — duyệt khóa học, giỏ hàng, thanh toán, sở hữu quyền truy cập.

| Bounded context | Aggregate / thành phần |
|---|---|
| Catalog | Course, Category, Lesson, Instructor |
| Ordering | Cart, Order, OrderItem, Coupon |
| Payment | Payment, tích hợp cổng thanh toán (xem Ghi chú sản xuất: VNPay hay SePay) |
| Enrollment | Quyền truy cập khóa học sau khi thanh toán thành công |
| Identity | User, Role — ASP.NET Core Identity + OpenIddict |

### Quyết định kiến trúc đã chốt (áp dụng xuyên suốt khóa)

1. **Query side tách khỏi Entity ghi.** Write model: EF Core + Aggregate. Read model: Dapper raw SQL chiếu thẳng ra DTO, không dùng lại Entity của Command side.
2. **OpenIddict chạy chung trong API project**, không tách Authorization Server thành service riêng (giảm độ phức tạp vận hành, vẫn đúng chuẩn OAuth2/OIDC cho quy mô vừa). Module 9 chỉ ghi chú hướng tách khi cần scale.
3. **Monorepo, API client là package riêng.** Turborepo + pnpm workspace; `packages/api-client` generate bằng `@hey-api/openapi-ts`, dùng chung cho `apps/admin` (React) và `apps/web` (Next.js).
4. **Domain bán khóa học** (Cart, Order, Coupon, Payment, Enrollment kích hoạt qua domain event) để có lý do thật dạy CQRS, Outbox Pattern, Idempotency.

### Cấu trúc solution

```
backend/
  src/
    TeduMarket.Domain/          # Entity, Value Object, Aggregate, Domain Event
    TeduMarket.Application/     # Command, Query, Handler, Validator, Behavior, interface
    TeduMarket.Infrastructure/  # EF Core, Dapper, OpenIddict, Hangfire, VNPay adapter
    TeduMarket.Api/             # Minimal API endpoint, Program.cs, middleware
  tests/
    TeduMarket.Application.UnitTests/
    TeduMarket.Api.IntegrationTests/
frontend/
  apps/
    admin/        # React (Vite) — quản trị Admin/Instructor
    web/          # Next.js (App Router) — trang bán khóa học cho học viên
  packages/
    api-client/   # @hey-api/openapi-ts, dùng chung 2 app
  turbo.json
  pnpm-workspace.yaml
docker-compose.yml / docker-compose.prod.yml   # ở thư mục gốc
```

### Quy ước đặt tên (bắt buộc nhất quán mọi bài)

| Loại | Quy ước | Ví dụ |
|---|---|---|
| Namespace | `TeduMarket.{Layer}` | `TeduMarket.Application` |
| Command | `{Động từ}{Thực thể}Command` | `CreateCourseCommand`, `CheckoutCommand` |
| Command Handler | `{Tên Command}Handler` | `CreateCourseCommandHandler` |
| Query | `Get{Thực thể}Query` / `Get{Thực thể}ListQuery` | `GetCourseDetailQuery`, `GetCoursesListQuery` |
| Validator | `{Tên Command}Validator` | `CreateCourseCommandValidator` |
| Domain Event | `{Thực thể}{Động từ quá khứ}Event` | `CourseOrderedEvent`, `OrderPaidEvent` |
| Repository | `I{Aggregate}Repository` — interface ở Application, implement ở Infrastructure | `ICategoryRepository` |
| Endpoint route | `/api/v1/{số nhiều thực thể}` | `/api/v1/courses` |
| Lỗi nghiệp vụ | `{Thực thể}{LoạiLỗi}Exception`, map sang ProblemDetails ở Api | `CourseNotFoundException` |

### Hướng dẫn cho AI khi soạn bài giảng khóa này (dùng với skill `tedu-lesson-writer`)

- File này là outline chuẩn: khi được yêu cầu viết "Bài x.y" hay "Module n", lấy tên bài, loại, thời lượng, nội dung, deliverable từ bảng bên dưới. Không hỏi lại thông tin đã có.
- Số bài và tên bài cố định, không đổi.
- Trước khi viết 1 bài, xem bài liền trước trong module (và bài được tham chiếu ở module khác) để nối mạch code: code bài N là nền cho bài N+1.
- Giữ nguyên domain TEDU Market và namespace `TeduMarket.*`; tuân thủ bảng quy ước đặt tên.
- Khi dùng lại khái niệm đã dạy, ghi rõ "đã học ở bài X.Y, giờ thay bằng cách làm đúng chuẩn" thay vì dạy lại.
- Deliverable là tiêu chí hoàn thành bài: phải chạy được thật.
- Phần "Lý thuyết" chỉ ở đầu bài (tối đa ~30–45 phút), còn lại là Demo/Lab trên chính TEDU Market.

## Giáo trình

| Module | Chủ đề | Số bài | Thời lượng |
|---|---|---|---|
| 0 | Kiến trúc nền & khởi tạo dự án | 10 | 450 phút |
| 1 | Catalog: Course, Lesson, Instructor | 10 | 450 phút |
| 2 | Order & Payment | 9 | 450 phút |
| 3 | OpenIddict — Authorization Server | 9 | 360 phút |
| 4 | API hoàn thiện & OpenAPI-first | 5 | 130 phút |
| 5 | Monorepo & API Client Codegen | 4 | 130 phút |
| 6 | React Admin | 6 | 255 phút |
| 7 | Next.js Student Site | 7 | 285 phút |
| 8 | Testing | 4 | 165 phút |
| 9 | Production | 8 | 285 phút |
| **Tổng** | | **72** | **2.960 phút (~49 giờ)** |

### Module 0 — Kiến trúc nền & khởi tạo dự án

Mục tiêu: dựng khung xương Clean Architecture + CQRS chạy end-to-end với 1 use case đủ 4 layer ngay từ đầu, không đợi cuối khóa mới refactor.

| Bài | Tên bài · Loại · Thời lượng | Nội dung chính | Deliverable |
|---|---|---|---|
| 0.1 | Tổng quan dự án & bản đồ kiến trúc · Lý thuyết · 30' | Toàn bộ hệ thống: backend Clean Architecture + CQRS, OpenIddict, React Admin, Next.js, monorepo. Sơ đồ big picture. Vì sao chọn kiến trúc này thay layered: đổi độ phức tạp ban đầu lấy khả năng mở rộng và test. | Sơ đồ kiến trúc hệ thống dùng xuyên suốt khóa |
| 0.2 | Khởi tạo Clean Architecture Solution · Demo + Lab · 45' | Tạo 4 project Domain, Application, Infrastructure, Api; reference đúng chiều phụ thuộc. Giải thích DIP bằng ví dụ thực tế. | `TeduMarket.sln` 4 project rỗng, build thành công, reference đúng chiều |
| 0.3 | Domain Layer: Entity, Value Object, Aggregate Root · Lý thuyết + Demo · 60' | DDD tactical ở mức đủ dùng: Entity có identity, Value Object bất biến so sánh theo giá trị, Aggregate Root là điểm vào duy nhất giữ invariant. Aggregate đầu tiên: Category. | Category Aggregate + unit test invariant (tên không rỗng) |
| 0.4 | CQRS với MediatR: Command đầu tiên · Demo + Lab · 60' | Setup MediatR, `CreateCategoryCommand` + Handler. Vì sao CQRS là khung xương từ Module 0, không phải "thêm vào" ở module refactor cuối — khác biệt cốt lõi của khóa. | `CreateCategoryCommand` chạy qua MediatR, log ra console |
| 0.5 | FluentValidation Pipeline Behavior · Demo + Lab · 45' | `ValidationBehavior<TRequest,TResponse>` chạy tự động trước mọi Handler; validate tập trung một chỗ. | `CreateCategoryCommandValidator` tự động trả lỗi 400 có cấu trúc |
| 0.6 | Infrastructure: EF Core DbContext (Write Model) · Demo · 45' | `AppDbContext`, connection string, DI. Implement `ICategoryRepository` ở Infrastructure — Application không biết EF Core. | `CategoryRepository` xong, Handler lưu được xuống Postgres |
| 0.7 | Api Layer: Minimal API Endpoint đầu tiên · Demo + Lab · 45' | `POST /api/v1/categories` bằng Minimal API + Endpoint Group. Luồng end-to-end: Endpoint → MediatR → Validation → Handler → Repository → Postgres. | Endpoint chạy thật từ Scalar/Postman, trả 201 Created |
| 0.8 | Query Side riêng biệt với Dapper · Demo + Lab · 60' | `GetCategoriesQuery` dùng Dapper raw SQL. CQRS có lý do: Command tối ưu toàn vẹn, Query tối ưu tốc độ đọc. So với "CQRS hình thức" dùng chung EF Core. | `GET /api/v1/categories` qua Dapper, đo thời gian so với EF Core |
| 0.9 | Global Error Handling & ProblemDetails · Demo · 30' | Exception middleware ở Api, chuẩn RFC 7807. Exception nghiệp vụ ở Domain (`CategoryNotFoundException`) map sang HTTP status. | Mọi lỗi trả về ProblemDetails nhất quán |
| 0.10 | Docker Compose Dev Environment · Demo + Lab · 30' | Postgres + Redis bằng Docker Compose; verify Module 0 end-to-end trên hạ tầng thật. | `docker compose up`, CRUD Category chạy trên Postgres thật |

### Module 1 — Catalog: Course, Lesson, Instructor

Mục tiêu: áp khung xương Module 0 vào Course Aggregate; lặp pattern CQRS nhiều lần để thành phản xạ.

| Bài | Tên bài · Loại · Thời lượng | Nội dung chính | Deliverable |
|---|---|---|---|
| 1.1 | Thiết kế Domain Model Course Aggregate · Lý thuyết + Demo · 60' | Course chứa Lesson (child entity) và Pricing (Value Object). Invariant: phải có ≥1 Lesson mới publish được; vì sao invariant nằm ở Domain chứ không ở Validation. | Course, Lesson, Pricing + unit test invariant publish |
| 1.2 | Command CreateCourse & UpdateCourse · Demo + Lab · 60' | Chỉ Instructor sở hữu mới được sửa (check tạm ở Handler bằng InstructorId — chuẩn hóa ở 3.8). Lặp lại pattern lần 2. | `CreateCourseCommand`, `UpdateCourseCommand` đầy đủ |
| 1.3 | Quản lý Lesson trong Course · Demo + Lab · 45' | Thêm/sửa/xóa/reorder Lesson qua `Course.AddLesson(...)`, không update thẳng bảng Lesson — lỗi hay gặp nhất khi mới học DDD. | Command quản lý Lesson, không sửa được ngoài Aggregate |
| 1.4 | EF Core Fluent Configuration cho Aggregate phức tạp · Demo · 45' | Owned Type cho Pricing, quan hệ Course–Lesson 1-n, cascade delete. | `CourseConfiguration.cs`, migration chạy sạch |
| 1.5 | Migration Workflow trong Clean Architecture · Demo + Lab · 30' | Migration ở Infrastructure, connection string ở Api → `IDesignTimeDbContextFactory`. Seed data Category/Course. | `dotnet ef migrations add` chạy từ Infrastructure, seed khi `docker compose up` |
| 1.6 | Query Side: Danh sách Course có phân trang/filter/sort · Demo + Lab · 60' | Dapper OFFSET/FETCH, filter Category, sort giá/ngày. So performance với EF Core LINQ bằng `EXPLAIN ANALYZE`. | `GetCoursesQuery` đủ phân trang/filter/sort, có benchmark thật |
| 1.7 | Query Side: Course Detail & tránh N+1 · Demo · 30' | Dapper multi-mapping lấy Course + Lesson + Instructor trong 1 round-trip, so với N+1. | `GetCourseDetailQuery` chỉ 1 câu SQL |
| 1.8 | Upload ảnh Course · Demo + Lab · 45' | Object storage tương thích S3 (MinIO qua Docker Compose). Presigned URL để client upload trực tiếp. | `UpdateCourseThumbnail`, MinIO lưu ảnh, trả presigned URL |
| 1.9 | Category & Instructor Management · Demo + Lab · 30' | Bài luyện tổng hợp: CRUD Category, Instructor profile theo pattern 1.1–1.8, học viên tự làm phần lớn. | CRUD Category, Instructor hoàn chỉnh |
| 1.10 | Unit Test cho CQRS Handler · Demo + Lab · 45' | Test sớm: mock Repository interface; so độ dễ test của Handler với Controller truyền thống. | Unit test `CreateCourseCommandHandler`, `UpdateCourseCommandHandler` |

### Module 2 — Order & Payment: biến Catalog thành nền tảng bán khóa học

Mục tiêu: Cart, Order, thanh toán, và nhất quán dữ liệu khi thanh toán.

| Bài | Tên bài · Loại · Thời lượng | Nội dung chính | Deliverable |
|---|---|---|---|
| 2.1 | Thiết kế Domain Order: Cart, Order, OrderItem, Coupon · Lý thuyết + Demo · 45' | Boundary giữa Cart (mutable, thuộc User) và Order (immutable khi tạo, là bản ghi lịch sử); vì sao không gộp 1 Aggregate. | Cart, Order, OrderItem, Coupon |
| 2.2 | Command AddToCart & Checkout · Demo + Lab · 60' | `AddToCartCommand`, `CheckoutCommand` chuyển Cart thành Order; phát `CourseOrderedEvent`, chuẩn bị Outbox ở 2.6. | AddToCart → Checkout tạo Order trạng thái Pending |
| 2.3 | Coupon & Pricing Rule · Demo + Lab · 45' | Value Object Discount, Specification pattern nhẹ (% giảm, thời hạn, giới hạn Course). | `ApplyCouponCommand`, tính đúng giá cuối |
| 2.4 | Tạo yêu cầu thanh toán & mã QR chuyển khoản ngân hàng (SePay) · Demo + Lab · 60' | Liên kết tài khoản SePay, cấu hình mã thanh toán. `InitiatePaymentCommand` sinh mã thanh toán duy nhất gắn Order, tạo QR VietQR (STK/ngân hàng/số tiền/nội dung). `GetOrderPaymentStatusQuery` cho FE polling. | `InitiatePaymentCommand` trả QR hợp lệ, endpoint trạng thái đơn hoạt động |
| 2.5 | Xử lý Webhook xác nhận chuyển khoản (SePay) an toàn · Demo + Lab · 60' | Webhook SePay là nguồn sự thật duy nhất. Xác thực HMAC-SHA256 (`X-SePay-Signature`/`X-SePay-Timestamp`), idempotency theo id giao dịch, UPDATE atomic Pending→Paid so khớp code + amount. | Endpoint webhook verify chữ ký, Order sang Paid đúng 1 lần dù gọi nhiều lần |
| 2.6 | Outbox Pattern · Lý thuyết + Demo · 45' | Order Paid và tạo Enrollment phải nhất quán; crash giữa chừng thì sao? Ghi event vào bảng Outbox cùng transaction, xử lý bất đồng bộ sau. | `OutboxMessage`, ghi Outbox cùng transaction với OrderPaid |
| 2.7 | Background Job với Hosted Service · Demo + Lab · 45' | Hosted service đọc Outbox, tạo Enrollment, gửi email xác nhận. | Job chạy định kỳ xử lý Outbox, Enrollment tự tạo sau thanh toán |
| 2.8 | Query Side: Order History & Doanh thu Instructor · Demo + Lab · 45' | Read model riêng cho báo cáo, không đụng write model. Dapper tổng hợp theo Instructor, khoảng thời gian. | `GetOrderHistoryQuery`, `GetInstructorRevenueQuery` |
| 2.9 | Idempotency & Concurrency trong Checkout · Demo + Lab · 45' | Race condition khi bấm thanh toán 2 lần. Idempotency key ở `CheckoutCommand`, optimistic concurrency (RowVersion) trên Order. | Checkout an toàn khi gọi trùng, có test race condition |

### Module 3 — OpenIddict: Authorization Server

Mục tiêu: module khó nhất. Xây Authorization Server thật bằng OpenIddict thay vì tự ký JWT.

| Bài | Tên bài · Loại · Thời lượng | Nội dung chính | Deliverable |
|---|---|---|---|
| 3.1 | OAuth2 & OpenID Connect — khái niệm đúng · Lý thuyết · 45' | AuthN vs AuthZ. Authorization Code Flow, PKCE (vì sao SPA/mobile bắt buộc), Client Credentials. Authorization Server / Resource Server / Client. | Sơ đồ luồng OAuth2 cho TEDU Market (React Admin, Next.js, background job) |
| 3.2 | Setup OpenIddict Server · Demo + Lab · 60' | Endpoint token, authorize, userinfo; ASP.NET Core Identity làm user store. OpenIddict quản lý vòng đời token, client, scope. | OpenIddict chạy trong Api project, `/connect/token` hoạt động |
| 3.3 | Authorization Code + PKCE Flow cho SPA · Demo + Lab · 60' | Redirect login, `code_verifier`/`code_challenge`, đổi code lấy access token (dùng lại ở 6.2). | Client cho React Admin, test bằng Postman/curl |
| 3.4 | Client Credentials Flow · Demo + Lab · 30' | Background job (2.7) gọi API nội bộ an toàn, không cần user context. | Client cho background job, lấy token và gọi API thành công |
| 3.5 | Custom Claims & Scope · Demo · 30' | Đưa Role, InstructorId vào Access Token qua claims destination. | Access Token đúng claims, kiểm tra trên jwt.io |
| 3.6 | Refresh Token & Token Revocation · Demo + Lab · 30' | So với refresh token tự chế ở khóa nền tảng: OpenIddict revoke tập trung. | Refresh token hoạt động, revoke khi logout |
| 3.7 | Policy-based Authorization · Demo + Lab · 30' | Policy Admin/Instructor/Student dựa trên claims 3.5. | `[Authorize(Policy = "InstructorOnly")]` đúng trên endpoint Course |
| 3.8 | Resource-based Authorization · Demo + Lab · 45' | Instructor chỉ sửa Course của mình — thay check tạm ở 1.2 bằng `IAuthorizationHandler`. | `CourseOwnerRequirement` + Handler, `UpdateCourseCommand` chặn đúng |
| 3.9 | CORS & Bảo mật API cho Multi-client · Demo + Lab · 30' | CORS theo từng origin cho Admin và Next.js, HTTPS, security headers. | CORS đúng, test từ 2 origin |

### Module 4 — API hoàn thiện & OpenAPI-first

Mục tiêu: chuẩn hóa OpenAPI schema trước khi generate client ở Module 5.

| Bài | Tên bài · Loại · Thời lượng | Nội dung chính | Deliverable |
|---|---|---|---|
| 4.1 | .NET 10 OpenAPI built-in · Demo · 30' | Annotate endpoint (WithName, Produces, tag) để schema đúng kiểu — quyết định chất lượng TypeScript ở Module 5. | `openapi.json` đầy đủ, đúng kiểu |
| 4.2 | Scalar UI thay Swagger · Demo · 20' | Cấu hình Scalar, test OAuth2 flow ngay trên UI. | `/scalar` test được API có auth |
| 4.3 | API Versioning cho hệ thống nhiều client · Demo · 20' | Versioning theo URL segment `/api/v1/...` cho 2 frontend sống lâu dài. | Toàn bộ endpoint dưới `/api/v1` |
| 4.4 | Rate Limiting .NET 10 · Demo + Lab · 30' | Bảo vệ Checkout/Payment khỏi spam. | Rate limit policy cho nhóm endpoint payment |
| 4.5 | Chuẩn hóa response & error format toàn hệ thống · Demo · 30' | Review endpoint Module 0–3, response envelope và ProblemDetails nhất quán 100%. | Checklist review xong, schema sẵn sàng generate |

### Module 5 — Monorepo & API Client Codegen

| Bài | Tên bài · Loại · Thời lượng | Nội dung chính | Deliverable |
|---|---|---|---|
| 5.1 | Setup Turborepo Monorepo · Demo + Lab · 45' | `apps/` (admin, web) + `packages/` (api-client), pnpm workspace, pipeline `turbo.json`. | Monorepo `tedu-market-frontend`, `pnpm install` sạch |
| 5.2 | Generate API Client bằng @hey-api/openapi-ts · Demo + Lab · 45' | Đọc `openapi.json`, sinh type-safe client + React Query hooks; giải thích cấu trúc output. | `packages/api-client` build thành công |
| 5.3 | Tự động hóa Regenerate Client · Demo · 20' | Script `generate:client` khi backend đổi API. | `pnpm generate:client` cập nhật toàn bộ client |
| 5.4 | Demo Type-safe API Call end-to-end · Demo · 20' | Đổi field `CourseDto` → generate lại → lỗi compile ở cả admin và web. | Video demo lỗi compile xuất hiện đồng thời ở 2 app |

### Module 6 — React Admin

| Bài | Tên bài · Loại · Thời lượng | Nội dung chính | Deliverable |
|---|---|---|---|
| 6.1 | Setup React (Vite) trong Monorepo · Demo + Lab · 30' | `apps/admin` bằng Vite, import `api-client` như workspace dependency. | `pnpm dev` chạy, gọi 1 API thành công (chưa auth) |
| 6.2 | Auth OIDC cho SPA · Demo + Lab · 60' | `react-oidc-context` + OpenIddict (Code + PKCE từ 3.3), silent renew, protected route. | Đăng nhập Admin end-to-end chuẩn OIDC |
| 6.3 | CRUD UI: Category, Course, Lesson · Demo + Lab · 60' | React Query + hooks từ api-client; form dùng error shape ProblemDetails (4.5). | Giao diện quản lý Category/Course/Lesson |
| 6.4 | Quản lý Order & Dashboard Doanh thu · Demo + Lab · 45' | Dùng lại query 2.8, biểu đồ doanh thu cơ bản. | Trang Order list + dashboard doanh thu |
| 6.5 | Upload ảnh Course từ Admin UI · Demo + Lab · 30' | Presigned URL (1.8), upload thẳng MinIO từ browser. | Upload ảnh từ UI thật |
| 6.6 | Xử lý lỗi & Loading State chuẩn · Demo · 30' | Parse ProblemDetails thành toast thống nhất; skeleton với React Query. | UX lỗi/loading nhất quán toàn app |

### Module 7 — Next.js Student Site

| Bài | Tên bài · Loại · Thời lượng | Nội dung chính | Deliverable |
|---|---|---|---|
| 7.1 | Setup Next.js App Router trong Monorepo · Demo + Lab · 30' | `apps/web` dùng chung api-client; gọi API ở Server Component vs Client Component. | `apps/web` gọi API từ Server Component |
| 7.2 | Trang danh sách khóa học (SEO) · Demo + Lab · 45' | ISR cho listing, dùng `GetCoursesQuery` (1.6). | `/courses` SSR, metadata SEO cơ bản |
| 7.3 | Trang chi tiết khóa học · Demo + Lab · 30' | SSR + `generateMetadata` theo Course, dùng `GetCourseDetailQuery` (1.7). | `/courses/[slug]` SEO đúng từng Course |
| 7.4 | Auth OIDC cho Next.js · Demo + Lab · 60' | Session phía server (route handler), không lưu token ở localStorage; so trade-off với SPA (6.2). | Đăng nhập Student, session an toàn phía server |
| 7.5 | Giỏ hàng & Checkout Flow · Demo + Lab · 60' | Client Component giỏ hàng, Server Action gọi `CheckoutCommand` (2.2), chuyển sang bước thanh toán (2.4). | Luồng mua khóa học hoàn chỉnh từ giỏ đến thanh toán |
| 7.6 | Trang "Khóa học của tôi" · Demo + Lab · 30' | Route protected theo Enrollment tạo ở 2.7. | `/my-courses` đúng danh sách đã mua |
| 7.7 | Xem bài học (Lesson Player) · Demo + Lab · 30' | Kiểm tra Enrollment ở API trước khi trả nội dung Lesson, không chỉ ẩn UI. | Chặn truy cập trái phép kể cả gọi thẳng API |

### Module 8 — Testing

| Bài | Tên bài · Loại · Thời lượng | Nội dung chính | Deliverable |
|---|---|---|---|
| 8.1 | Unit Test CQRS Handler · Demo + Lab · 45' | Mở rộng 1.10, test Handler Course, Order chỉ cần mock Repository. | Unit test Handler chính, chạy trong CI |
| 8.2 | Unit Test Domain Logic · Demo + Lab · 30' | Test invariant Aggregate và Value Object thuần (1.1, 2.3). | Unit test Domain chạy mili-giây |
| 8.3 | Integration Test với WebApplicationFactory + Testcontainers · Demo + Lab · 60' | Postgres container thật, phát hiện lỗi migration/SQL mà mock không thấy. | Integration test CRUD Course và Checkout |
| 8.4 | Test luồng Payment Webhook · Demo + Lab · 30' | Mock cổng thanh toán, test idempotency webhook (2.5): gọi 2 lần, Order chỉ Paid 1 lần. | Test race condition webhook, Outbox/Enrollment không trùng |

### Module 9 — Production

| Bài | Tên bài · Loại · Thời lượng | Nội dung chính | Deliverable |
|---|---|---|---|
| 9.1 | Dockerize Backend · Demo + Lab · 30' | Multi-stage build .NET 10, tối ưu image. | Dockerfile backend production |
| 9.2 | Dockerize Frontend · Demo + Lab · 30' | React → static file Nginx; Next.js standalone output. | Dockerfile `apps/admin`, `apps/web` |
| 9.3 | Docker Compose Full-stack · Demo + Lab · 30' | API + Admin + Web + Postgres + Redis + Seq trong `docker-compose.prod.yml`. | `docker compose -f docker-compose.prod.yml up` chạy toàn hệ thống |
| 9.4 | Nginx Reverse Proxy Multi-app · Demo + Lab · 45' | Route theo subdomain api/admin/www. | Nginx route đúng 3 subdomain |
| 9.5 | SSL Let's Encrypt cho Multi-domain · Demo + Lab · 30' | Certbot cho 3 subdomain, auto-renew. | HTTPS trên cả 3 subdomain |
| 9.6 | CI/CD GitHub Actions · Demo + Lab · 60' | Build & deploy 3 service, bắt buộc pass bộ test Module 8 trước deploy. | Workflow test → build → deploy khi merge main |
| 9.7 | Serilog + Seq Production Logging · Demo · 30' | Structured logging, query log khi có sự cố. | Log tập trung, query theo CorrelationId |
| 9.8 | Checklist Go-live & Rollback Plan · Lý thuyết + Demo · 30' | Backup DB, kiểm tra webhook thanh toán production, DNS; kế hoạch rollback. | Checklist go-live học viên dùng lại được |

### Phụ lục — Tái sử dụng nội dung từ khóa "Xây dựng website học trực tuyến với ASP.NET Core"

Tham khảo khi viết bài, không sao chép nguyên văn: kiến trúc đã đổi nên phần lớn code mẫu phải viết lại.

| Module khóa cũ | Mức dùng lại |
|---|---|
| Module 1, 3 (EF Core) | ~70% giải thích khái niệm (Migration, Tracking, Relationships). Code viết lại theo Repository, không lộ DbContext |
| Module 4 (DTO/Validation/Mapping) | ~60%. FluentValidation chuyển thành MediatR Pipeline Behavior |
| Module 5 (Auth) | Chỉ lý thuyết OAuth2/OIDC (bài 49 cũ) và Authorization cơ bản (bài 46 cũ). Không dùng lại code JWT tự chế |
| Module 6 (Production) | ~50% (Nginx, Serilog/Seq), mở rộng sang multi-app |

## Ghi chú sản xuất

- Mục tiêu: mở bán cuối tháng 11/2026 ([plans/2026-Q4.md](../../../plans/2026-Q4.md): phát hành trước 30/11).
- **Điểm chưa nhất quán trong outline gốc, cần Toan chốt trước khi soạn bài:**
  1. **Cổng thanh toán:** bài 2.4, 2.5 đã đổi sang **SePay** (QR VietQR + webhook), nhưng mục Bounded context, cấu trúc solution (`VNPay adapter`), bài 7.5 ("redirect sang VNPay sandbox"), 8.4 ("mock VNPay, IPN handler") và 9.8 ("webhook VNPay") vẫn ghi VNPay. Trong file này đã viết trung tính ở 7.5, 8.4, 9.8.
  2. **Hangfire hay Hosted Service:** bài 2.7 đổi sang Hosted Service nhưng vẫn còn câu "PostgreSQL làm storage cho Hangfire"; cấu trúc solution, 3.1, 3.4, 7.6 vẫn nhắc Hangfire. Đã ghi "background job" trong file này.
  3. **MediatR:** từ v13 (2025) MediatR chuyển sang license thương mại (miễn phí có điều kiện). Nên ghi chú cho học viên hoặc cân nhắc tự viết mediator đơn giản / thư viện thay thế. (Giả định của AI, cần kiểm tra lại điều khoản license hiện tại.)
  4. **Phiên bản:** outline ghi PostgreSQL 17 trong khi TEDU-68 dạy PostgreSQL 18 (cùng combo); Next.js 15 trong khi Next.js 16 đã phát hành (10/2025). Nên thống nhất PG 18 và cân nhắc Next.js 16.
  5. **Yêu cầu đầu vào** nhắc khóa "Xây dựng website học trực tuyến với ASP.NET Core" — trùng tên cũ/slug URL của chính TEDU-67. Cần ghi rõ đó là khóa cũ nào, hoặc đổi thành "TEDU-66 hoặc tương đương" cho khớp combo.
