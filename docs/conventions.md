# Conventions — TEDU Market

Áp dụng cho stack trong [architecture.md](architecture.md) §2. Tên nghiệp vụ theo [domain-glossary.md](domain-glossary.md). Mã thẻ dạng `T123`. Quy tắc nào ghi "CI" là máy chặn; "Review" là tự soát bằng [review-checklist.md](review-checklist.md).

## 1. Naming

### Chung (backend và frontend)

**Chuyển tên từ cột Code của glossary** (Code luôn là PascalCase, tiếng Anh, số ít):

| Dùng cho | Quy tắc | `RevenueShare` → |
|---|---|---|
| Class, type, enum, component | giữ nguyên Code | `RevenueShare` |
| Biến, tham số, property JSON | camelCase | `revenueShare` |
| Bảng DB | snake_case, số nhiều | `revenue_shares` |
| Cột DB | snake_case, số ít; khóa ngoại `<entity>_id` | `revenue_share_id` |
| URL endpoint | kebab-case, số nhiều | `/api/v1/revenue-shares` |
| File frontend | kebab-case | `revenue-share.ts` |
| Giá trị enum lưu DB và JSON | đúng Code của trạng thái, lưu dạng chuỗi | `"PendingShare"` |

Ngoại lệ duy nhất: header `X-API-Token` giữ nguyên. Tên kỹ thuật nội bộ được phép: `User`, `CartItem`, `OrderItem`, `OutboxMessage`, `Identity`, `Notification` (architecture §3, §6). Tên khác chưa có trong glossary thì thêm vào glossary trước khi dùng. Cấm dùng từ ở cột "KHÔNG gọi là" làm tên trong code.

| Loại | Quy tắc | Đúng | Sai |
|---|---|---|---|
| Bảng DB | `snake_case` số nhiều | `order_items` | `OrderItem`, `tblOrder` |
| Cột DB | `snake_case`, thời điểm kết thúc `_at`, UTC `timestamptz`, tiền `numeric(18,0)` | `expires_at`, `final_price` | `ExpiresAt`, `expiry`, `finalPrice` |
| Index | `ix_<bảng>_<cột>`, unique `ux_<bảng>_<cột>` | `ux_enrollments_student_id_course_id` | `idx1` |
| API endpoint | `/api/v1/<danh-từ-số-nhiều>`; hành động không phải CRUD là danh từ con | `POST /api/v1/orders/{orderId}/resolution` | `POST /api/v1/resolveOrder`, `/api/v1/Order` |
| Query string, JSON | camelCase | `?categoryId=...&pageSize=20` | `?category_id=` |
| `errorCode` | `UPPER_SNAKE_CASE` | `AMOUNT_MISMATCH` | `amountMismatch` |
| Nhánh git | `<feature\|fix\|chore\|docs\|refactor>/<mã thẻ>-<slug không dấu, kebab-case>` | `feature/T123-dang-ky-hoc-vien` | `feature/dang-ky-hoc-vien`, `T123` |
| Commit | `<mã thẻ>: <động từ + việc, tiếng Việt, ≤ 72 ký tự>`; 1 commit 1 mã thẻ | `T123: thêm API đăng ký học viên` | `fix bug`, `update code` |

CI: hook `commit-msg` chặn nếu không khớp `^T\d+: .{5,72}$`; CI chặn tên nhánh không khớp `^(feature|fix|chore|docs|refactor)/T\d+(-[a-z0-9]+)+$`. Hook nằm ở `.githooks/`, tự bật khi chạy `pnpm install` (script `prepare` đặt `core.hooksPath`); workflow ở `.github/workflows/ci.yml`.

**Viết tắt:** chỉ được dùng danh sách đóng `Id, Url, Dto, Api, Db, Qr, Hls, Json, Http, Oidc`. Tên khác phải viết đủ chữ. Ngoại lệ: biến vòng lặp `i`, tham số lambda một chữ cái trong lambda ≤ 1 dòng.

### Backend (.NET 10, C#)

