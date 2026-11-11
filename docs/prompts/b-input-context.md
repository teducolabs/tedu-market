# Input bối cảnh (luồng B, điền trước khi chạy B0)

> Đây là input anh tự điền, không phải prompt. Điền xong thì dán vào chỗ đánh dấu trong [B0](b0-codebase-scan.md).

```text
## Bối cảnh dự án đang vận hành (điền tay)
- Tên sản phẩm:
- Sản phẩm đang phục vụ ai, giải quyết việc gì (vài câu):
- Phạm vi repo cần tài liệu hóa (toàn repo, hay chỉ thư mục X, Y; thư mục nào bỏ qua: vendor, generated code, thư viện copy vào...):
- Các thành phần triển khai độc lập (API, worker, web, mobile, admin...):
- Tài liệu cũ đang có ở đâu (README, wiki, ADR, file thiết kế...). Tài liệu nằm ngoài repo thì export và đặt vào docs/_discovery/legacy-docs/:
- Khu vực legacy/nhạy cảm đã biết là "đừng động vào nếu không bắt buộc", và lý do:
- Vấn đề đang gặp khi bảo trì (bug lặp lại, phần khó sửa, phần hay vỡ khi deploy...):
- Hạ tầng thật mà code không thể hiện (nơi host, nơi lưu secret, reverse proxy, cron trên server, dịch vụ bên ngoài...):
- Công cụ Kanban và format mã thẻ:
- Người nắm lịch sử hệ thống (để biết hỏi ai khi gặp [CẦN XÁC NHẬN]):
```
