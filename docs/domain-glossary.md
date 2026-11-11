# Domain Glossary — TEDU Market

Nơi chốt DUY NHẤT 1 tên cho mỗi khái niệm nghiệp vụ, và là bảng map duy nhất giữa tên hiển thị (UI copy, tài liệu) và tên trong code.

## Vai trò

| Hiển thị | Code | Định nghĩa | KHÔNG gọi là | Liên quan |
|---|---|---|---|---|
| Học viên | `Student` | Người đăng nhập Google, mua và học khóa học. Tài khoản tạo ở lần đăng nhập đầu. | người học, người dùng, user, khách hàng | Khách, Enrollment, Instructor |
| Instructor | `Instructor` | Học viên được Admin duyệt, tạo khóa học và bài học của mình, xem doanh thu và khoản chia. | người dạy, giảng viên, giáo viên, teacher | Hồ sơ Instructor, Duyệt Instructor, Khoản chia |
| Admin | `Admin` | Quản trị toàn hệ thống: duyệt, sửa mọi khóa, xử lý đơn, tạo phiếu chi. Tài khoản đầu tạo bằng seeding. | quản trị viên, administrator | App quản trị, Phiếu chi |
| Khách | `Guest` | Người chưa đăng nhập; xem được danh sách, chi tiết khóa học và bài học xem thử. | visitor, người lạ, anonymous | Học viên, Bài học xem thử |

## Ứng dụng

| Hiển thị | Code | Định nghĩa | KHÔNG gọi là | Liên quan |
|---|---|---|---|---|
| Web học viên | `StudentWeb` | Website Next.js công khai (SEO) cho Khách và Học viên: duyệt, mua, học. | web Student, storefront, trang người học | Học viên, Khách |
| App quản trị | `AdminApp` | Ứng dụng React cho Admin và Instructor quản lý nội dung, đơn, doanh thu. | admin panel, CMS, backoffice, dashboard | Admin, Instructor |

## Catalog

| Hiển thị | Code | Định nghĩa | KHÔNG gọi là | Liên quan |
|---|---|---|---|---|
| Khóa học | `Course` | Sản phẩm bán, gồm thông tin, giá, các bài học; thuộc 1 Instructor và 1 Category. | sản phẩm, course (trên UI), lớp học | Bài học, Category, Instructor |
| Bài học | `Lesson` | Đơn vị nội dung trong khóa học, gắn 1 video; có thứ tự. | Lesson (trên UI), bài giảng, tiết học | Khóa học, Video, Bài học xem thử |
| Bài học xem thử | `PreviewLesson` | Bài học được đánh dấu cho xem không cần mua, kể cả Khách. | free lesson, bài miễn phí, demo | Bài học, Khách |
| Video | `Video` | File mp4 (tối đa 1 GB) của bài học, mã hóa bằng ffmpeg, lưu static folder có phân quyền. | clip, media | Bài học, Báo lỗi video |
| Category | `Category` | Nhóm phân loại khóa học do Admin quản lý, dùng để lọc danh sách. | danh mục, chuyên mục, thể loại | Khóa học |
| Hồ sơ Instructor | `InstructorProfile` | Thông tin hiển thị của Instructor trên khóa học; tạo khi Admin duyệt Instructor. | profile, tài khoản Instructor | Instructor, Duyệt Instructor |

## Vòng đời khóa học