| Loại | Quy tắc | Đúng | Sai |
|---|---|---|---|
| File | trùng tên type, 1 type công khai / file | `OrderResolution.cs` | `order_resolution.cs`, `Models.cs` |
| Class, record | PascalCase, danh từ | `PaymentHook` | `PaymentHookManager`, `paymentHook` |
| Command / Query / Handler / Dto | `<ĐộngTừ><Danh từ>Command`, `Get<…>Query`, hậu tố `Handler`, `Dto` | `CreatePayoutCommand`, `CreatePayoutCommandHandler`, `OrderHistoryDto` | `PayoutCreator`, `OrderHistoryModel` |
| Interface | tiền tố `I` | `IOrderRepository` | `OrderRepositoryInterface` |
| Domain event | danh từ + động từ quá khứ | `OrderPaidEvent` | `PayOrderEvent` |
| Method | PascalCase động từ; async có hậu tố `Async`, `CancellationToken` ở cuối | `MarkAsPaidAsync(ct)` | `markAsPaid()`, `PaidAsync()` |
| Biến cục bộ, tham số | camelCase, danh từ; bool bắt đầu `is`/`has`/`can` | `finalPrice`, `isExpired` | `fp`, `expired` |
| Field private | `_camelCase` | `_clock` | `clock_`, `m_clock` |
| Hằng | PascalCase | `MaxVideoSizeBytes` | `MAX_VIDEO_SIZE` |
| Test | xem §5 | | |

### Frontend (TypeScript, React, Next.js 16)

| Loại | Quy tắc | Đúng | Sai |
|---|---|---|---|
| File | kebab-case; test `.test.ts(x)` | `order-history.tsx` | `OrderHistory.tsx`, `orderHistory.tsx` |
| Component, type | PascalCase, không tiền tố `I`, không hậu tố `Type` | `OrderHistory`, `OrderStatus` | `IOrder`, `OrderType` |
| Hook | `use` + PascalCase, file `use-<tên>.ts` | `useOrderStatus` | `orderStatusHook` |
| Hàm, biến | camelCase; hàm là động từ; bool `is`/`has`/`can` | `calculateFinalPrice`, `hasEnrollment` | `calc`, `enrolled` |
| Hằng cấp module | `UPPER_SNAKE_CASE` | `ORDER_POLL_INTERVAL_MS` | `orderPollInterval` |
| Thư mục route | đúng URL trong ux.md | `app/my-courses/` | `app/MyCourses/` |
| Biến môi trường | `NEXT_PUBLIC_` chỉ cho giá trị không bí mật | `NEXT_PUBLIC_API_URL` | `NEXT_PUBLIC_RESEND_KEY` |

Chữ hiển thị trên UI dùng cột "Hiển thị" của glossary, không dùng tên Code (`Đơn`, không phải `Order`).

## 2. Cấu trúc thư mục

```
backend/
  TeduMarket.slnx
  src/
    TeduMarket.Domain/<Module>/           Aggregate, Entity, ValueObject, enum trạng thái, domain event, interface repository
    TeduMarket.Application/<Module>/
      Commands/<ĐộngTừDanhTừ>/            <..>Command, <..>CommandHandler, <..>CommandValidator   (ghi, qua EF Core + Aggregate)
      Queries/<GetDanhTừ>/                <..>Query, <..>QueryHandler, <..>Dto                    (đọc, qua Dapper, SQL tham số)
    TeduMarket.Infrastructure/<Module>/   EF Configuration, Repository, Dapper reader, adapter Resend/MinIO/ffmpeg, BackgroundService
    TeduMarket.Infrastructure/Persistence/Migrations/
    TeduMarket.Api/<Module>/              <Module>Endpoints.cs (map HTTP ↔ Mediator, policy), Program.cs, middleware
  tests/
    TeduMarket.Domain.Tests/  TeduMarket.Application.Tests/  TeduMarket.Api.IntegrationTests/  TeduMarket.Architecture.Tests/
apps/
  student-web/src/  app/<route theo ux.md>/   features/<module>/{components,hooks}/   lib/
  admin-app/src/    routes/                   features/<module>/{components,hooks}/   lib/
packages/
  api-client/       sinh bởi @hey-api/openapi-ts, không sửa tay
  ui/               Tailwind + shadcn dùng chung, token theo ux.md §4
e2e/                Playwright
```

