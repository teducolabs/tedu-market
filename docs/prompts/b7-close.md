# B7: Đóng quá trình tái dựng

> Luồng B · Chạy sau: C2 · Model: `opus` · Effort: `high` · Session mới

```text
Nhiệm vụ: đóng lại quá trình tái dựng tài liệu. Không sửa code. Không đọc toàn bộ docs/; chỉ dùng grep và đọc đúng các mục nêu dưới.

1. `grep -rn "CẦN XÁC NHẬN\|CẦN LÀM RÕ\|ĐỀ XUẤT - CẦN DUYỆT" docs/ CLAUDE.md --exclude-dir=_discovery`, rồi in 1 bảng: file, mục, nội dung cần xác nhận, nên hỏi ai. Không tự giải quyết.

2. Gom việc cần tạo thẻ Kanban từ đúng các nguồn sau (dùng grep tìm dòng tiêu đề rồi chỉ Read đoạn đó): mục 7 "Rủi ro & nợ kỹ thuật" của `docs/architecture.md`, mục 9 "Khoảng trống UX" của `docs/ux.md`, mục 10 "Khoảng trống vận hành" của `docs/deploy.md`, mục "Việc cần tạo thẻ" của `docs/conventions.md` (nếu có), `docs/_discovery/11-risks.md`, `docs/_discovery/13-terminology-debt.md`. Gộp mục trùng. Mỗi việc 1 dòng bảng: tiêu đề thẻ đề xuất, mô tả 1-2 câu, vị trí, ưu tiên đề xuất (Cao cho rủi ro bảo mật hoặc mất dữ liệu; nợ thuật ngữ lộ ra API công khai hoặc schema DB phải ghi rõ cần kế hoạch migration và tương thích ngược). Xuất ra chat; tôi yêu cầu thì xuất thêm file tạm `backlog-import.csv` (KHÔNG commit). Có MCP tới công cụ Kanban thì chỉ tạo thẻ khi tôi xác nhận.

3. `grep -rn "_discovery" docs/ CLAUDE.md --exclude-dir=_discovery`: các file lâu dài không được tham chiếu tới file tạm. Có thì báo tôi.

4. Có `docs/_discovery/legacy-docs/` thì hỏi tôi có cần giữ không; cần giữ thì chuyển sang `docs/legacy/` trước khi xóa.

5. Chỉ sau khi tôi xác nhận đã xử lý xong bước 1 và 2: xóa thư mục `docs/_discovery/`. Không xóa trước khi có xác nhận.
```
