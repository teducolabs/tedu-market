---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-16, NFR-08, NFR-21, 2026-11-11-video-static-folder-ma-hoa-ffmpeg.md]
cards: []
---
# Video ở static folder phải phân quyền truy cập

## Bối cảnh
[Quyết định lưu video ở static folder](2026-11-11-video-static-folder-ma-hoa-ffmpeg.md) làm file có thể bị tải trực tiếp, bỏ qua kiểm tra Enrollment ở API.

## Các lựa chọn đã xem xét
- Để static folder công khai, chỉ dựa vào mã hóa: không chọn, vì ai cũng truy cập được.
- Chuyển video sang object storage hoặc dịch vụ ngoài: chủ dự án không chọn.
- Giữ static folder nhưng phân quyền truy cập: được chọn.

## Quyết định
Vẫn lưu video ở static folder, nhưng chỉ người có quyền (có Enrollment) mới truy cập được.

## Lý do
Chủ dự án chốt "dùng static folder nhưng phải phân quyền, không phải ai cũng truy cập được". Phù hợp NFR-08 (kiểm tra quyền ở API, không chỉ ẩn UI).

## Đánh đổi / hệ quả
Request video phải đi qua bước kiểm tra quyền; cơ chế cụ thể thuộc architecture. Tải stream chịu thêm chi phí kiểm tra (NFR-20).

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 21).