- `<Module>` là đúng 11 module architecture §3: `Course, Instructor, Video, Cart, Coupon, Order, Payment, Enrollment, Revenue, Identity, Notification`. Cùng tên thư mục ở cả 4 project backend; `features/<module>` ở frontend dùng kebab-case (`features/course`). Mã dùng chung nhiều module đặt ở `Common/` của từng project, không đặt ở module.
- Hướng phụ thuộc: Domain không tham chiếu gì; Application → Domain; Infrastructure → Application; Api → tất cả. Module gọi module khác chỉ qua Mediator hoặc interface trong Application của module đó (đúng cột "Phụ thuộc" ở architecture §3). CI: `TeduMarket.Architecture.Tests` (NetArchTest) fail khi vi phạm.
- Frontend: `features/*` không import chéo `features` khác; chỉ import `packages/*` và `lib/`. CI: `eslint-plugin-boundaries`.
- Migration: tạo bằng `dotnet ef migrations add`, đặt tên `<NămThángNgày>_<TenViecPascalCase>`, đã merge thì không sửa.

## 3. Coding style

Mọi quy tắc dưới đây do công cụ chặn ở CI; không có quy tắc style nào chỉ ghi bằng lời.

### Backend

`Directory.Build.props`:
```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <AnalysisLevel>latest-recommended</AnalysisLevel>
  <GenerateDocumentationFile>true</GenerateDocumentationFile> <!-- IDE0005 chỉ chạy khi build nếu bật -->
  <NoWarn>$(NoWarn);CS1591</NoWarn> <!-- không bắt buộc XML doc -->
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Microsoft.CodeAnalysis.BannedApiAnalyzers" PrivateAssets="all" /> <!-- version khai báo ở Directory.Packages.props (Central Package Management) -->
  <AdditionalFiles Include="$(MSBuildThisFileDirectory)BannedSymbols.txt" />
</ItemGroup>
```
`BannedSymbols.txt` (lỗi RS0030):
```
T:System.Console;Dùng ILogger
P:System.DateTime.Now;Dùng TimeProvider
P:System.DateTime.UtcNow;Dùng TimeProvider
P:System.Threading.Tasks.Task`1.Result;Dùng await
M:System.Threading.Tasks.Task.Wait();Dùng await
N:MediatR;Dùng Mediator (martinothamar)
```
`.editorconfig`:
```ini
root = true
[*.cs]
indent_style = space
indent_size = 4
end_of_line = lf
csharp_style_namespace_declarations = file_scoped:error
csharp_style_var_elsewhere = true:error
dotnet_sort_system_directives_first = true
dotnet_diagnostic.IDE0005.severity = error   # using thừa
dotnet_diagnostic.CA1031.severity = error    # cấm catch Exception chung
dotnet_diagnostic.CA2007.severity = none
dotnet_naming_rule.private_field.severity = error
dotnet_naming_rule.private_field.symbols = private_fields
dotnet_naming_rule.private_field.style = underscore_camel
dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private
dotnet_naming_style.underscore_camel.required_prefix = _
dotnet_naming_style.underscore_camel.capitalization = camel_case
dotnet_naming_rule.async_suffix.severity = error
dotnet_naming_rule.async_suffix.symbols = async_methods
dotnet_naming_rule.async_suffix.style = end_with_async
dotnet_naming_symbols.async_methods.applicable_kinds = method
dotnet_naming_symbols.async_methods.required_modifiers = async
dotnet_naming_style.end_with_async.required_suffix = Async
dotnet_naming_style.end_with_async.capitalization = pascal_case
```
CI: `dotnet format --verify-no-changes` và `dotnet build -warnaserror`.

### Frontend

`tsconfig.base.json`: `"strict": true, "noUncheckedIndexedAccess": true, "noImplicitOverride": true, "verbatimModuleSyntax": true`. `.prettierrc`: `{ "printWidth": 100, "singleQuote": true, "trailingComma": "all", "semi": true }`. `eslint.config.js` (flat config, `typescript-eslint` strict + `next/core-web-vitals` + `jsx-a11y`):
```js
rules: {
  'no-console': 'error',
  'no-debugger': 'error',
  'eqeqeq': 'error',
  '@typescript-eslint/no-explicit-any': 'error',
  '@typescript-eslint/ban-ts-comment': ['error', { 'ts-ignore': true, 'ts-expect-error': 'allow-with-description' }],
  '@typescript-eslint/no-floating-promises': 'error',
  '@typescript-eslint/naming-convention': ['error',
    { selector: 'typeLike', format: ['PascalCase'] },
    { selector: 'variable', types: ['boolean'], format: ['PascalCase'], prefix: ['is', 'has', 'can'] }],
  'no-restricted-globals': ['error', 'localStorage', 'sessionStorage'],
  'no-restricted-imports': ['error', { paths: ['axios', 'pg', 'dapper'], patterns: ['**/features/*/**'] }],
  'react/no-danger': 'error',
  'check-file/filename-naming-convention': ['error', { '**/*.{ts,tsx}': 'KEBAB_CASE' }],
  'no-warning-comments': ['error', { terms: ['todo', 'fixme'], location: 'start' }], // TODO phải có mã thẻ: dùng // TODO(T123)
}
```
CI: `pnpm lint --max-warnings 0`, `pnpm prettier --check .`, `pnpm tsc --noEmit`.

TODO hợp lệ: `// TODO(T123): …`; CI quét `TODO|FIXME` không có `(T\d+)` thì fail (`git grep -nE 'TODO|FIXME' | grep -vE '\(T[0-9]+\)'`).

