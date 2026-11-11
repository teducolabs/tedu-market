# UX — TEDU Market

Nguồn: [prd.md](prd.md) (persona, FR), [domain-glossary.md](domain-glossary.md) (mọi label, tên màn hình, tên trạng thái lấy từ cột Hiển thị), [architecture.md](architecture.md) mục 2 (Next.js 16 + React Vite). Chưa có brand guideline: mọi giá trị thị giác đánh `[ĐỀ XUẤT - CẦN DUYỆT]`. Nền UI: Tailwind v4 + shadcn/ui, dùng chung qua `packages/ui` ([log](decision-logs/2026-11-11-ui-tailwind-shadcn-dung-chung-package-ui.md)).

## 1. Nguyên tắc thiết kế

| # | Nguyên tắc | Gắn với persona | Hệ quả khi làm UI |
|---|---|---|---|
| 1 | Mua trong ít bước, chỉ đăng nhập khi cần | Học viên chỉ có kỹ năng máy tính cơ bản; Khách xem được mọi thứ công khai (FR-02, FR-46) | Duyệt, tìm, xem thử không cần đăng nhập. Đăng nhập bằng Google hiện ở lúc Thêm vào giỏ. Từ Giỏ hàng đến Mã QR tối đa 3 màn hình |
| 2 | Màn hình Thanh toán chỉ làm một việc | Học viên chuyển khoản qua QR, không có cổng thanh toán (FR-10, FR-11) | Mã QR, số tiền, Nội dung chuyển khoản (nút sao chép), đếm ngược 15 phút; không quảng cáo hay link rời trang |
| 3 | Không lộ thuật ngữ kỹ thuật | Học viên và Instructor không phải developer (NFR-23) | Không hiển thị ffmpeg, hook, token, idempotency, mã HTTP, `errorCode`. Lỗi nói chuyện gì xảy ra và làm gì tiếp |
| 4 | Mọi trạng thái bất đồng bộ phải nhìn thấy kèm bước tiếp theo | Khóa Chờ duyệt, Đơn Đang chờ, Video đang xử lý đều chờ người/job khác | Badge trạng thái + 1 câu giải thích "bạn cần làm gì / chờ ai". Không để người dùng đoán |
| 5 | Instructor đi theo trình tự, hệ thống chặn sai sớm | Instructor đào tạo ngoài lập trình, tự làm không có hỗ trợ (NFR-23) | Tạo khóa theo bước (thông tin → Bài học → Gửi duyệt); checklist "cần ≥ 1 Bài học" trước Gửi duyệt; chặn file sai định dạng ngay khi chọn |
| 6 | Admin: dày thông tin, hành động nặng phải xác nhận hậu quả | Admin là chuyên gia, xử lý nhiều đơn/khóa (FR-24, FR-33) | Bảng có lọc/tìm; Đã hủy, Từ chối, Tạo phiếu chi luôn qua modal nêu hệ quả cụ thể |

## 2. Information Architecture

### Web học viên (Khách + Học viên)

```
Header: logo · ô tìm kiếm · Giỏ hàng (số khóa) · Khóa học của tôi · tài khoản (Lịch sử đơn, Đăng xuất) / Đăng nhập bằng Google
├─ Danh sách khóa học  /courses ................ FR-01, FR-44
├─ Chi tiết khóa học   /courses/[slug] ......... FR-02, FR-06, FR-30
│   └─ Bài học xem thử (phát tại chỗ, Khách xem được) ... FR-45, FR-46
├─ Đăng nhập bằng Google (chuyển hướng SSO) .... FR-03, FR-27
├─ Giỏ hàng + Coupon ......................... FR-06, FR-07, FR-29   (cần đăng nhập)
├─ Thanh toán (Mã QR theo Đơn) ................ FR-08–FR-11, FR-32   (cần đăng nhập)
├─ Khóa học của tôi  /my-courses ............... FR-15              (cần đăng nhập)
│   └─ Trang học (Video + danh sách Bài học) ... FR-16, FR-34, FR-50
└─ Lịch sử đơn ............................... FR-37              (cần đăng nhập)
```

Chưa đăng nhập mà vào trang cần đăng nhập → chuyển sang Đăng nhập bằng Google, xong quay lại đúng trang đó (FR-03). `/` hiển thị Danh sách khóa học [ĐỀ XUẤT].

