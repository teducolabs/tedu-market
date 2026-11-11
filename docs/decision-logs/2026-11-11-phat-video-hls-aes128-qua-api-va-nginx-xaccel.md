---
date: 2026-11-11
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-16, FR-34, FR-46, NFR-20, NFR-21]
cards: []
---
# Phát video HLS AES-128, cấp quyền bằng API, Nginx phục vụ file qua X-Accel-Redirect

## Bối cảnh
[Video ở static folder](2026-11-11-video-static-folder-ma-hoa-ffmpeg.md) phải phân quyền ([log](2026-11-11-video-static-folder-phai-phan-quyen.md)), bắt đầu phát ≤ 3 giây (NFR-20), chịu 500 người đồng thời trên 1 instance (NFR-12).

## Các lựa chọn đã xem xét
- mp4 nguyên file + ASP.NET trả file có kiểm tra quyền: không chọn, không mã hóa nội dung được, API gánh toàn bộ băng thông.
- Static folder công khai + chỉ mã hóa: không chọn, vi phạm NFR-21.
- Dịch vụ video ngoài / CDN có DRM: chủ dự án không chọn.
- ffmpeg tạo HLS AES-128 vào thư mục ngoài wwwroot; API cấp token phát ngắn hạn sau khi kiểm tra Enrollment/PreviewLesson/chủ khóa; mọi request manifest, segment, key đi qua API (chỉ kiểm tra chữ ký token, không truy vấn DB) rồi trả X-Accel-Redirect cho Nginx: được chọn.

## Quyết định
Video mã hóa HLS AES-128 lưu ở thư mục không public; truy cập luôn qua endpoint API kiểm tra token, file do Nginx phục vụ nội bộ.

## Lý do
NFR-21 (người không có quyền bị từ chối dù biết URL); NFR-20 nhờ HLS segment ngắn; Nginx gánh băng thông nên 500 đồng thời khả thi hơn.

## Đánh đổi / hệ quả
Token phát chia sẻ được trong thời hạn TTL; AES-128 không phải DRM, người có quyền vẫn có thể tải lại. Dev không có Nginx thì API tự stream file. File Video của bản cũ phải giữ đến khi bản duyệt mới thay thế.

## Bằng chứng
Agent chọn tạm khi viết [docs/architecture.md](../architecture.md); chủ dự án chưa xác nhận. Câu trả lời chủ dự án ngày 2026-11-11 (mục 21) qua hai log liên kết ở trên.