| Hiển thị | Code | Định nghĩa | KHÔNG gọi là | Liên quan |
|---|---|---|---|---|
| Chưa đăng | `Draft` | Trạng thái khóa học mới tạo hoặc chưa gửi duyệt; không hiện công khai. | nháp, bản nháp | Chờ duyệt (khóa học) |
| Chờ duyệt (khóa học) | `PendingApproval` | Khóa đã gửi duyệt, cần ≥1 bài học; ẩn với Khách. Khác "Khoản chia chờ duyệt". | đang duyệt, pending review | Duyệt khóa học, Khoản chia chờ duyệt |
| Đã đăng | `Published` | Khóa được Admin duyệt (hoặc Admin tự đăng) và hiện ở danh sách công khai. | public, đã duyệt (khóa học), live | Duyệt khóa học |
| Bị từ chối | `Rejected` | Khóa bị Admin từ chối kèm lý do; Instructor xem lý do và gửi lại. | không duyệt, bị trả về | Lý do từ chối, Gửi lại |
| Duyệt khóa học | `CourseApproval` | Admin duyệt khóa đang chờ để đăng, hoặc từ chối kèm lý do bắt buộc. | phê duyệt khóa, publish | Chờ duyệt (khóa học), Bị từ chối |
| Lý do từ chối | `RejectionReason` | Nội dung bắt buộc Admin nhập khi từ chối khóa; Instructor xem được. | ghi chú duyệt, feedback | Bị từ chối |
| Gửi lại | `Resubmit` | Instructor đưa khóa bị từ chối trở lại trạng thái chờ duyệt. | nộp lại, submit lại | Bị từ chối |
| Bản đã duyệt gần nhất | `LastApprovedVersion` | Phiên bản khóa đã được duyệt; Học viên đã có Enrollment vẫn học bản này khi khóa chờ duyệt lại hoặc bị từ chối. | bản cũ, bản công khai | Enrollment, Chờ duyệt (khóa học) |
| Duyệt Instructor | `InstructorApproval` | Admin gán role Instructor cho người đã đăng ký; hồ sơ Instructor được tạo. | cấp quyền Instructor, nâng cấp tài khoản | Instructor, Hồ sơ Instructor |

## Mua hàng

| Hiển thị | Code | Định nghĩa | KHÔNG gọi là | Liên quan |
|---|---|---|---|---|
| Giỏ hàng | `Cart` | Danh sách khóa học Học viên định mua; mỗi Học viên 1 giỏ; Checkout thì chuyển thành Đơn. | giỏ, basket | Đơn, Coupon |
| Coupon | `Coupon` | Mã giảm giá theo %, có thời hạn và giới hạn khóa học; chỉ Admin tạo. | mã giảm giá, voucher, khuyến mãi | Giỏ hàng |
| Checkout | `Checkout` | Hành động Học viên chuyển giỏ hàng thành đơn (có idempotency key). | đặt hàng, thanh toán giỏ | Giỏ hàng, Đơn |
| Đơn | `Order` | Bản ghi mua một hay nhiều khóa học của 1 Học viên, kèm trạng thái và tổng tiền. | đơn hàng, Order (trên UI), hóa đơn | Checkout, Thanh toán, Enrollment |
| Mới | `New` | Trạng thái đơn vừa tạo, chưa thanh toán, còn hạn 15 phút. | Pending, chưa thanh toán | Hết hạn, Đang chờ |
| Đang chờ | `OnHold` | Trạng thái đơn thiếu/thừa tiền hoặc có tiền về sau Hết hạn/Đã hủy, chờ Admin xử lý. | Pending, treo, bất thường | Chênh lệch tiền, Xử lý đơn |
| Đã thanh toán | `Paid` | Trạng thái đơn đã thu đủ tiền; kích hoạt Enrollment và Khoản chia. | Hoàn thành, Completed, Paid (trên UI) | Enrollment, Khoản chia |
| Đã hủy | `Cancelled` | Trạng thái đơn bị Admin hủy hoặc tự hủy sau 1 giờ Hết hạn; thu hồi Enrollment nếu có. | huỷ, Canceled, bị hủy | Xử lý đơn, Enrollment |
| Hết hạn | `Expired` | Trạng thái đơn chưa thanh toán quá 15 phút; chờ Admin xử lý, 1 giờ sau tự hủy. | quá hạn, timeout | Đã hủy, Đang chờ |
| Xử lý đơn | `OrderResolution` | Admin chuyển đơn ở bất kỳ trạng thái sang Đã thanh toán hoặc Đã hủy. | duyệt đơn, xác nhận đơn | Đơn, Đang chờ |
| Enrollment | `Enrollment` | Quyền học 1 khóa của 1 Học viên, tạo đúng 1 lần khi đơn Đã thanh toán; mỗi khóa mua 1 lần. | quyền học, đăng ký học, ghi danh, khóa đã mua | Học viên, Khóa học, Đơn |