### App quản trị (Admin + Instructor)

Sidebar trái; mục hiển thị theo vai trò (API vẫn kiểm quyền, FR-05).

| Mục sidebar | Vai trò | Màn hình con | FR |
|---|---|---|---|
| Doanh thu | Admin, Instructor | Doanh thu theo Instructor và khoảng thời gian; Instructor chỉ thấy của mình kèm Khoản chia và Số dư | FR-25 |
| Khóa học | Admin, Instructor (của mình) | Danh sách; Sửa khóa (thông tin, giá, thumbnail); Bài học (thêm/sửa/xóa/sắp xếp, upload Video, đánh dấu Xem thử); Gửi duyệt; Gửi lại | FR-19–FR-23, FR-35, FR-45, FR-21, FR-51 |
| Hồ sơ Instructor | Instructor (của mình) | Sửa hồ sơ | FR-43 |
| Duyệt khóa học | Admin | Hàng đợi Chờ duyệt (khóa học); Duyệt / Từ chối + Lý do từ chối | FR-36, FR-49 |
| Đơn | Admin | Danh sách, chi tiết, Xử lý đơn | FR-24, FR-33 |
| Instructor | Admin | Danh sách; Duyệt Instructor; sửa/xóa Hồ sơ Instructor; đặt/xóa Tỷ lệ chia riêng | FR-40, FR-18, FR-39 |
| Khoản chia chờ duyệt | Admin | Duyệt Khoản chia | FR-47 |
| Phiếu chi | Admin | Danh sách; Tạo phiếu chi | FR-48 |
| Category | Admin | CRUD | FR-17 |
| Coupon | Admin | CRUD | FR-31 |
| Báo lỗi video | Admin | Danh sách báo lỗi kèm Bài học liên quan | FR-50 |
| Menu tài khoản | Cả hai | Đăng xuất; Đổi mật khẩu (chỉ Admin) | FR-04, FR-42 |

## 3. Luồng chính

Mỗi luồng ghi bốn trạng thái bắt buộc: **Loading / Empty / Error / Success**. Câu chữ lỗi theo mục 7.

### Luồng: Tìm và xem Khóa học (Khách, Học viên)
1. Mở Danh sách khóa học → lọc Category, sắp xếp theo giá hoặc ngày, hoặc nhập từ khóa (gõ không dấu vẫn khớp, FR-44); phân trang.
2. Chọn một khóa → Chi tiết khóa học: giá, Instructor, danh sách Bài học (nhãn Xem thử trên Bài học xem thử).
3. Nút chính theo trạng thái: chưa mua → Thêm vào giỏ; đã có Enrollment → Vào học (FR-30); Khách bấm Thêm vào giỏ → Đăng nhập bằng Google.
- **Loading:** skeleton lưới thẻ khóa (danh sách), skeleton khối thông tin (chi tiết).
- **Empty:** không có khóa nào → "Chưa có khóa học nào"; tìm/lọc không ra → "Không tìm thấy khóa học phù hợp" + nút "Xóa bộ lọc".
- **Error:** không tải được → khối lỗi trong trang + nút "Thử lại"; slug không tồn tại hoặc chưa Đã đăng → trang không tìm thấy kèm nút về Danh sách khóa học.
- **Success:** Thêm vào giỏ → toast "Đã thêm vào giỏ hàng" có link "Xem Giỏ hàng"; số trên header cập nhật.

### Luồng: Đăng nhập bằng Google (Học viên, Instructor)
1. Bấm Đăng nhập bằng Google → chuyển sang Google → quay lại trang đang xem.
2. Lần đầu: tài khoản Học viên được tạo; email chào mừng gửi nền (FR-27), không chặn UI.
- **Loading:** toàn trang "Đang đăng nhập…" lúc quay lại.
- **Empty:** không áp dụng.
- **Error:** người dùng hủy hoặc Google lỗi → về trang cũ, banner "Đăng nhập chưa hoàn tất. Bạn thử lại nhé" + nút Đăng nhập bằng Google.
- **Success:** header hiển thị tài khoản; không toast riêng ngoài lần đầu ("Chào mừng bạn đến TEDU Market").

