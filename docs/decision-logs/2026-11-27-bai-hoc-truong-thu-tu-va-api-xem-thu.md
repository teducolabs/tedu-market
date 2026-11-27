---
date: 2026-11-27
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-20, FR-45, NFR-08, Lesson, PreviewLesson, Course, 2026-11-27-khoa-hoc-truong-chu-khoa-va-rang-buoc-xoa.md]
cards: [T27]
---
# Bài học: trường, thứ tự và hình dạng API sắp xếp lại / xem thử

## Bối cảnh
T27 quản lý Bài học trong Khóa học nhưng PRD chỉ nói "gắn 1 video, có thứ tự". Chưa chốt Bài học gồm trường nào, cách sắp xếp lại, cách đánh dấu Bài học xem thử, và nơi trả cờ xem thử khi chi tiết khóa công khai (T31) chưa có.

## Các lựa chọn đã xem xét
- Trường: `LessonTitle` (bắt buộc ≤ 200) + `LessonOrder` + `IsPreview` (mặc định false): được chọn (chủ dự án chọn). Thêm mô tả bài học: không chọn, chưa có yêu cầu. Video gắn ở T38.
- Trả cờ xem thử: `GET /courses/{courseId}/lessons` cho Admin/chủ khóa: được chọn (chủ dự án chọn). `GET /courses/{id}` kèm bài học: không chọn. Hiển thị trên chi tiết khóa công khai để T31.
- Sắp xếp lại: `PUT .../lessons/order` nhận đủ danh sách `lessonIds`; đánh dấu xem thử `PUT .../lessons/{lessonId}/preview` riêng: được chọn (chủ dự án chọn). Gộp cờ vào `PUT` sửa bài: không chọn.
- Thêm bài luôn vào cuối khóa; xóa bài thì dồn lại `LessonOrder` liên tục từ 1: agent đề xuất, cần xác nhận.
- Index `(course_id, lesson_order)` không unique, vì cập nhật nhiều dòng trong một `SaveChanges` có thể tạm trùng thứ tự: agent đề xuất, cần xác nhận.

## Quyết định
Bảng `lessons` gồm `lesson_id`, `course_id` (FK CASCADE), `lesson_title`, `lesson_order`, `is_preview`, `created_at`, `updated_at`. Các endpoint dưới `/api/v1/courses/{courseId}/lessons` cho Admin và Instructor sở hữu khóa; Bài học không thuộc khóa trên đường dẫn trả 404.

## Lý do
FR-20, FR-45 (Admin hoặc Instructor sở hữu khóa được đánh dấu), NFR-08 (kiểm tra sở hữu ở Handler).

## Đánh đổi / hệ quả
- Hai request thêm bài đồng thời có thể cùng nhận một `LessonOrder` (không unique); lần sắp xếp lại kế tiếp sửa lại.
- Sắp xếp lại bắt buộc gửi đủ danh sách: client phải có danh sách mới nhất, nếu không nhận 400.
- Xóa khóa xóa luôn bài học; chưa kiểm tra Enrollment (xem log T26, T41 xử lý).
- Cờ xem thử chưa lộ cho Khách cho tới T31.

## Bằng chứng
Trả lời của chủ dự án trong phiên T27 ngày 2026-11-27; code `backend/src/TeduMarket.Domain/Course/Lesson.cs`, `backend/src/TeduMarket.Application/Course/Commands/`, test `LessonTests` (integration).
