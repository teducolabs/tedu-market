# PRD — TEDU Market

Nguồn: input yêu cầu dự án, câu trả lời của chủ dự án (2026-11-11) và [outline TEDU-67](course/tedu-67.md) (phạm vi MVP chốt "theo tedu-67.md"; chỗ nào câu trả lời khác outline thì theo câu trả lời, xem [decision logs](decision-logs/)). Chỗ chưa có thông tin đánh `[CẦN LÀM RÕ → Q-xx]` và gom ở mục 7. Số liệu NFR do PO đề xuất "theo chuẩn ngành" theo ủy quyền của chủ dự án, chưa đối chiếu nhu cầu thực.

## 1. Context & Problem Statement

TEDU Market là website bán khóa học trực tuyến. Nhiều người có nội dung để dạy (các Instructor độc lập) nhưng không có nơi để đăng bán cho người học online. Người học thì cần một chỗ tìm khóa học, mua và học ngay.

Trọng tâm là hành trình thương mại: duyệt khóa học → giỏ hàng → thanh toán → được cấp quyền truy cập. Đây không phải LMS quản lý học tập nội bộ.

Phần lõi dễ sai nhất là thanh toán và cấp quyền học. Đơn đã trả tiền mà không có quyền học, hoặc hook xác nhận gọi trùng làm đơn xử lý hai lần, là lỗi nghiêm trọng nhất; các FR/NFR về Order, Payment, Enrollment chặn các lỗi này.

Hậu quả nếu không giải quyết: Instructor không có kênh bán, người học khó tìm được khóa học (mức độ cụ thể: chưa có số liệu).

Giai đoạn này không tích hợp cổng thanh toán thật: hệ thống chỉ hiển thị mã QR, việc kích hoạt đơn dùng hook gọi thử (xem FR-10, FR-12).

## 2. Đối tượng người dùng

| Persona | Hành vi chính | Am hiểu kỹ thuật |
|---|---|---|
| Student (người học) | Muốn học lập trình hoặc kỹ năng khác. Tìm và duyệt khóa học, xem thử, thêm giỏ, áp coupon, quét QR chuyển khoản, vào xem video; đăng nhập bằng tài khoản Google. | Kỹ năng máy tính cơ bản |
| Instructor (người dạy, nhiều người độc lập) | Có nội dung video. Đăng ký như Student, được Admin duyệt thành Instructor; cập nhật hồ sơ, tạo khóa học và bài học của mình, gửi duyệt để đăng, xem doanh thu và phần chia của mình. | Kỹ năng máy tính cơ bản; đào tạo ở mảng ngoài lập trình, không phải developer |
| Admin | Duyệt Instructor và khóa học, sửa mọi khóa, quản lý Category/Coupon, xử lý đơn, duyệt phần chia và tạo phiếu chi cho Instructor, xem đơn và doanh thu. Tài khoản đầu tiên tạo bằng seeding. | Chuyên gia |

Ba vai trò lấy từ policy authorization trong outline (3.7) và câu trả lời của chủ dự án.

## 3. Phạm vi

| Trong phạm vi (MVP) | Ngoài phạm vi |
|---|---|
| Web Student (Next.js, SEO): danh sách, tìm theo từ khóa, chi tiết, xem thử, giỏ hàng, coupon, checkout, QR thanh toán, "Khóa học của tôi", lịch sử đơn, xem video | Đánh giá, bình luận |
| App quản trị (React) cho Admin/Instructor: Category, Course, Lesson, Instructor, Coupon, Order, duyệt khóa, dashboard doanh thu, upload ảnh và video | Hoàn tiền |
| Đăng nhập OAuth2/OIDC: Student và Instructor qua Google SSO; Admin đầu tiên tạo bằng seeding, có đổi mật khẩu | Ứng dụng mobile |
| Thanh toán: QR VietQR, hook (X-API-Token) kích hoạt đơn, đơn bất thường chờ Admin xử lý | Đa ngôn ngữ; chứng chỉ; đa tiền tệ |
| Chia doanh thu: ghi nhận theo tỷ lệ, Admin duyệt, Admin tạo phiếu chi để kế toán chuyển khoản | Tích hợp cổng thanh toán thật (hook gọi thử, sau này đối tác gọi hook) |
| Video Lesson (mp4, tối đa 1 GB) lưu ở static folder có phân quyền, mã hóa bằng ffmpeg qua worker nền, phát streaming | Quên mật khẩu |
| Email qua Resend (gói free): đăng ký, hoàn thành đơn | Bước xác thực email riêng ngoài Google |
| | Chuyển tiền tự động cho Instructor (kế toán chuyển khoản ngoài hệ thống) |
| | Lưu tài khoản ngân hàng của Instructor trong hệ thống (kế toán tự lưu) |

