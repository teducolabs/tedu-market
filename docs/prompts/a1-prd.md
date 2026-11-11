# A1: Sinh `docs/prd.md`

> Luồng A · Chạy sau: C1 · Input: [a0-input-requirement.md](a0-input-requirement.md) · Model: `opus`/`opusplan` · Effort: `high`/`xhigh` · Session mới

```text
Bạn là Product Owner kỹ thuật. Viết `docs/prd.md` từ input dưới đây. Không thêm tính năng ngoài input. Input thiếu thông tin cần cho FR/NFR thì hỏi tôi; tôi chưa trả lời được thì đánh [CẦN LÀM RÕ] tại chỗ, không tự giả định.

=== INPUT YÊU CẦU DỰ ÁN ===
[dán nội dung đã điền ở mục Input Requirement vào đây]
=== HẾT INPUT ===

Cấu trúc (đúng thứ tự, đúng tiêu đề):
1. **Context & Problem Statement** — bài toán, vì sao tồn tại, hậu quả nếu không giải quyết. Viết cho kỹ sư mới join, không viết như slide bán hàng.
2. **Đối tượng người dùng** — persona cụ thể, hành vi, mức am hiểu kỹ thuật.
3. **Phạm vi** — bảng 2 cột: Trong phạm vi / Ngoài phạm vi.
4. **Functional Requirements** — bảng `| Mã | Nhóm | Hành vi (user làm gì → hệ thống phản hồi gì) | Ghi chú |`. Mỗi FR là 1 hành vi quan sát được, không viết chung chung kiểu "hệ thống phải linh hoạt".
5. **Non-Functional Requirements** — bảng `| Mã | Loại | Yêu cầu | Ghi chú |`. Tối thiểu có: hiệu năng (số liệu cụ thể), bảo mật (mức tối thiểu chấp nhận được), khả năng mở rộng, uptime nếu liên quan.
6. **Success Metrics** — số liệu đo được.
7. **Open Questions** — gom mọi [CẦN LÀM RÕ].

Định dạng (agent sẽ grep file này theo mã ở mọi task):
- Mỗi FR/NFR đúng 1 dòng bảng, bắt đầu bằng mã (`| FR-03 |`), không xuống dòng trong ô, để `grep "FR-03"` trả về đủ nội dung.
- Mục 1, 2, 3, 6 cộng lại không quá ~60 dòng. Không lặp nội dung giữa các mục.
- Mã FR/NFR không bao giờ đổi (docs khác, decision log, thẻ Kanban tham chiếu tới): thêm sau dùng số tiếp theo, không chèn giữa, không đánh lại.

Phải chọn giữa nhiều cách hiểu hợp lý cho 1 yêu cầu và tôi đã chốt → tạo decision log theo `docs/decision-logs/README.md`.
```
