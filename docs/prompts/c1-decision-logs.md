# C1: Dựng thư mục `docs/decision-logs/`

> Dùng chung cho cả 2 luồng, chạy **đầu tiên** (luồng A) hoặc ngay sau Input bối cảnh (luồng B) · **Không cần agent, không tốn token.**
>
> Nội dung `README.md` và `_template.md` là cố định, nên copy thẳng từ [templates/decision-logs/](templates/decision-logs/) thay vì để model chép lại. Cách này cho kết quả giống nhau ở mọi dự án và không tốn token đầu ra.
>
> Mỗi quyết định là một file riêng để nhiều nhánh cùng thêm quyết định mà không conflict, mỗi quyết định có link riêng cho thẻ/PR, và `git log` trên một file cho thấy đúng lịch sử của quyết định đó.

Chạy ở gốc repo dự án (thay `<KIT>` bằng đường dẫn tới thư mục `docs/prompts` của bộ prompt này):

```text
mkdir -p docs/decision-logs
cp <KIT>/templates/decision-logs/README.md    docs/decision-logs/README.md
cp <KIT>/templates/decision-logs/_template.md docs/decision-logs/_template.md
git add docs/decision-logs && git commit -m "docs: dựng decision-logs"
```

Nếu repo đã có `docs/decision-logs/README.md` thì so sánh bằng tay trước khi ghi đè.

Thư mục này được ghi tiếp trong suốt đời dự án; `CLAUDE.md` (bước C2) biến việc ghi vào đây thành quy tắc bắt buộc.