### Luồng: Giỏ hàng và Coupon (Học viên)
1. Giỏ hàng liệt kê khóa kèm giá; xóa từng khóa (FR-29).
2. Nhập Coupon → Áp dụng → hiển thị mức giảm và Giá cuối (FR-07).
3. Bấm Checkout (xem luồng sau).
- **Loading:** skeleton dòng khóa; nút Áp dụng có spinner, các nút khác vẫn dùng được.
- **Empty:** "Giỏ hàng của bạn đang trống" + nút "Xem khóa học".
- **Error:** Coupon hết hạn/không áp dụng được → lỗi dưới ô Coupon nêu lý do cụ thể ("Coupon đã hết hạn", "Coupon không áp dụng cho khóa trong giỏ"), giỏ giữ nguyên; khóa đã mua (FR-30) → "Bạn đã sở hữu khóa này" + link Vào học.
- **Success:** Coupon áp dụng → dòng giảm giá màu success + nút "Bỏ Coupon"; xóa khóa → Giá cuối tính lại ngay.

### Luồng: Checkout và Thanh toán (Học viên)
1. Checkout → tạo Đơn trạng thái Mới (hai lần bấm vẫn một Đơn, FR-09) → màn hình Thanh toán.
2. Hiển thị Mã QR, số tiền, Nội dung chuyển khoản (nút sao chép), Mã đơn, đếm ngược 15 phút (FR-32). Trang tự hỏi trạng thái định kỳ ~3 giây [ĐỀ XUẤT] (FR-11).
3. Đơn thành Đã thanh toán → màn hình thành công → nút Vào học (tới Khóa học của tôi).
- **Loading:** nút Checkout có spinner và bị khóa tới khi có kết quả; QR dùng skeleton vuông cùng kích thước.
- **Empty:** vào Thanh toán mà không có Đơn Mới → chuyển về Giỏ hàng.
- **Error:** (a) tạo Đơn lỗi → banner trong Giỏ hàng, giỏ giữ nguyên, nút "Thử lại"; (b) hỏi trạng thái lỗi → giữ nguyên QR, dải nhỏ "Chưa cập nhật được trạng thái, đang thử lại", không đổi nội dung chính; (c) vượt giới hạn (429) → "Bạn thao tác hơi nhanh. Vui lòng thử lại sau ít phút".
- **Success / trạng thái khác:**
  - Đã thanh toán: tiêu đề "Thanh toán thành công", tóm tắt Đơn; nút Vào học chuyển sang "Đang mở khóa học…" cho tới khi Enrollment có (tối đa ~30 giây, NFR-02), không báo lỗi trong lúc chờ.
  - Hết hạn: "Đơn đã Hết hạn" + nút "Quay lại Giỏ hàng"; nêu "Nếu bạn đã chuyển khoản, đơn sẽ chuyển sang Đang chờ để Admin xem xét" (FR-53).
  - Đang chờ: "Chúng tôi đã nhận chuyển khoản nhưng cần Admin kiểm tra số tiền. Bạn không cần làm gì thêm" (FR-13).

### Luồng: Học (Học viên, Khách xem thử)
1. Khóa học của tôi → chọn khóa → Trang học: Video bên trái/trên, danh sách Bài học bên phải/dưới. Khách vào Bài học xem thử từ Chi tiết khóa học.
2. Học viên vẫn học được khi khóa bị ẩn hay đang duyệt lại; không hiển thị banner nào về việc đó (FR-16, FR-49).
- **Loading:** skeleton khung Video 16:9; Video bắt đầu ≤ 3 giây (NFR-20), quá 3 giây hiện spinner trên khung.
- **Empty:** Khóa học của tôi chưa có khóa → "Bạn chưa có khóa học nào" + "Xem khóa học".
- **Error:** Video trạng thái Đang xử lý → "Video đang được xử lý, bạn quay lại sau nhé"; Lỗi → "Video này đang gặp sự cố" + nút Gửi báo lỗi (FR-35, FR-50); chưa có Enrollment mà gọi thẳng → về Chi tiết khóa học kèm "Bạn cần mua khóa học này để xem bài học"; Bài học không xem thử với Khách → Đăng nhập bằng Google.
- **Success:** Gửi báo lỗi xong → toast "Đã gửi báo lỗi cho Admin" và nút đổi thành "Đã gửi báo lỗi" (khóa).