Ràng buộc kỹ thuật (input): backend .NET; admin bằng ReactJS; web học viên bằng Next.js tối ưu SEO. Phi kỹ thuật (deadline, ngân sách, team): N/A theo input.

## 4. Functional Requirements

| Mã | Nhóm | Hành vi (user làm gì → hệ thống phản hồi gì) | Ghi chú |
|---|---|---|---|
| FR-01 | Catalog | Khách/Student mở `/courses` → hệ thống trả danh sách khóa học đã được đăng, phân trang, lọc theo Category, sắp xếp theo giá hoặc ngày | Outline 1.6, 7.2. Chỉ khóa đã được Admin duyệt (FR-36) mới xuất hiện |
| FR-02 | Catalog | Khách/Student mở `/courses/[slug]` → hệ thống hiển thị thông tin khóa học, giá, danh sách Lesson (đánh dấu Lesson cho xem thử) và Instructor, metadata SEO riêng theo khóa | Outline 1.7, 7.3 |
| FR-03 | Auth | Student bấm đăng nhập trên web → được chuyển sang đăng nhập bằng tài khoản Google (SSO), quay lại web với phiên đăng nhập; lần đầu đăng nhập thì tài khoản Student được tạo; chưa đăng nhập thì không vào được giỏ hàng, `/my-courses`, trang học | Outline 7.4 ([log](decision-logs/2026-11-11-student-dang-nhap-google-sso.md)) |
| FR-04 | Auth | Admin/Instructor đăng nhập vào app quản trị qua OIDC → vào được trang quản trị theo vai trò; đăng xuất thì token bị thu hồi | Outline 6.2, 3.6. Instructor dùng Google như Student; Admin đăng nhập bằng email + mật khẩu, tài khoản tạo bằng seeding (chủ dự án xác nhận) |
| FR-05 | Auth | Người dùng gọi API không đúng vai trò (Admin/Instructor/Student) → hệ thống từ chối | Outline 3.7. Ma trận quyền chi tiết để ở architecture |
| FR-06 | Cart | Student thêm khóa học vào giỏ → giỏ của chính Student đó được cập nhật | Outline 2.2. Khóa đã mua thì bị từ chối (FR-30) |
| FR-07 | Coupon | Student nhập coupon ở giỏ → hệ thống tính lại giá cuối theo % giảm, thời hạn, giới hạn khóa học; coupon hết hạn hoặc không áp dụng được thì trả lỗi rõ ràng | Outline 2.3. Coupon do Admin tạo (FR-31) |
| FR-08 | Order | Student bấm Checkout → hệ thống chuyển giỏ thành Order trạng thái Mới, giỏ không còn dùng cho đơn đó | Outline 2.2 (Pending). Các trạng thái đơn: Mới, Đang chờ, Đã thanh toán, Đã hủy, Hết hạn : Mới = vừa tạo đơn, Đang chờ = đơn thiếu hoặc thừa tiền chờ Admin xử lý ([log](decision-logs/2026-11-11-trang-thai-don-moi-dang-cho-va-moc-tu-huy.md)) |
| FR-09 | Order | Student bấm Checkout hai lần liên tiếp (cùng idempotency key) → chỉ tạo đúng 1 Order | Outline 2.9 |
| FR-10 | Payment | Student chọn thanh toán một Order Pending → hệ thống hiển thị mã QR theo chuẩn VietQR với tài khoản nhận tiền lấy từ cấu hình `appsettings.json`, kèm số tiền và nội dung chuyển khoản duy nhất gắn với Order | Outline 2.4 đã đổi ([log](decision-logs/2026-11-11-thanh-toan-chi-hien-thi-qr-hook-test.md), [log](decision-logs/2026-11-11-qr-vietqr-tai-khoan-trong-appsettings.md)) |
| FR-11 | Payment | Màn hình thanh toán hỏi trạng thái đơn định kỳ → hệ thống trả trạng thái hiện tại của Order; khi Paid, web chuyển Student sang bước tiếp theo | Outline 2.4, 7.5 |
| FR-12 | Payment | Hook kèm header `X-API-Token` hợp lệ báo "tiền về" cho một Order chưa thanh toán còn hạn (trạng thái Mới hoặc Đang chờ), đúng mã thanh toán và số tiền → Order chuyển sang Đã thanh toán; gọi lại nhiều lần vẫn chỉ Paid một lần; thiếu hoặc sai token thì bị từ chối và không đổi dữ liệu; hook trả 200 khi thành công, 400 khi lỗi nghiệp vụ hoặc xác thực (sai số tiền thừa/thiếu, sai token, ...), 500 khi server lỗi; body phản hồi lỗi (400, 500) có `errorCode` và `errorMessage` | Khi test: chủ dự án tự gọi; khi tích hợp thật: đối tác ngân hàng gọi và bắt buộc truyền `X-API-Token` ([log](decision-logs/2026-11-11-bao-ve-hook-bang-x-api-token.md), [log](decision-logs/2026-11-11-hook-do-doi-tac-ngan-hang-goi-khi-tich-hop.md), [log](decision-logs/2026-11-11-doi-tac-thanh-toan-phai-gui-x-api-token.md), [log mã phản hồi](decision-logs/2026-11-11-hook-phan-hoi-200-400-500.md), [log body lỗi](decision-logs/2026-11-11-hook-loi-tra-errorcode-errormessage-trong-body.md)). Hook gọi cho đơn đã Hết hạn hoặc Đã hủy: xem FR-53. Danh sách giá trị `errorCode` do architecture định nghĩa |
| FR-13 | Payment | Hook báo số tiền thiếu hoặc thừa → hệ thống không chuyển Đã thanh toán, chuyển đơn sang Đang chờ, ghi nhận chênh lệch tiền và trả 400; đơn chờ Admin xử lý (FR-33) | [Log](decision-logs/2026-11-11-don-bat-thuong-cho-admin-xac-nhan.md), [log](decision-logs/2026-11-11-trang-thai-don-moi-dang-cho-va-moc-tu-huy.md) |
| FR-14 | Enrollment | Order chuyển Đã thanh toán (từ hook hoặc do Admin chuyển) → hệ thống tự tạo Enrollment cho từng khóa trong đơn (đúng 1 lần) và gửi email hoàn thành đơn qua Resend gồm mã đơn, danh sách khóa, tổng tiền và liên kết tới `/my-courses` | Outline 2.6, 2.7 (Outbox + job nền). Nội dung email do PO soạn theo yêu cầu chủ dự án |
| FR-15 | Enrollment | Student mở `/my-courses` → thấy đúng danh sách khóa đã mua | Outline 7.6 |
| FR-16 | Learning | Student mở bài học → hệ thống cho xem video nếu đã có Enrollment (bản đã duyệt gần nhất), kể cả khi khóa đang bị ẩn công khai; chưa có Enrollment thì từ chối (trừ Lesson cho xem thử, FR-46), kể cả khi gọi thẳng API hoặc URL file video | Outline 7.7; phân quyền cả file ở static folder ([log](decision-logs/2026-11-11-video-static-folder-phai-phan-quyen.md), [log](decision-logs/2026-11-11-khoa-duyet-lai-hoc-vien-xem-ban-cu.md)) |
| FR-17 | Admin | Admin tạo/sửa/xóa Category trong app quản trị → danh sách Category cập nhật | Outline 1.9, 6.3 |
| FR-18 | Admin | Admin sửa hoặc xóa hồ sơ Instructor → thay đổi hiển thị trên khóa học liên quan | Outline 1.9. Hồ sơ được tạo khi Admin duyệt Instructor (FR-40) |
| FR-19 | Admin | Instructor/Admin tạo và sửa khóa học (thông tin, giá) → khóa học lưu ở trạng thái chưa đăng | Outline 1.2 |
| FR-20 | Admin | Instructor/Admin thêm, sửa, xóa, sắp xếp lại Lesson trong khóa → thứ tự Lesson cập nhật | Outline 1.3 |
| FR-21 | Admin | Instructor gửi khóa học đi duyệt → nếu khóa chưa có ít nhất 1 Lesson thì bị từ chối kèm lý do; còn không thì khóa chuyển sang trạng thái chờ duyệt | Outline 1.1 (invariant ≥1 Lesson), thêm bước duyệt (FR-36) |
| FR-22 | Admin | Instructor sửa khóa học không phải của mình → hệ thống từ chối; Admin sửa được mọi khóa và thay đổi của Admin không cần duyệt; Instructor sửa khóa của mình đã đăng → khóa chuyển lại trạng thái chờ duyệt, khách không thấy khóa cho đến khi Admin duyệt, Student đã mua vẫn học được bản đã duyệt gần nhất | Outline 3.8 ([log](decision-logs/2026-11-11-khoa-da-dang-sua-phai-duyet-lai.md), [log](decision-logs/2026-11-11-khoa-cho-duyet-lai-khong-cong-khai.md), [log](decision-logs/2026-11-11-khoa-duyet-lai-hoc-vien-xem-ban-cu.md), [log](decision-logs/2026-11-11-admin-dang-khoa-khong-can-duyet.md)). Việc Admin sửa khóa của Instructor cũng không cần duyệt là suy từ "Admin tự đăng thì không cần duyệt" |
| FR-23 | Admin | Instructor/Admin chọn ảnh thumbnail cho khóa → ảnh tải thẳng lên kho lưu trữ qua presigned URL và hiển thị trên khóa học | Outline 1.8, 6.5 |
| FR-24 | Admin | Admin xem danh sách Order trong app quản trị → thấy lịch sử đơn và trạng thái (kể cả đơn bất thường) | Outline 2.8, 6.4 |
| FR-25 | Admin | Admin/Instructor xem dashboard doanh thu → thấy doanh thu theo Instructor và khoảng thời gian chọn; Instructor chỉ thấy của mình, kèm phần chia và số dư của mình | Outline 2.8, 6.4; Instructor thấy số dư (chủ dự án xác nhận) |
| FR-26 | Error | Client gửi dữ liệu không hợp lệ → hệ thống trả lỗi 400 có cấu trúc theo từng trường; mọi lỗi khác cũng theo định dạng ProblemDetails thống nhất | Outline 0.5, 0.9, 4.5 |
| FR-27 | Auth | Student đăng nhập Google lần đầu và tài khoản được tạo → ngay sau đó hệ thống gửi email chào mừng qua Resend, nội dung: xác nhận tài khoản đã tạo bằng email Google đó và liên kết tới `/courses`; không có bước xác thực email riêng | [Log](decision-logs/2026-11-11-bo-buoc-xac-thuc-email-rieng.md). Thời điểm do chủ dự án chốt, nội dung do PO soạn |
| FR-28 | Auth | [ĐÃ BỎ 2026-11-11] Quên mật khẩu: không còn trong phạm vi | Giữ mã để không đánh lại số ([log](decision-logs/2026-11-11-bo-chuc-nang-quen-mat-khau.md)) |
| FR-29 | Cart | Student xóa một khóa khỏi giỏ → giỏ cập nhật và giá cuối tính lại | Chủ dự án xác nhận |
| FR-30 | Cart | Student đã đăng nhập mở khóa học đã mua → thấy nút "Vào học" thay cho nút mua; gọi thêm khóa đó vào giỏ hoặc Checkout lại thì bị từ chối (mỗi khóa chỉ mua 1 lần) | Chủ dự án xác nhận. Khóa nằm trong đơn chưa thanh toán đã quá hạn thì cứ để đó, không coi là đã mua (FR-52 tự hủy đơn) |
| FR-31 | Coupon | Admin tạo và quản lý Coupon (% giảm, thời hạn, giới hạn khóa) trong app quản trị → Student dùng được ở FR-07 | Chủ dự án xác nhận: chỉ Admin |
| FR-32 | Order | Order chưa thanh toán quá 15 phút kể từ khi tạo → hệ thống chuyển trạng thái Hết hạn | Hạn 15 phút do chủ dự án chốt. Tiền về sau khi quá hạn: chờ Admin xử lý (FR-33) |
| FR-33 | Order | Admin mở một đơn ở bất kỳ trạng thái nào (kể cả thiếu tiền, thừa tiền, Hết hạn) và chọn chuyển sang Đã thanh toán hoặc Đã hủy → đơn đổi trạng thái theo lựa chọn; Đã thanh toán thì Enrollment được tạo (FR-14); Đã hủy với đơn đã có Enrollment thì quyền học bị thu hồi | [Log](decision-logs/2026-11-11-admin-chuyen-don-sang-hoan-thanh-hoac-huy.md), [log thu hồi](decision-logs/2026-11-11-huy-don-thu-hoi-enrollment.md) |
| FR-34 | Learning | Student đã có Enrollment bấm xem Lesson → video phát streaming trên web | Video lưu file trong static folder của web ([log](decision-logs/2026-11-11-video-static-folder-ma-hoa-ffmpeg.md)) |
| FR-35 | Learning | Instructor/Admin tải lên video cho Lesson → hệ thống chỉ nhận file mp4 tối đa 1 GB (file khác hoặc lớn hơn bị từ chối), worker nền mã hóa bằng ffmpeg rồi lưu vào static folder; trong lúc xử lý Lesson hiển thị trạng thái "đang xử lý" để học viên chờ; nếu ffmpeg lỗi thì Lesson hiển thị thông báo lỗi kèm nút gửi báo lỗi cho Admin (FR-50) | [Log](decision-logs/2026-11-11-upload-video-mp4-1gb-worker-ffmpeg-nen.md), [log lỗi](decision-logs/2026-11-11-ffmpeg-loi-thong-bao-va-nut-bao-loi-admin.md) |
| FR-36 | Admin | Admin duyệt một khóa đang chờ duyệt → khóa được đăng và hiện ở `/courses`; chưa duyệt thì không hiện công khai | Chủ dự án chốt ([log](decision-logs/2026-11-11-admin-duyet-moi-duoc-dang-khoa-hoc.md)). Từ chối duyệt: FR-49. Admin đăng khóa thì không cần duyệt |
| FR-37 | Order | Student mở lịch sử đơn → thấy các đơn của chính mình kèm trạng thái | Chủ dự án xác nhận |
| FR-38 | Revenue | Order chuyển Paid/Hoàn thành → hệ thống tự tính phần doanh thu chia cho Instructor của từng khóa trong đơn theo tỷ lệ riêng của Instructor đó nếu có, ngược lại theo tỷ lệ chung trong cấu hình hệ thống; tỷ lệ đó được sao chép vào Order tại thời điểm đơn hoàn thành và không đổi về sau; khoản chia được ghi nhận ở trạng thái chờ Admin duyệt | [Log](decision-logs/2026-11-11-chia-doanh-thu-theo-ty-le-instructor.md), [log](decision-logs/2026-11-11-chi-tra-instructor-qua-phieu-chi.md), [log](decision-logs/2026-11-11-ty-le-chia-sao-chep-vao-order-khi-hoan-thanh.md) |
| FR-39 | Revenue | Admin đặt hoặc xóa tỷ lệ riêng cho một Instructor (đã thống nhất với Instructor đó) → chỉ có tác dụng với các đơn hoàn thành sau thời điểm đổi, đơn đã hoàn thành không đổi; xóa thì quay về tỷ lệ chung | [Log](decision-logs/2026-11-11-ty-le-chia-sao-chep-vao-order-khi-hoan-thanh.md) |
| FR-40 | Auth | Admin duyệt một người dùng đã đăng ký thành Instructor (gán role) → người đó vào được app quản trị với quyền Instructor và hồ sơ Instructor được tạo | [Log](decision-logs/2026-11-11-instructor-dang-ky-thuong-admin-gan-role.md), [log hồ sơ](decision-logs/2026-11-11-ho-so-instructor-tao-khi-admin-duyet.md) |
| FR-41 | Auth | Khi hệ thống khởi tạo lần đầu (seeding) → tạo sẵn một tài khoản Admin với mật khẩu ngẫu nhiên và ghi mật khẩu vào file bí mật (ví dụ `secret.json`) nằm ngoài source control | [Log](decision-logs/2026-11-11-admin-dau-tien-tao-bang-seeding.md), [log](decision-logs/2026-11-11-mat-khau-admin-seed-ghi-vao-secret-json.md). Email tài khoản Admin seed: `support@tedu.com.vn` (chủ dự án chốt) |
| FR-42 | Auth | Admin đổi mật khẩu của mình → lần đăng nhập sau dùng mật khẩu mới | Chủ dự án nói sẽ đổi mật khẩu seed sau |
| FR-43 | Admin | Instructor cập nhật hồ sơ Instructor của chính mình → thông tin mới hiển thị trên các khóa của họ | Chủ dự án xác nhận; sửa hồ sơ người khác bị từ chối |
| FR-44 | Catalog | Khách/Student nhập từ khóa tìm khóa học → hệ thống tìm trong tiêu đề, mô tả và nội dung khóa học bằng full-text search của PostgreSQL, trả danh sách khóa đã đăng phù hợp, phân trang; gõ không dấu vẫn khớp tên có dấu | [Log](decision-logs/2026-11-11-tim-kiem-fulltext-postgresql.md). Hỗ trợ không dấu: chủ dự án xác nhận |
| FR-45 | Learning | Instructor/Admin đánh dấu một Lesson là cho xem thử → Lesson hiển thị nhãn xem thử trên trang khóa học | Chủ dự án: xem thử nếu video nào cho phép. Admin hoặc Instructor sở hữu khóa được đánh dấu (chủ dự án xác nhận) |
| FR-46 | Learning | Khách/Student chưa mua mở Lesson cho xem thử → video phát được; Lesson không cho xem thử thì bị từ chối | Ngoại lệ duy nhất của FR-16 và NFR-21. Khách chưa đăng nhập cũng xem thử được (chủ dự án xác nhận) |
| FR-47 | Revenue | Admin duyệt một khoản chia đang chờ → khoản đó được cộng vào số dư của Instructor | [Log](decision-logs/2026-11-11-chi-tra-instructor-qua-phieu-chi.md). "Tài khoản Instructor" là số dư trong hệ thống (chủ dự án xác nhận) |
| FR-48 | Revenue | Admin chọn số tiền và bấm thanh toán cho một Instructor → hệ thống tạo phiếu chi (Instructor, số tiền, thời điểm, người tạo) để kế toán chuyển khoản ngoài hệ thống; số tiền vượt số dư của Instructor thì bị từ chối; hệ thống không có bước kế toán xác nhận và không lưu tài khoản ngân hàng Instructor | [Log](decision-logs/2026-11-11-phieu-chi-admin-chon-so-tien-ke-toan-ngoai-he-thong.md). Phiếu chi không vượt số dư: chủ dự án xác nhận |
| FR-49 | Admin | Admin từ chối duyệt một khóa và nhập lý do (bắt buộc) → khóa ẩn với khách, Instructor xem được lý do; Student đã có Enrollment vẫn xem bản đã duyệt gần nhất; bản mới chỉ hiển thị sau khi được duyệt | [Log](decision-logs/2026-11-11-khoa-duyet-lai-hoc-vien-xem-ban-cu.md), [log](decision-logs/2026-11-11-tu-choi-duyet-co-ly-do-instructor-gui-lai.md) |
| FR-50 | Learning | Student thấy thông báo lỗi video bấm "Gửi báo lỗi" → báo lỗi kèm Lesson liên quan xuất hiện ở màn hình báo lỗi của Admin | [Log](decision-logs/2026-11-11-ffmpeg-loi-thong-bao-va-nut-bao-loi-admin.md). Student thấy nút, Admin nhận ở màn hình báo lỗi (chủ dự án xác nhận) |
| FR-51 | Admin | Instructor có khóa bị từ chối bấm "Gửi lại" → khóa chuyển lại trạng thái chờ duyệt; trong lúc chờ kết quả Instructor không cần làm gì thêm | [Log](decision-logs/2026-11-11-tu-choi-duyet-co-ly-do-instructor-gui-lai.md) |
| FR-52 | Order | Đơn Hết hạn mà Admin chưa xử lý sau 1 giờ kể từ lúc Hết hạn → hệ thống tự chuyển Đã hủy | [Log](decision-logs/2026-11-11-trang-thai-don-moi-dang-cho-va-moc-tu-huy.md) |
| FR-53 | Payment | Hook báo tiền về cho đơn đã Hết hạn hoặc Đã hủy → hệ thống vẫn ghi nhận khoản tiền, đơn chuyển sang Đang chờ để Admin xem xét (FR-33); không tạo Enrollment; hook trả 200 vì đã ghi nhận | [Log](decision-logs/2026-11-11-hook-don-het-han-da-huy-tra-200-dang-cho.md) |

