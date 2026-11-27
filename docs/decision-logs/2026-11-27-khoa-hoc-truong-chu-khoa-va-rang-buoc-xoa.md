---
date: 2026-11-27
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-19, FR-22, FR-18, NFR-08, Course, Category, InstructorProfile, 2026-11-27-category-chi-co-ten-va-quy-tac-xoa-tam.md, 2026-11-26-noi-dung-ho-so-instructor-va-nghia-cua-xoa.md]
cards: [T26]
---
# Khóa học: trường, chủ khóa, ràng buộc xóa Category và Hồ sơ Instructor

## Bối cảnh
T26 tạo thực thể Khóa học nhưng PRD chỉ nói "thông tin, giá, Category"; chưa nói Khóa học gồm trường nào, Admin tạo khóa cho Instructor nào, và T22/T25 để lại hai câu hỏi cho T26: xóa Hồ sơ Instructor còn khóa (kể cả khóa đã có Enrollment) và xóa Category đang có khóa.

## Các lựa chọn đã xem xét
- Trường: `Title` (≤ 200, bắt buộc), `Description` (văn bản thuần ≤ 5000, tùy chọn), `Price` (số nguyên ≥ 0, `numeric(18,0)`), `CategoryId`, `Status`: được chọn (chủ dự án chọn trong phiên T26). Slug sinh từ tiêu đề lúc tạo, duy nhất, không đổi khi sửa tiêu đề (giữ URL ổn định cho SEO; người dùng yêu cầu làm trong T26): được chọn (agent đề xuất cách sinh). Slug đổi theo tiêu đề: không chọn vì gãy liên kết. Thumbnail: thẻ T28.
- Xóa Khóa học: Admin hoặc Instructor sở hữu xóa cứng, mọi trạng thái hiện có (người dùng yêu cầu làm trong T26): được chọn. Chặn xóa khi có Enrollment: chưa làm được vì chưa có Enrollment, thuộc T41.
- Chủ khóa: Admin truyền `instructorProfileId` (bắt buộc), Instructor luôn là hồ sơ của mình, truyền hồ sơ người khác → 403: được chọn (chủ dự án chọn). Chỉ Instructor được tạo: không chọn vì lệch FR-19.
- Xóa Hồ sơ Instructor còn khóa: chặn khi còn bất kỳ khóa nào (mọi trạng thái, kể cả có Enrollment) bằng 400 `INSTRUCTOR_HAS_COURSES`: được chọn (chủ dự án chọn). Chỉ chặn khóa Đã đăng/có Enrollment: không chọn vì để khóa mồ côi.
- Xóa Category còn khóa: chặn bằng 400 `CATEGORY_IN_USE` ở Handler, thêm FK `RESTRICT` làm lớp bảo vệ cuối: được chọn (chủ dự án chọn). Chỉ FK (lỗi 500): không chọn.
- `CourseStatus` hiện chỉ có `Draft`: các giá trị `PendingApproval`, `Published`, `Rejected` thêm ở thẻ duyệt khóa (lưu dạng chuỗi nên không cần đổi schema): agent đề xuất.
- Không đặt FK `courses.instructor_profile_id` tới `instructor_profiles` để module Course không phụ thuộc bảng của Instructor; kiểm tra bằng cổng `IInstructorCourseGate`: agent đề xuất, cần xác nhận.

## Quyết định
Bảng `courses` gồm các trường trên cộng `InstructorProfileId`, `CreatedAt`, `UpdatedAt`. `POST`/`PUT /api/v1/courses` cho Instructor và Admin, `PUT` thay toàn bộ nội dung, không đổi chủ khóa và slug; `DELETE` xóa cứng. Xóa Category hoặc Hồ sơ Instructor còn khóa bị từ chối bằng `DomainException` (400).

## Lý do
FR-19, FR-22, NFR-08 (sở hữu kiểm tra trong Handler); 6.3 (Instructor hỏi Course qua cổng `IInstructorCourseGate`, không đụng bảng Course).

## Đánh đổi / hệ quả
- Race hiếm: Instructor tạo khóa đúng lúc Admin xóa hồ sơ của họ có thể để lại khóa mồ côi vì không có FK tới hồ sơ.
- Muốn xóa Instructor/Category phải xóa hết khóa trước.
- Xóa khóa chưa kiểm tra Enrollment: T41 phải thêm ràng buộc này, nếu không có thể xóa khóa Học viên đã mua.
- Hai request tạo cùng tiêu đề đồng thời có thể cùng chọn một slug: một bên chạm unique index và trả 500 (hiếm).
- Giá chỉ nhận số nguyên; JSON gửi `10.5` bị từ chối.

## Bằng chứng
Trả lời của chủ dự án trong phiên T26 ngày 2026-11-27; code `backend/src/TeduMarket.Domain/Course/Course.cs`, `backend/src/TeduMarket.Application/Course/`, test `CourseTests` (integration).
