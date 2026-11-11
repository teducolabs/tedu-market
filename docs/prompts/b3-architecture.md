# B3: Tái dựng `docs/architecture.md`

> Luồng B · Chạy sau: B2 · Model: `opus` · Effort: `xhigh` · Session mới

```text
Đọc `docs/_discovery/` các file 01, 02, 04, 06, 08, 09, 10, 11, 12; `docs/prd.md` (chỉ bảng FR, NFR); `docs/domain-glossary.md`; `docs/decision-logs/README.md`. Mở trực tiếp các file khởi động/DI, file project và entry point của các module chính để kiểm chứng, không chỉ tin file discovery. Viết `docs/architecture.md` mô tả kiến trúc ĐANG CHẠY; không đề xuất kiến trúc mới. Có tài liệu kiến trúc cũ thì hợp nhất; chỗ lệch với code thì code là hiện trạng, tài liệu cũ ghi là "thiết kế ban đầu" kèm [CẦN XÁC NHẬN].

Cấu trúc (giữ đúng tiêu đề):
1. **Kiến trúc tổng quan** — vài đoạn văn + diagram mermaid, vẽ đúng những gì code thể hiện, kể cả chỗ không đẹp (frontend gọi thẳng 2 backend, service đọc chéo database...).
2. **Tech Stack** — bảng `| Thành phần | Phiên bản thực tế | Lý do | Ghi chú |`. Lý do chỉ ghi khi có bằng chứng (link decision log tái dựng), không có thì "không rõ lý do ban đầu". Đánh dấu thành phần hết hỗ trợ/cần nâng cấp.
3. **Module** — theo cấu trúc code thật, mỗi module 1 tiểu mục `### Module: <TênCode>`, không quá ~12 dòng: trách nhiệm, đường dẫn, mã FR, thuật ngữ glossary, phụ thuộc (ghi rõ phụ thuộc vòng hoặc vi phạm phân lớp).
4. **Bảo mật** — authn/authz thực tế, nơi kiểm tra quyền, xử lý secret. Chỗ thiếu ghi vào mục 7.
5. **Performance** — cơ chế đang có (cache, index, async, queue) và con số NFR nếu prd.md có.
6. **Data model tổng quan** — entity chính, tên theo glossary; tên bảng khác thuật ngữ chuẩn thì ghi cả hai.
7. **Rủi ro & nợ kỹ thuật** — 2 nhóm: rủi ro (có thể gây sự cố) và nợ (làm chậm bảo trì); mỗi mục 1 dòng bảng: mô tả, vị trí, mức độ, FR/NFR ảnh hưởng. Liệt kê riêng hotspot (đổi nhiều + không có test) và khu vực nhạy cảm từ input bối cảnh; đây là chỗ agent phải trình kế hoạch và chờ xác nhận trước khi sửa.
8. **Hướng tiến hóa đã thống nhất (to-be)** — để trống, hoặc chỉ ghi hướng tôi đã xác nhận, mỗi hướng có decision log. Tách riêng để agent không nhầm tương lai với hiện trạng.

Định dạng: không quá ~250 dòng. Agent đọc file theo phần (`grep "Module: <Tên>"`), nên giữ đúng tiêu đề `### Module: <TênCode>`. Không chép lại nội dung prd/glossary, chỉ dẫn mã và tên.

Mỗi dấu vết trong 09-decision-traces.md có đủ dữ liệu (rõ đổi từ gì sang gì, khi nào) → tạo 1 decision log tái dựng (`reconstructed: true`) theo `docs/decision-logs/README.md`. Không tạo log cho những gì chỉ là suy đoán.
```