### Luồng: Instructor tạo và gửi duyệt Khóa học
1. Khóa học → Tạo: nhập thông tin, giá, Category, chọn thumbnail → Lưu (khóa ở trạng thái Chưa đăng, FR-19, FR-23).
2. Thêm Bài học: tiêu đề, chọn Video (mp4 ≤ 1 GB), tùy chọn Xem thử, kéo thả sắp xếp (FR-20, FR-35, FR-45).
3. Gửi duyệt: checklist (có ≥ 1 Bài học, đủ thông tin) → khóa thành Chờ duyệt (khóa học) (FR-21).
4. Bị từ chối: banner đầu trang hiện Lý do từ chối + nút Gửi lại (FR-49, FR-51). Sửa khóa Đã đăng: modal báo "Khóa sẽ ẩn với Khách cho đến khi Admin duyệt; Học viên đã mua vẫn học bản đã duyệt gần nhất" rồi mới lưu (FR-22).
- **Loading:** skeleton form khi mở; nút Lưu spinner; upload Video có thanh tiến độ % và nút Hủy; Video thành Đang xử lý sau upload.
- **Empty:** chưa có khóa → "Bạn chưa có khóa học nào" + nút "Tạo khóa học"; khóa chưa có Bài học → "Chưa có Bài học" + nút thêm, Gửi duyệt khóa kèm lý do vì sao chưa dùng được.
- **Error:** file không phải mp4 hoặc > 1 GB → báo ngay khi chọn, trước khi upload; upload đứt → "Tải lên bị gián đoạn" + "Tải lại"; xử lý Video Lỗi → badge Lỗi trên Bài học; lỗi từng trường theo FR-26 hiện dưới đúng ô.
- **Success:** Lưu → toast "Đã lưu"; Gửi duyệt → badge Chờ duyệt (khóa học) + dòng "Admin sẽ duyệt khóa này. Bạn không cần làm gì thêm".

### Luồng: Admin Duyệt khóa học
1. Duyệt khóa học → mở khóa Chờ duyệt (khóa học), xem đủ thông tin và Bài học.
2. Duyệt → khóa thành Đã đăng. Từ chối → modal bắt buộc nhập Lý do từ chối (FR-36, FR-49). Admin tự đăng thì không qua bước này.
- **Loading:** skeleton bảng; nút Duyệt/Từ chối spinner.
- **Empty:** "Không có khóa nào đang chờ duyệt".
- **Error:** Lý do từ chối để trống → lỗi dưới ô, modal không đóng; khóa đã được Admin khác xử lý → toast lỗi + làm mới hàng.
- **Success:** toast "Đã duyệt khóa học" hoặc "Đã từ chối khóa học"; hàng rời khỏi hàng đợi.

### Luồng: Admin Xử lý đơn
1. Đơn → lọc theo trạng thái; Đang chờ và Hết hạn xếp trước [ĐỀ XUẤT]. Mở Đơn: Mã đơn, Học viên, khóa, Nội dung chuyển khoản, Chênh lệch tiền (thừa/thiếu).
2. Chọn "Chuyển sang Đã thanh toán" hoặc "Chuyển sang Đã hủy" → modal nêu hệ quả (Đã thanh toán: tạo Enrollment và Khoản chia; Đã hủy: thu hồi Enrollment nếu có) → xác nhận (FR-33).
- **Loading:** skeleton bảng; nút xác nhận spinner.
- **Empty:** "Chưa có đơn nào" / lọc không ra → "Không có đơn ở trạng thái này".
- **Error:** đơn đã đổi trạng thái do hook hoặc Admin khác → toast "Đơn vừa được cập nhật, vui lòng xem lại" + tải lại chi tiết.
- **Success:** toast "Đơn đã chuyển sang Đã thanh toán/Đã hủy"; badge và lịch sử trạng thái cập nhật.

### Luồng: Admin duyệt Khoản chia và Tạo phiếu chi
1. Khoản chia chờ duyệt → Duyệt → cộng vào Số dư của Instructor (FR-47).
2. Phiếu chi → Tạo phiếu chi: chọn Instructor (hiện Số dư), nhập số tiền → xác nhận; ghi chú cố định "Kế toán chuyển khoản ngoài hệ thống" (FR-48).
- **Loading:** skeleton bảng; nút spinner.
- **Empty:** "Không có Khoản chia chờ duyệt"; "Chưa có Phiếu chi nào".
- **Error:** số tiền > Số dư → lỗi dưới ô "Số tiền vượt Số dư (1.200.000 ₫)", chặn gửi.
- **Success:** toast "Đã duyệt Khoản chia" / "Đã tạo Phiếu chi"; Số dư cập nhật tại chỗ.

