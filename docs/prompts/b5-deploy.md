# B5: Tái dựng `docs/deploy.md`

> Luồng B · Chạy sau: B4 · Model: `opus` · Effort: `xhigh` · Session mới

```text
Đọc `docs/_discovery/08-build-infra.md`, `11-risks.md`, `12-open-questions.md` (câu trả lời về hạ tầng); bảng NFR trong `docs/prd.md`; mục Tổng quan, Tech Stack, Bảo mật trong `docs/architecture.md`. Đọc trực tiếp pipeline CI/CD, Dockerfile, docker-compose, script deploy, IaC, file cấu hình theo môi trường. Viết `docs/deploy.md` mô tả quy trình deploy ĐANG DÙNG, đủ chi tiết để agent làm đúng quy trình đó. Hạ tầng không thể hiện trong repo (server, nơi lưu secret, reverse proxy, cron trên server...) lấy từ input bối cảnh và câu trả lời trong 12-open-questions.md; thiếu thì [CẦN XÁC NHẬN], không đoán hạ tầng production.

Cấu trúc (giữ đúng 10 tiêu đề đánh số):
1. **Môi trường** — các môi trường đang tồn tại thật, khác nhau ở đâu, mục đích.
2. **Hạ tầng** — nơi host từng thành phần.
3. **Biến môi trường/secret** — nơi lưu, cách lấy khi chạy local, bảng TÊN biến bắt buộc. Không chép giá trị secret vào bất kỳ file nào; secret đang nằm trong repo thì ghi vị trí vào mục 10.
4. **Build & CI/CD** — mô tả đúng gate test đang có. Pipeline hiện KHÔNG chặn deploy khi test fail thì ghi đúng như vậy.
5. **Database migration** — cách đang làm thật (tự chạy khi app khởi động, chạy tay, script...). Quy tắc cho mọi thay đổi mới: tương thích ngược với code đang chạy (expand → migrate → contract), vì lúc deploy có thể có instance cũ chạy song song.
6. **Quy trình deploy** — checklist đánh số, có lệnh cụ thể, đúng như đang làm.
7. **Rollback** — quy trình thật nếu có; chưa từng có thì ghi "chưa có" kèm đề xuất tối thiểu [ĐỀ XUẤT - CẦN DUYỆT].
8. **Monitoring sau deploy** — những gì đang có thật để xác nhận hệ thống ổn.
9. **Quy tắc cho agent** — đúng 1 dòng: "Mọi deploy production và mọi lệnh tác động dữ liệu/hạ tầng production cần xác nhận rõ ràng trong phiên hiện tại (xem CLAUDE.md mục 8)."
10. **Khoảng trống vận hành** — thiếu health check, thiếu rollback, bước thủ công dễ sai, secret trong repo... mỗi dòng có vị trí và mức độ.

Định dạng: không quá ~200 dòng. Agent đọc toàn bộ file ở mọi task deploy, nên ưu tiên checklist và lệnh, không giải thích chung chung. Không chép lại nội dung architecture.md.
```
