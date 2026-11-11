---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-35, NFR-21, 2026-11-11-video-static-folder-ma-hoa-ffmpeg.md]
cards: []
---
# Upload video: chỉ mp4 tối đa 1 GB, ffmpeg xử lý nền qua worker

## Bối cảnh
Cần chốt định dạng, dung lượng và cách chạy ffmpeg khi Instructor upload video.

## Các lựa chọn đã xem xét
- Xử lý ffmpeg đồng bộ trong request upload: không chọn (file lớn, dễ timeout).
- Xử lý nền qua worker: được chọn.

## Quyết định
Chỉ nhận mp4, tối đa 1 GB; mã hóa bằng ffmpeg chạy nền qua worker.

## Lý do
Chủ dự án chốt "mp4, max 1GB, xử lý ffmpeg nền qua worker". Không nêu lý do thêm.

## Đánh đổi / hệ quả
Cần worker và trạng thái xử lý cho Lesson; trạng thái hiển thị khi đang xử lý hoặc lỗi chưa chốt (PRD Q-26). Dung lượng đĩa và backup tăng (NFR-19).

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 21, lượt 3).
