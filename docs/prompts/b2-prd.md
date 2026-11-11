# B2: Tái dựng `docs/prd.md`

> Luồng B · Chạy sau: B1 · Model: `opus` · Effort: `xhigh` · Session mới · Đây là file có độ tin cậy thấp nhất, cần review kỹ nhất.

```text
Đọc `docs/_discovery/03-capabilities.md`, `12-open-questions.md` (kể cả câu trả lời), `docs/domain-glossary.md`, và tài liệu yêu cầu cũ nếu có. Viết `docs/prd.md` mô tả hệ thống ĐANG LÀM GÌ (as-is), không phải nên làm gì. Đã có prd.md hoặc tài liệu yêu cầu cũ thì hợp nhất; tài liệu cũ mô tả tính năng mà code không có (hoặc ngược lại) thì ghi cả hai và gắn [CẦN XÁC NHẬN].

Từ code chỉ suy ra được hành vi, không suy ra được ý đồ nghiệp vụ, persona hay success metric. Những phần đó lấy từ input bối cảnh và câu trả lời trong 12-open-questions.md; thiếu thì [CẦN XÁC NHẬN], không bịa. Danh từ nghiệp vụ dùng đúng glossary.

Cấu trúc (giữ đúng thứ tự và tiêu đề như PRD dự án mới):
1. **Context & Problem Statement** — từ input bối cảnh; thêm "Trạng thái vận hành": chạy từ khi nào (theo git log), đang phục vụ ai.
2. **Đối tượng người dùng** — vai trò lấy từ code phân quyền (role, policy, permission) [TỪ CODE]; hành vi và mức am hiểu kỹ thuật từ input, thiếu thì [CẦN XÁC NHẬN].
3. **Phạm vi** — 2 cột: "Hệ thống đang làm" / "Hệ thống không làm" (việc người dùng tưởng hệ thống làm nhưng thực tế không, hoặc đang làm thủ công ngoài hệ thống).
4. **Functional Requirements** — bảng `| Mã | Module | Hành vi | Vị trí trong code | Nguồn |`, nhóm theo module. Mọi mục trong 03-capabilities.md phải map vào ít nhất 1 FR, hoặc được ghi là kỹ thuật thuần túy (health check, job dọn dẹp...). Hành vi trông như bug thì ghi đúng như code đang làm và gắn [CẦN XÁC NHẬN - nghi lỗi], không tự "sửa" trong mô tả.
5. **Non-Functional Requirements** — bảng `| Mã | Loại | Yêu cầu | Vị trí trong code | Nguồn |`, lấy từ bằng chứng trong code/config: timeout, rate limit, cache TTL, retry, connection pool, CORS, cookie, password policy, token lifetime... Con số cam kết (latency, uptime) không có trong code thì [CẦN XÁC NHẬN], không tự đặt.
6. **Success Metrics** — từ input; thiếu thì [CẦN XÁC NHẬN].
7. **Tính năng chết hoặc nghi chết** — code còn nhưng không có đường vào (route không đăng ký, flag tắt lâu, bảng không ai ghi). Tách riêng, không đánh mã FR.
8. **Open Questions** — gom mọi [CẦN XÁC NHẬN] trong file.

Định dạng (agent sẽ grep file này theo mã ở mọi task):
- Mỗi FR/NFR đúng 1 dòng bảng bắt đầu bằng mã (`| FR-03 |`), không xuống dòng trong ô.
- Mục 1, 2, 3, 6 cộng lại không quá ~60 dòng.
- Mã FR/NFR đã đánh thì không đổi: thêm sau dùng số tiếp theo, không chèn giữa.
```