## Thanh toán

| Hiển thị | Code | Định nghĩa | KHÔNG gọi là | Liên quan |
|---|---|---|---|---|
| Thanh toán | `Payment` | Việc Học viên chuyển khoản cho 1 đơn theo mã QR; hệ thống chưa tích hợp cổng thanh toán thật. | giao dịch, transaction | Mã QR, Hook thanh toán |
| Mã QR | `PaymentQr` | Mã VietQR hiển thị số tiền, tài khoản nhận (từ cấu hình) và nội dung chuyển khoản của đơn. | QR code, mã VietQR | Thanh toán, Nội dung chuyển khoản |
| Nội dung chuyển khoản | `TransferContent` | Chuỗi duy nhất gắn với 1 đơn, dùng để hook đối chiếu đơn. | mã thanh toán, mã đơn chuyển khoản | Mã QR, Hook thanh toán |
| Hook thanh toán | `PaymentHook` | Endpoint báo "tiền về", yêu cầu header X-API-Token; gọi lại nhiều lần chỉ Paid một lần. | webhook, callback, IPN | X-API-Token, Chênh lệch tiền |
| X-API-Token | `X-API-Token` | Token cấp riêng cho hook thanh toán, gửi qua header cùng tên; thiếu hoặc sai thì từ chối. | API key, secret key, bearer token | Hook thanh toán |
| Chênh lệch tiền | `AmountDiscrepancy` | Phần tiền thừa hoặc thiếu so với tổng đơn, ghi nhận khi hook báo sai số tiền. | sai tiền, lệch tiền | Đang chờ, Hook thanh toán |

## Doanh thu Instructor

| Hiển thị | Code | Định nghĩa | KHÔNG gọi là | Liên quan |
|---|---|---|---|---|
| Doanh thu | `Revenue` | Tổng tiền đơn Đã thanh toán, xem theo Instructor và khoảng thời gian trên dashboard. | thu nhập, sales | Khoản chia |
| Tỷ lệ chia chung | `DefaultShareRate` | Tỷ lệ chia mặc định lấy từ cấu hình hệ thống, áp cho Instructor không có tỷ lệ riêng. | tỷ lệ mặc định, commission chung | Tỷ lệ chia riêng |
| Tỷ lệ chia riêng | `InstructorShareRate` | Tỷ lệ Admin đặt cho 1 Instructor; chỉ áp cho đơn Đã thanh toán sau lúc đổi. | tỷ lệ riêng lẻ, commission riêng | Tỷ lệ chia chung, Khoản chia |
| Khoản chia | `RevenueShare` | Phần doanh thu của Instructor tính cho từng khóa trong đơn; tỷ lệ được sao chép vào đơn lúc Đã thanh toán. | phần chia, chia sẻ doanh thu, hoa hồng | Đơn, Số dư |
| Khoản chia chờ duyệt | `PendingShare` | Trạng thái khoản chia mới ghi nhận, chờ Admin duyệt. Khác "Chờ duyệt (khóa học)". | pending share | Khoản chia, Số dư |
| Số dư | `Balance` | Tổng khoản chia đã duyệt trừ tổng phiếu chi của Instructor, tính trong hệ thống. | tài khoản Instructor, ví | Khoản chia, Phiếu chi |
| Phiếu chi | `Payout` | Bản ghi Admin tạo (Instructor, số tiền, thời điểm, người tạo) để kế toán chuyển khoản ngoài hệ thống; không vượt số dư. | lệnh chi, chuyển tiền, rút tiền | Số dư, Admin |

## Học tập