## 5. Non-Functional Requirements

| Mã | Loại | Yêu cầu | Ghi chú |
|---|---|---|---|
| NFR-01 | Hiệu năng | API danh sách và chi tiết khóa học: p95 ≤ 300 ms ở tải 100 request/giây | Số theo chuẩn ngành (PO đề xuất) |
| NFR-02 | Hiệu năng | Từ lúc Order Paid đến khi Enrollment được tạo: ≤ 30 giây ở p95 | Liên quan FR-14; phụ thuộc chu kỳ job nền. PO đề xuất |
| NFR-03 | Hiệu năng | Trang `/courses` và `/courses/[slug]` đạt LCP ≤ 2,5 giây (p75, mạng di động) | Ngưỡng "good" của Core Web Vitals |
| NFR-04 | Toàn vẹn dữ liệu | Order chỉ chuyển sang Đã thanh toán đúng 1 lần dù hook gọi trùng hoặc đồng thời; Enrollment không bao giờ trùng | Test race condition bắt buộc (outline 2.5, 2.9, 8.4) |
| NFR-05 | Toàn vẹn dữ liệu | Việc ghi Order Paid và ghi sự kiện cấp Enrollment cùng một transaction; hệ thống crash giữa chừng không làm mất quyền học | Outbox Pattern (outline 2.6) |
| NFR-06 | Bảo mật | Đăng nhập theo OAuth2/OIDC: Authorization Code + PKCE cho SPA và web; web Next.js không lưu token ở localStorage (session phía server) | Outline 3.1–3.3, 7.4 |
| NFR-07 | Bảo mật | Endpoint hook kích hoạt đơn chỉ chấp nhận request có header `X-API-Token` là token được cấp riêng cho hook (cả khi đối tác gọi); thiếu hoặc sai thì từ chối | Chủ dự án chốt thay cho HMAC-SHA256 ở outline 2.5 |
| NFR-08 | Bảo mật | Kiểm tra quyền ở API, không chỉ ẩn trên UI: Enrollment cho video Lesson, quyền sở hữu cho sửa khóa học | Outline 3.8, 7.7 |
| NFR-09 | Bảo mật | Toàn bộ lưu lượng qua HTTPS; CORS chỉ cho phép origin của app quản trị và web; có security headers | Outline 3.9, 9.5 |
| NFR-10 | Bảo mật | Rate limiting nhóm endpoint checkout/thanh toán: tối đa 10 request/phút/người dùng, vượt thì trả 429 | Outline 4.4. PO đề xuất |
| NFR-11 | SEO | Trang công khai render phía server (SSR/ISR), mỗi trang khóa học có metadata riêng; điểm SEO Lighthouse ≥ 90 | Ràng buộc "Next.js tối ưu SEO" của input. PO đề xuất ngưỡng |
| NFR-12 | Khả năng mở rộng | Phía đọc tách khỏi phía ghi (ghi: EF Core + Aggregate; đọc: Dapper ra DTO); chịu được 10.000 tài khoản và 500 người dùng đồng thời trên 1 instance API | Quyết định kiến trúc trong outline TEDU-67. Quy mô do PO đề xuất |
| NFR-13 | Khả năng mở rộng | API versioned theo URL `/api/v1/...` và có OpenAPI schema; hai frontend dùng chung API client sinh từ schema | Outline 4.1, 4.3, 5.2 |
| NFR-14 | Uptime | Uptime mục tiêu 99% mỗi tháng (tối đa khoảng 7,3 giờ ngừng/tháng) | Chủ dự án chốt |
| NFR-15 | Vận hành | Log có cấu trúc, truy vấn được theo CorrelationId khi có sự cố | Outline 9.7 |
| NFR-16 | Chất lượng | Logic nghiệp vụ (Handler, Domain) có unit test, độ phủ dòng ≥ 70% cho Domain và Application; checkout và CRUD khóa học có integration test với PostgreSQL thật; test bắt buộc pass trước khi deploy | Outline Module 8, 9.6. Ngưỡng coverage do PO đề xuất |
| NFR-17 | Ràng buộc kỹ thuật | Backend .NET; admin bằng React; web bằng Next.js 16; CSDL PostgreSQL 18; mediator dùng thư viện Mediator (martinothamar, MIT) thay MediatR | Chủ dự án chốt ([PostgreSQL](decision-logs/2026-11-11-chon-postgresql-18.md), [Next.js](decision-logs/2026-11-11-chon-nextjs-16.md), [Mediator](decision-logs/2026-11-11-dung-mediator-martinothamar-thay-mediatr.md)) |
| NFR-18 | Vận hành | Email đăng ký và hoàn thành đơn gửi qua Resend gói free | Chủ dự án chốt ([log](decision-logs/2026-11-11-gui-email-bang-resend.md)); email quên mật khẩu đã bỏ. Hạn mức gói free cần kiểm tra lại với Resend trước khi chạy thật |
| NFR-19 | Uptime | Khi sự cố: RPO ≤ 24 giờ (backup CSDL và file video ít nhất 1 lần/ngày), RTO ≤ 4 giờ | Chủ dự án yêu cầu theo chuẩn ngành; số do PO đề xuất |
| NFR-20 | Hiệu năng | Video bắt đầu phát trong ≤ 3 giây (p95) sau khi bấm xem | PO đề xuất |
| NFR-21 | Bảo mật | Video Lesson được mã hóa bằng ffmpeg; file video ở static folder chỉ truy cập được bởi người có Enrollment (và Admin/Instructor sở hữu), người khác nhận từ chối dù biết URL; ngoại lệ duy nhất là Lesson cho xem thử | Chủ dự án chốt: phải phân quyền. Cách thực hiện thuộc architecture |
| NFR-22 | Bảo mật | Mật khẩu Admin seed được sinh ngẫu nhiên khi chạy, ghi vào file bí mật bị loại khỏi source control (ví dụ `secret.json` trong `.gitignore`), không nằm trong file commit; `X-API-Token` cũng không nằm trong file commit; mật khẩu lưu dạng băm | Chủ dự án chọn file bí mật cho mật khẩu seed; phần token và băm do PO đề xuất mức tối thiểu |
| NFR-23 | Khả năng sử dụng | Luồng của Instructor (cập nhật hồ sơ, tạo khóa và Lesson, upload video, gửi duyệt) dùng được qua giao diện, không cần dòng lệnh hay kiến thức kỹ thuật | Chủ dự án: Instructor có kỹ năng máy tính cơ bản, không phải developer. Tiêu chí đo do PO đề xuất: thử với ≥ 3 Instructor thật, mỗi người hoàn thành luồng ở lần thử đầu mà không cần hỗ trợ trực tiếp |