## 4. Error handling

### Backend

- **Throw:** vi phạm quy tắc nghiệp vụ → `DomainException(errorCode, message)` (Domain/Application). Không có dữ liệu → `NotFoundException`; sai quyền sở hữu hoặc role → `ForbiddenException`; dữ liệu vào sai → `<Command>Validator` chạy trong pipeline Mediator, gom lỗi theo trường (thư viện validate chưa chốt trong architecture §2; thêm thì ghi decision log). Không throw `Exception`/`ApplicationException` trần. Không dùng exception cho luồng bình thường.
- **Catch:** chỉ ở 3 chỗ: (1) `IExceptionHandler` toàn cục ở Api; (2) vòng lặp xử lý từng `OutboxMessage`/job ffmpeg trong `BackgroundService`, để 1 message lỗi không dừng worker; (3) adapter Infrastructure bọc lỗi thư viện ngoài thành exception của mình, giữ `InnerException`. Chỗ khác không catch. Mỗi catch phải log `LogError(ex, …)` kèm CorrelationId hoặc rethrow. `catch` rỗng, `catch { return null; }` bị cấm (CA1031 + Review).
- **Format lỗi API** (RFC 9457 ProblemDetails, `Content-Type: application/problem+json`):
  ```json
  { "type": "...", "title": "Dữ liệu không hợp lệ", "status": 400, "errorCode": "INVALID_REQUEST",
    "errors": { "title": ["Không được để trống"] }, "correlationId": "..." }
  ```
  `errors` là map `tên trường camelCase → mảng thông điệp` (FR-26, ux §5 gắn lỗi vào ô). Mã trạng thái: 400 dữ liệu sai/vi phạm nghiệp vụ, 401 chưa đăng nhập, 403 sai quyền, 404 không có, 409 xung đột (unique, điều kiện UPDATE không khớp), 429 rate limit, 500 lỗi không lường. 500 chỉ trả `errorCode: INTERNAL_ERROR` + `correlationId`, không stack, không thông điệp exception.
- **`PaymentHook`** theo architecture §3 Payment: body lỗi `{ "errorCode": "...", "errorMessage": "..." }` với 200/400/500 và bộ `errorCode` đã liệt kê ở đó; không dùng ProblemDetails cho endpoint này.
- **Job nền:** message lỗi tăng `attempt_count`, retry backoff; quá 5 lần chuyển `Failed` và log mức Error. Chỉ đặt `processed_at` sau khi xử lý thành công. Không fire-and-forget `Task` (không `_ = DoAsync()`).