| Hiển thị | Code | Định nghĩa | KHÔNG gọi là | Liên quan |
|---|---|---|---|---|
| Khóa học của tôi | `MyCourses` | Trang `/my-courses` liệt kê các khóa Học viên có Enrollment. | thư viện, khóa đã mua | Enrollment, Học viên |
| Trạng thái xử lý video | `VideoProcessingStatus` | Trạng thái Video của bài học sau upload: đang xử lý (ffmpeg chạy nền), sẵn sàng, hoặc lỗi (hiện thông báo và nút gửi Báo lỗi video). | trạng thái encode, trạng thái upload | Video, Báo lỗi video |
| Báo lỗi video | `VideoErrorReport` | Báo cáo Học viên gửi Admin khi video bài học lỗi xử lý ffmpeg. | bug report, feedback, khiếu nại | Video, Bài học, Admin |

## Màn hình và nhãn thao tác

Tên màn hình/nhãn dùng trên UI mà chưa thuộc khái niệm nghiệp vụ nào ở trên (nguồn: [ux.md](ux.md)). Màn hình trùng tên khái niệm (Giỏ hàng, Thanh toán, Khóa học của tôi, Đơn, Doanh thu, Phiếu chi, Báo lỗi video, Category, Coupon...) dùng đúng tên khái niệm đó.

| Hiển thị | Code | Định nghĩa | KHÔNG gọi là | Liên quan |
|---|---|---|---|---|
| Danh sách khóa học | `CourseList` | Màn hình `/courses` của Web học viên: lọc Category, sắp xếp, tìm kiếm. | trang chủ khóa học, catalog, shop | Khóa học, Category |
| Chi tiết khóa học | `CourseDetail` | Màn hình `/courses/[slug]`: thông tin, giá, danh sách bài học, Instructor, nút mua hoặc "Vào học". | trang sản phẩm, trang khóa học | Khóa học, Bài học xem thử |
| Trang học | `LearningPage` | Màn hình Học viên xem video và danh sách bài học của một khóa. | player, classroom | Enrollment, Video |
| Lịch sử đơn | `OrderHistory` | Màn hình Học viên xem các đơn của mình kèm trạng thái. | đơn đã mua, hóa đơn của tôi | Đơn |
| Mã đơn | `OrderCode` | Mã hiển thị để nhận diện một đơn cho Học viên và Admin. | số đơn, order id | Đơn |
| Giá cuối | `FinalPrice` | Giá sau khi áp Coupon, hiển thị ở Giỏ hàng và Đơn. | giá sau giảm, thành tiền | Giỏ hàng, Coupon |
| Vào học | `GoToLearning` | Nút thay cho nút mua khi Học viên đã có Enrollment của khóa. | học ngay, xem khóa học | Enrollment, Chi tiết khóa học |
| Xem thử | `PreviewBadge` | Nhãn trên bài học được đánh dấu Bài học xem thử. | miễn phí, free, demo | Bài học xem thử |
| Thêm vào giỏ | `AddToCart` | Nút thêm khóa học vào Giỏ hàng. | mua ngay, thêm giỏ | Giỏ hàng |
| Gửi duyệt | `SubmitForApproval` | Nút Instructor đưa khóa từ Chưa đăng sang Chờ duyệt (khóa học). Khác "Gửi lại" (chỉ cho khóa Bị từ chối). | nộp duyệt, publish, đăng khóa | Chờ duyệt (khóa học), Gửi lại |
| Gửi báo lỗi | `SendVideoErrorReport` | Nút Học viên gửi Báo lỗi video khi video ở trạng thái Lỗi. | báo cáo, khiếu nại | Báo lỗi video |
| Tạo phiếu chi | `CreatePayout` | Nút Admin tạo Phiếu chi cho Instructor. | thanh toán cho Instructor, chuyển tiền, rút tiền | Phiếu chi, Số dư |
| Đăng nhập bằng Google | `SignInWithGoogle` | Nút đăng nhập của Học viên và Instructor. | đăng nhập Gmail, đăng ký | Học viên, Instructor |
