# Nguyên tắc luồng B (đọc trước khi chạy)

Luồng B dùng khi sản phẩm đã chạy production và cần bảo trì, phát triển tiếp bằng Claude Code, nhưng chưa có bộ tài liệu hoặc tài liệu cũ đã lệch xa code. Luồng này đọc source code hiện tại để dựng lại bộ tài liệu cùng cấu trúc với luồng A, nhờ đó hai luồng dùng chung một CLAUDE.md và một cách làm việc.

1. **Code trả lời được "cái gì" và "như thế nào", nhưng không trả lời "tại sao" hay "đáng lẽ phải thế nào".** Nếu code có bug, agent sẽ đọc bug đó như một tính năng. Vì vậy mọi nội dung tái dựng phải gắn nhãn nguồn:
   - `[TỪ CODE]`: thấy trực tiếp trong code/config, kèm đường dẫn file.
   - `[SUY LUẬN]`: suy ra từ nhiều dấu hiệu, chưa có bằng chứng trực tiếp.
   - `[CẦN XÁC NHẬN]`: không đủ dữ liệu hoặc code mâu thuẫn, cần người nắm hệ thống trả lời.

   Nếu bộ tài liệu tái dựng không có nhãn `[CẦN XÁC NHẬN]` nào thì gần như chắc chắn agent đang bịa.
2. **Tách riêng hiện trạng (as-is) và mong muốn (to-be).** Không viết "code đang làm thế này" và "nên làm thế này" trong cùng một đoạn.
3. **Chỉ đọc, không sửa code.** Kể cả khi thấy lỗi rõ ràng, lỗi chỉ được đưa vào danh sách để tạo thẻ Kanban.
4. **Không bịa lý do lịch sử.** Decision log tái dựng chỉ ghi lý do khi có bằng chứng (commit, PR, comment, tài liệu cũ). Không có bằng chứng thì ghi "Không rõ".
5. **Tài liệu cũ là đầu vào, không bị ghi đè.** Khi tài liệu cũ mâu thuẫn với code: code là hiện trạng, tài liệu cũ là "ý định cũ". Ghi cả hai và gắn `[CẦN XÁC NHẬN]`.
6. **Không nhồi repo lớn vào một context.** B0 dùng subagent quét song song theo từng khu vực rồi ghi kết quả ra `docs/_discovery/`, mỗi mục 1 file. Các prompt sau chỉ đọc đúng file mục mình cần và chỉ mở file code cụ thể khi cần kiểm chứng.
7. **Review xong mới thành luật.** Làm toàn bộ luồng trên nhánh riêng (ví dụ `docs/bootstrap`) và chỉ merge sau khi người nắm hệ thống đã review.