## 4. Design tokens [ĐỀ XUẤT - CẦN DUYỆT]

Chưa có brand: baseline sạch, trung tính, một màu nhấn xanh (hợp nội dung học tập). Cỡ chữ và độ tương phản chọn để người dùng cơ bản đọc dễ. Giá trị theo bảng màu Tailwind; kiểm lại contrast khi implement.

| Token | Giá trị | Nền tint | Dùng cho |
|---|---|---|---|
| `primary` | `#2563EB` (trắng trên nền này ≥ 4.5:1) | `#DBEAFE` | Nút chính, link, focus ring, badge Mới |
| `secondary` | `#475569` | `#F1F5F9` | Nút phụ, chữ phụ, badge Chưa đăng/Đã hủy |
| `success` | `#15803D` | `#DCFCE7` | Đã thanh toán, Đã đăng, Sẵn sàng, toast thành công |
| `warning` | `#B45309` | `#FEF3C7` | Chờ duyệt, Đang chờ, Đang xử lý |
| `error` | `#B91C1C` | `#FEE2E2` | Lỗi, Bị từ chối, Hết hạn, nút nguy hiểm |
| `text` / `text-muted` | `#0F172A` / `#475569` | — | Chữ chính / chữ phụ (chữ trên nền tint dùng màu đậm của chính token) |
| `border` / `bg` | `#CBD5E1` / `#F8FAFC` (card `#FFFFFF`) | — | Viền, nền trang |

| Typography | Cỡ / dòng | Dùng cho |
|---|---|---|
| Font | Inter (có Vietnamese subset), qua `next/font` / self-host | Cả hai app |
| `xs` | 12 / 16 | Chú thích, badge |
| `sm` | 14 / 20 | Cỡ nền AdminApp; chữ phụ StudentWeb |
| `base` | 16 / 24 | Cỡ nền StudentWeb, input (≥ 16 để iOS không tự phóng to) |
| `lg` / `xl` | 18 / 28 · 20 / 28 | Tiêu đề thẻ, tiêu đề mục |
| `2xl` / `3xl` | 24 / 32 · 30 / 36 | Tiêu đề trang; tiêu đề Chi tiết khóa học (từ `md` lên) |

| Spacing (4px) | `1`=4 · `2`=8 · `3`=12 · `4`=16 · `6`=24 · `8`=32 · `12`=48 · `16`=64. Khoảng cách trong thẻ `4`, giữa thẻ `6`, giữa mục lớn `12`. Bo góc `8` (nút, input, thẻ), `12` (modal). |
|---|---|

| Breakpoint | `sm` 640 · `md` 768 · `lg` 1024 · `xl` 1280 (Tailwind mặc định); container tối đa 1280 |
|---|---|

Định dạng hiển thị: tiền `1.200.000 ₫`; ngày `dd/MM/yyyy HH:mm`, múi giờ GMT+7 [ĐỀ XUẤT].

## 5. Component pattern