### Frontend

- Mọi lời gọi API đi qua `packages/api-client`; lỗi typed theo ProblemDetails. Lỗi 400 có `errors` → gắn vào đúng ô form; lỗi khác không gắn được → toast (ux §5). Lỗi Thanh toán và trạng thái Đơn → khối trong trang.
- Thông điệp UI tiếng Việt tra từ `lib/error-messages.ts` theo `errorCode`; chưa có mã → câu chung "Có lỗi xảy ra, vui lòng thử lại". Không hiển thị `errorMessage` thô, stack, hoặc `errorCode`.
- Mỗi route Next có `error.tsx`; AdminApp có error boundary ở root. `catch` chỉ để: hiển thị lỗi cho người dùng, hoặc rethrow. `.catch(() => {})` và `catch {}` rỗng bị cấm (`no-empty` + `no-floating-promises`).

## 5. Testing

**Đặt tên**
- Backend: class `<TypeUnderTest>Tests`; method `<Method>_<Condition>_<Expected>`, ví dụ `Handle_OrderAlreadyPaid_ReturnsOkWithoutChange`; test `async` thêm hậu tố `Async` như mọi method (§1): `Handle_OrderAlreadyPaid_ReturnsOkWithoutChangeAsync`. Sai: `Test1`, `ShouldWork`.
- Frontend: `describe('<Unit>')` + `it('<kết quả> when <điều kiện>')`, ví dụ `it('shows field error when server returns 400')`.
- Integration/e2e: 1 file / 1 luồng, tên theo luồng ux.md §3.

**Arrange-Act-Assert:** mỗi test có 3 khối ngăn bằng dòng comment `// Arrange`, `// Act`, `// Assert` (hoặc dòng trống trong TS); đúng 1 Act; không `if`/vòng lặp trong test; không test nào không có assert.

**Mock**
- Chỉ mock biên ngoài: Resend, MinIO, Google OIDC, tiến trình ffmpeg, thời gian (`FakeTimeProvider`), HTTP của `api-client` ở frontend.
- Không mock: Domain, Handler, EF Core/Dapper, PostgreSQL (integration dùng Testcontainers PostgreSQL 18 thật). Dùng 1 thư viện mock duy nhất toàn solution.
- Unit test Domain không cần mock.

**Mức test tối thiểu** (tương xứng NFR-04, 05, 16, 21; ngưỡng ≥ 90% cho Order/Payment/Enrollment là PO đề xuất thêm so với NFR-16):

| Loại | Bắt buộc có | Ngưỡng | Chặn |
|---|---|---|---|
| Unit backend | Mọi Aggregate, Handler, Validator mới/đổi: ≥ 1 test happy path + 1 test mỗi nhánh lỗi | Domain + Application: line ≥ 70% (NFR-16); `Order`, `Payment`, `Enrollment`, `Revenue`: line ≥ 90%, branch ≥ 80% | CI fail nếu thấp hơn |
| Integration backend (PostgreSQL thật) | Checkout; CRUD Course; `PaymentHook` gọi trùng và ≥ 10 lời gọi song song → đúng 1 `Paid`, 1 `Enrollment`, 1 `RevenueShare`; Paid + `OutboxMessage` cùng transaction (lỗi giữa chừng → rollback cả hai); Outbox → `Enrollment` idempotent; Video: không có `Enrollment` → 403, `PreviewLesson` → 200; ownership Course/Order (IDOR) | Mỗi endpoint mới: 1 test thành công + 1 test 401/403 | CI fail trước deploy |
| Unit frontend (Vitest + Testing Library) | Hàm trong `lib/`, hook, form có lỗi 400 theo trường | `lib/` và `hooks/`: line ≥ 70% | CI fail |
| E2E (Playwright, `e2e/`) | Mỗi luồng chính ux.md §3 ≥ 1 test; bắt buộc: Học viên Checkout → gọi `PaymentHook` giả → `Vào học` → phát Video; Instructor tạo khóa → `Gửi duyệt` → Admin duyệt; Khách xem `PreviewLesson` | 100% luồng chính chạy pass | Chạy trên staging trước deploy production |

