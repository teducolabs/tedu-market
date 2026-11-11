---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-35, FR-50, 2026-11-11-upload-video-mp4-1gb-worker-ffmpeg-nen.md]
cards: []
---
# ffmpeg lỗi: hiện thông báo lỗi kèm nút báo lỗi cho Admin

## Bối cảnh
Worker ffmpeg xử lý nền có thể lỗi; cần chốt người dùng thấy gì.

## Các lựa chọn đã xem xét
- Tự động thử lại im lặng: chủ dự án không chọn.
- Hiện thông báo lỗi kèm nút gửi báo lỗi cho Admin: được chọn.

## Quyết định
Khi ffmpeg lỗi, Lesson hiển thị thông báo lỗi kèm nút gửi báo lỗi cho Admin.

## Lý do
Chủ dự án chốt "ffmpeg lỗi thì show thông báo lỗi, kèm nút gửi báo lỗi cho admin".

## Đánh đổi / hệ quả
Admin xử lý lỗi thủ công. Chưa chốt ai thấy nút và Admin nhận báo lỗi ở đâu (PRD Q-26).

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 26, lượt 5).