| Component | Hành vi mặc định | Khi dùng loại nào |
|---|---|---|
| Form | Label phía trên input; trường bắt buộc ghi "(bắt buộc)" bằng chữ; kiểm tra khi rời ô và khi gửi; lỗi hiện dưới ô, màu `error` kèm icon; lỗi 400 theo từng trường từ server (FR-26) gắn đúng ô; nút gửi chỉ khóa khi đang gửi (kèm spinner), không khóa chỉ vì form chưa hợp lệ; cuộn và focus vào ô lỗi đầu tiên; rời trang khi chưa lưu thì hỏi | Một cột. Form dài (Khóa học) chia nhóm có tiêu đề, nút "Lưu" dính đáy. Input số tiền: chỉ nhận số, hiển thị phân cách nghìn |
| Table (AdminApp) | Cột đầu là định danh; sắp xếp theo cột; tìm + lọc phía trên; phân trang 20 dòng; trạng thái là Badge; thao tác dòng ở cột cuối (tối đa 2 nút, còn lại vào menu "…"); bấm dòng mở chi tiết | Dữ liệu ≥ 3 cột cần so sánh/lọc. Danh sách khóa học cho Học viên dùng lưới thẻ, không dùng bảng. Lịch sử đơn: bảng từ `md` lên, thẻ xếp dọc dưới `md` |
| Modal (Dialog) | Tiêu đề là câu hỏi hành động ("Hủy đơn này?"); thân nêu hệ quả cụ thể; nút phải đặt tên bằng động từ ("Hủy đơn", "Từ chối khóa"), nút còn lại "Quay lại"; Esc và bấm nền đóng (trừ khi đang gửi); không lồng modal | Chỉ cho: xác nhận hành động khó hoàn tác (Đã hủy, Từ chối, Tạo phiếu chi, sửa khóa Đã đăng), hoặc nhập 1 trường bắt buộc (Lý do từ chối). Nút nguy hiểm dùng `error`. Không dùng modal để báo thành công |
| Toast | Góc dưới phải (mobile: trên cùng), tự tắt sau 4 giây, tối đa 3 cái xếp chồng; lỗi không tự tắt, có nút đóng | Success và lỗi không gắn được vào ô nào: dùng toast. Lỗi trường: dùng lỗi dưới ô. Kết quả quan trọng (Đã thanh toán, lỗi Thanh toán): dùng khối trong trang, không dùng toast |
| Empty state | Icon + 1 câu nói lý do + tối đa 1 nút hành động; không dùng ảnh minh họa nặng | 3 loại: chưa có dữ liệu (nút tạo/duyệt), lọc/tìm không ra (nút "Xóa bộ lọc"), không có quyền (nút về trang trước) |
| Skeleton | Cùng bố cục với nội dung thật, nhấp nháy nhẹ (tắt khi người dùng bật giảm chuyển động); chỉ hiện nếu tải > 300 ms | Tải trang, danh sách, thẻ, chi tiết. Spinner chỉ dùng trong nút và khung Video. Upload dùng thanh tiến độ có % |
| Badge trạng thái | Luôn có chữ (không chỉ màu), tên đúng glossary | Mới `primary` · Đang chờ `warning` · Đã thanh toán `success` · Đã hủy `secondary` · Hết hạn `error` · Chưa đăng `secondary` · Chờ duyệt (khóa học) `warning` · Đã đăng `success` · Bị từ chối `error` · Video: Đang xử lý `warning` (spinner nhỏ), Sẵn sàng `success`, Lỗi `error` · Khoản chia chờ duyệt `warning` |
| Banner trong trang | Khối ngang đầu vùng nội dung, có icon, không tự tắt | Trạng thái cần người dùng chú ý lâu: Lý do từ chối, mất kết nối cập nhật Đơn, đang chờ Admin |

## 6. Accessibility tối thiểu

- **Contrast:** chữ ≥ 4.5:1 với nền; icon và viền input ≥ 3:1. Dùng đúng token mục 4, không tự chọn màu khác. Trạng thái luôn có chữ, không truyền đạt chỉ bằng màu.
- **Keyboard:** mọi nút, link, ô nhập, thẻ khóa học dùng được bằng Tab/Enter/Space; focus ring 2px `primary`, không bỏ outline. Modal giữ focus bên trong, Esc đóng, đóng xong trả focus về nút đã mở. Player Video điều khiển được bằng bàn phím.
- **Label:** mọi input có `<label>` hiển thị, không dùng placeholder thay label; lỗi gắn với ô qua `aria-describedby`; toast và banner lỗi có `aria-live`. Đếm ngược Thanh toán không đọc từng giây (chỉ thông báo khi còn 5 phút và 1 phút).
- **Alt text:** thumbnail khóa = tên khóa học; ảnh trang trí `alt=""`; Mã QR `alt` = "Mã QR thanh toán Đơn {Mã đơn}, số tiền {số tiền}"; nút chỉ có icon phải có `aria-label`.
- **Vùng bấm:** trên StudentWeb mobile tối thiểu 44×44 px.

## 7. Tone of voice