**Sửa bug:** bắt buộc có test tái hiện bug, viết trước, chạy thấy FAIL khi chưa sửa, PASS sau khi sửa, đặt cùng commit với bản sửa; tên test ghi mã thẻ (`Handle_..._Regression_T123`, `it('... (T123)')`). Test lỗi thanh toán/Enrollment nằm ở tầng integration.

**Cấm trong test:** `[Fact(Skip=…)]`, `it.skip`, `it.only`, `Thread.Sleep`/`setTimeout` chờ cố định (dùng `FakeTimeProvider`, `waitFor`), phụ thuộc thứ tự chạy, dữ liệu dùng chung giữa test. CI: ESLint `jest/no-focused-tests`, `jest/no-disabled-tests`; analyzer xUnit `xUnit1004` = error.

## 6. Điều cấm tuyệt đối

| # | Cấm | Chặn bởi |
|---|---|---|
| 6.1 | Business logic trong Endpoint/Controller hoặc component UI. Api chỉ map HTTP ↔ Mediator; quy tắc nghiệp vụ ở Domain/Handler | Architecture.Tests, Review |
| 6.2 | Domain/Application tham chiếu EF Core, Dapper, Infrastructure, Api; đảo chiều phụ thuộc layer | Architecture.Tests |
| 6.3 | Handler/phía ghi truy cập bảng, `DbContext` entity của module khác hoặc join bảng chéo module | Architecture.Tests, Review |
| 6.4 | Nối chuỗi SQL (`$"SELECT … {x}"`, `+`) với Dapper; phải dùng tham số | Review, analyzer CA2100 |
| 6.5 | Frontend truy cập DB/MinIO bằng khóa bí mật, hoặc gọi backend không qua `packages/api-client`, hoặc sửa tay file trong `packages/api-client` | ESLint `no-restricted-imports`, Review |
| 6.6 | Lưu token ở `localStorage`/`sessionStorage` (NFR-06) | ESLint `no-restricted-globals` |
| 6.7 | Hardcode secret: `X-API-Token`, khóa Resend/MinIO/Google, mật khẩu seed, connection string có mật khẩu; commit file `secret.json` (NFR-22) | gitleaks CI, Review |
| 6.8 | Chỉ ẩn nút trên UI mà endpoint thiếu policy role và kiểm tra sở hữu trong Handler (NFR-08) | Integration test, Review |
| 6.9 | Chuyển `Order` sang `Paid` bằng đọc-rồi-ghi (không UPDATE có điều kiện); ghi `Paid` khác transaction với `OutboxMessage`; tạo `Enrollment` ngoài Outbox handler (NFR-04, 05) | Integration test, Review |
| 6.10 | Nuốt lỗi: `catch` rỗng, trả 200 khi thất bại, `.catch(() => {})`, `Task` fire-and-forget | CA1031, ESLint, Review |
| 6.11 | Trả stack trace, thông điệp exception hoặc ID nội bộ ra API; ghi log token, mật khẩu | Review |
| 6.12 | Phục vụ file Video bằng URL tĩnh không qua kiểm tra token; gọi ffmpeg bằng chuỗi lệnh shell (phải dùng danh sách tham số) | Integration test, Review |
| 6.13 | Tên nghiệp vụ khác glossary hoặc dùng từ cột "KHÔNG gọi là" (trong code và chữ UI); tên mới không có trong glossary | Review |
| 6.14 | Dùng `float`/`double` cho tiền (dùng `decimal`/`numeric(18,0)`) | Review |
| 6.15 | Sửa schema DB bằng tay hoặc sửa migration đã merge | Review |
| 6.16 | Thêm thư viện ngoài Tech Stack (kể cả MediatR) mà không có decision log | Banned API, Review |
| 6.17 | `any`, `@ts-ignore`, `dangerouslySetInnerHTML` không sanitize; `console.log`/`Console.WriteLine` debug | ESLint, Banned API |
| 6.18 | Xóa/skip test hoặc nới ngưỡng coverage để CI pass | Review |
