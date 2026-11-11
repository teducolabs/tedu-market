# A5: Sinh `docs/deploy.md`

> Luồng A · Chạy sau: A4 · Model: `opus`/`opusplan` · Effort: `high`/`xhigh` · Session mới

```text
Đọc trong `docs/prd.md` bảng NFR (uptime, bảo mật, compliance), và trong `docs/architecture.md` các mục Tech Stack, Bảo mật. Viết `docs/deploy.md`: hướng dẫn vận hành để agent làm hoặc hỗ trợ deploy đúng quy trình. File phải đáp ứng đúng NFR đã cam kết, không chỉ đúng kỹ thuật (vd NFR uptime 99.9% thì phải có health check và rollback tương ứng). architecture.md chưa nói rõ hosting/infra thì hỏi tôi trước khi viết, không tự chọn, vì đây là quyết định về chi phí thật; tôi chốt xong thì ghi decision log.

Cấu trúc (giữ đúng tiêu đề đánh số):
1. **Môi trường** — local/dev, staging, production: khác nhau ở đâu (config, database, domain), mục đích.
2. **Hạ tầng** — nơi host từng thành phần (API, worker, frontend...), khớp với architecture.md.
3. **Biến môi trường/secret** — nơi lưu (không bao giờ commit vào repo), cách lấy khi chạy local, bảng TÊN biến bắt buộc.
4. **Build & CI/CD** — các bước build; test unit → integration → e2e là gate bắt buộc, tầng nào fail thì pipeline dừng.
5. **Database migration** — cách chạy ở từng môi trường; migration phải reversible hoặc có kế hoạch rollback trước khi lên production.
6. **Quy trình deploy** — checklist đánh số, có lệnh cụ thể: pre-check → build → migrate → deploy → smoke test.
7. **Rollback** — điều kiện phải rollback, các bước cho từng môi trường.
8. **Monitoring sau deploy** — kiểm tra gì ngay sau deploy (log lỗi, health check endpoint, alert).
9. **Quy tắc cho agent** — đúng 1 dòng: "Production chỉ deploy khi người dùng xác nhận rõ ràng trong phiên hiện tại (xem CLAUDE.md mục 8). Staging được tự deploy nếu pipeline cho phép."

Định dạng: không quá ~200 dòng. Agent đọc toàn bộ file này ở mọi task deploy, nên ưu tiên checklist và lệnh, không viết giải thích chung chung. Không chép lại nội dung architecture.md, chỉ dẫn tới mục tương ứng.
```
