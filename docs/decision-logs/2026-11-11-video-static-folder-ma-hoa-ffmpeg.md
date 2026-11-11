---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-16, FR-34, FR-35, NFR-08, NFR-20, NFR-21]
cards: []
---
# Video Lesson lưu ở static folder, streaming, mã hóa bằng ffmpeg

## Bối cảnh
Lesson là video (outline chưa nói nội dung Lesson là gì). Cần chọn nơi lưu và cách phát video.

## Các lựa chọn đã xem xét
- Object storage tương thích S3 (MinIO): outline dùng cho ảnh thumbnail; không chọn cho video.
- Dịch vụ video bên thứ ba: chủ dự án không nêu.
- File trong static folder của web, phát streaming, mã hóa bằng ffmpeg: được chọn.

## Quyết định
Video Lesson lưu dạng file trong static folder của web, phát thẳng lên web qua streaming và được mã hóa bằng ffmpeg.

## Lý do
Chủ dự án chốt như trên, không nêu lý do.

## Đánh đổi / hệ quả
- File ở static folder có thể bị tải trực tiếp, bỏ qua kiểm tra Enrollment ở API (mâu thuẫn FR-16, NFR-08); cần chốt kiểu mã hóa và cách cấp khóa (PRD Q-21).
- Backup phải gồm cả file video (NFR-19); dung lượng đĩa tăng theo số video.

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 4).