| Khía cạnh | Quy ước |
|---|---|
| Ngôn ngữ | Tiếng Việt, có dấu; không đa ngôn ngữ (ngoài phạm vi MVP) |
| Xưng hô | Web học viên và hướng dẫn cho Instructor: gọi người dùng là "bạn", hệ thống xưng "chúng tôi" khi cần, thường lược chủ ngữ. AdminApp: trung tính, ngắn, không xưng hô ("Đã lưu", "Không thể hủy đơn") |
| Mức trang trọng | Lịch sự, thân thiện, câu ngắn; không tiếng lóng, không từ cảm thán dồn dập, không viết hoa cả câu |
| Thuật ngữ | Chỉ dùng tên ở cột Hiển thị của glossary (ví dụ "Học viên", "Đơn", "Khóa học của tôi"; không "người học", "đơn hàng", "khóa đã mua"). Không dùng thuật ngữ kỹ thuật với Học viên và Instructor |
| Nhãn nút | Động từ + đối tượng, 1-3 từ: "Thêm vào giỏ", "Gửi duyệt", "Tạo phiếu chi"; không "OK", "Submit", "Xác nhận" trơ trọi |

**Thông báo lỗi** — cấu trúc: chuyện gì xảy ra + làm gì tiếp; không đổ lỗi người dùng, không mã HTTP/`errorCode`. Lỗi không tự xử lý được kèm dòng nhỏ "Mã tham chiếu: {CorrelationId}" để báo hỗ trợ (NFR-15).

| Tình huống | Nên | Không nên |
|---|---|---|
| Coupon hết hạn | "Coupon đã hết hạn. Bạn thử Coupon khác nhé." | "Lỗi 400: coupon invalid" |
| Mạng/Server lỗi | "Có sự cố khi tải dữ liệu. Bạn thử lại sau ít phút." | "Internal Server Error" |
| File sai định dạng | "Chỉ nhận video mp4, tối đa 1 GB." | "Invalid file type" |
| Video xử lý Lỗi | "Video này đang gặp sự cố. Bạn có thể Gửi báo lỗi để Admin kiểm tra." | "ffmpeg exited with code 1" |
| Thiếu Bài học khi Gửi duyệt | "Khóa học cần ít nhất 1 Bài học trước khi Gửi duyệt." | "Validation failed" |

**Thông báo hệ thống** (toast, banner, email FR-14/FR-27): thì quá khứ ngắn cho kết quả ("Đã lưu", "Đã thêm vào giỏ hàng"); thì hiện tại cho tiến trình ("Đang xử lý video…"). Email: tiêu đề nêu việc đã xảy ra + Mã đơn nếu có, cùng giọng "bạn", có 1 liên kết hành động (tới Khóa học của tôi hoặc Danh sách khóa học).

## 8. Responsive

| Breakpoint | Web học viên (mobile-first, NFR-03) | App quản trị (desktop-first) |
|---|---|---|
| < 640 | 1 cột; menu thu gọn (hamburger); lưới thẻ 1 cột; Chi tiết khóa học: thanh dính đáy có giá + Thêm vào giỏ/Vào học; Thanh toán: QR rộng tối đa 280 px, Nội dung chuyển khoản và nút sao chép ngay dưới; Trang học: Video 16:9 full chiều ngang, danh sách Bài học bên dưới | Hiện thông báo "Vui lòng dùng máy tính hoặc máy tính bảng để quản trị" kèm nút Đăng xuất [ĐỀ XUẤT] |
| `sm` 640 – `md` 768 | Lưới thẻ 2 cột; Lịch sử đơn dạng thẻ | Dùng được tối thiểu từ 768: sidebar thu thành icon, bảng cuộn ngang |
| `md` 768 – `lg` 1024 | Lưới thẻ 3 cột; Lịch sử đơn dạng bảng; Chi tiết khóa học 2 cột | Sidebar icon, mở rộng khi bấm; form 1 cột |
| ≥ `lg` 1024 | Lưới thẻ 4 cột; Chi tiết khóa học 2 cột với thẻ mua dính khi cuộn; Trang học: Video trái, danh sách Bài học phải | Sidebar mở rộng cố định; bảng đầy đủ cột; form Khóa học 2 cột (thông tin + thumbnail) |

Ảnh thumbnail dùng `next/image` với tỷ lệ 16:9 cố định để không nhảy bố cục (NFR-03). Không có ứng dụng mobile riêng (ngoài phạm vi).