## 6. Success Metrics

Chủ dự án chưa cần mục tiêu kinh doanh; chỉ giữ các chỉ số kỹ thuật đo được.

| Chỉ số | Cách đo | Mục tiêu |
|---|---|---|
| Đơn Paid có Enrollment đúng 1 bản | Đối chiếu Order Paid với Enrollment | 100% (0 thiếu, 0 trùng) |
| Order Paid hai lần do hook gọi trùng/đồng thời | Test race condition + log hook | 0 đơn |
| Truy cập video Lesson không cho xem thử khi chưa Enrollment | Test tích hợp gọi thẳng API và URL file | 0 lần thành công |
| Gọi hook thiếu hoặc sai `X-API-Token` | Test tích hợp | 0 lần đổi trạng thái đơn |
| Đơn chưa thanh toán quá 15 phút còn ở trạng thái Mới/Đang chờ | Truy vấn DB theo job quá hạn | 0 đơn |
| Đơn Hết hạn chưa được Admin xử lý còn tồn quá 1 giờ | Truy vấn DB | 0 đơn |
| Phiếu chi vượt số dư của Instructor | Test tích hợp | 0 phiếu tạo được |
| Enrollment tạo từ hook gọi cho đơn Hết hạn/Đã hủy | Test tích hợp | 0 Enrollment |
| Phần chia Instructor so với tỷ lệ áp dụng của đơn Paid | Đối chiếu tính toán với đơn | 100% khớp; đơn Paid cũ không đổi khi đổi tỷ lệ |
| Số dư Instructor so với tổng khoản chia đã duyệt trừ tổng phiếu chi | Đối chiếu sổ | 100% khớp |
| Student đã Enrollment xem được bản đã duyệt gần nhất khi khóa chờ duyệt lại hoặc bị từ chối | Test tích hợp | 100%; khách thấy khóa: 0 lần |
| Upload video không phải mp4 hoặc > 1 GB | Test tích hợp | 100% bị từ chối |
| Uptime hàng tháng | Giám sát uptime | ≥ 99% |

## 7. Open Questions

| Mã | Câu hỏi | Ảnh hưởng |
|---|---|---|
Không còn câu hỏi mở. Đã trả lời ngày 2026-11-11: Q-01 đến Q-31.
