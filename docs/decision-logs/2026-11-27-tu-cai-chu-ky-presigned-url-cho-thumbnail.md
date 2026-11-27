---
date: 2026-11-27
status: Active
decided_on: 2026-11-27
reconstructed: false
supersedes: []
superseded_by:
related: [FR-23, NFR-03, NFR-22, Course]
cards: [T28]
---
# Tự cài chữ ký presigned URL SigV4 cho thumbnail; giới hạn JPEG/PNG/WebP, 2 MB

## Bối cảnh
T28 cần cấp presigned PUT tới MinIO giới hạn kiểu và dung lượng. Tech Stack không có thư viện S3/MinIO; thêm thư viện cần decision log (conventions 6.16). PRD không nêu kiểu ảnh, dung lượng tối đa, thời hạn url.

## Các lựa chọn đã xem xét
- `AWSSDK.S3`: không chọn, kéo theo phụ thuộc lớn chỉ để ký một loại URL.
- Gói `Minio` (.NET client): không chọn, cùng lý do; thêm khác biệt với S3 thật khi đổi nhà cung cấp.
- Tự cài SigV4 query-signing (~60 dòng, `PresignedUrlSigner`): được chọn, kiểm chứng bằng ví dụ có sẵn trong tài liệu AWS và bằng test tích hợp tính lại chữ ký.
- Presigned POST policy (có `content-length-range`): không chọn, FR-23 và thẻ ghi rõ presigned PUT.

## Quyết định
Tự cài chữ ký presigned URL SigV4 (path-style) và buộc `Content-Type`, `Content-Length` vào chữ ký để MinIO từ chối file sai kiểu hoặc dung lượng. Kiểu cho phép: `image/jpeg`, `image/png`, `image/webp`; dung lượng 1 byte đến 2 MB; url PUT sống 5 phút. API kiểm tra ảnh đã có trong MinIO (HEAD) trước khi lưu khóa vào Khóa học.

## Lý do
Thỏa FR-23 mà không thêm phụ thuộc; 2 MB đủ cho ảnh 16:9 đã nén và giữ LCP (NFR-03). Không nhận SVG/GIF để tránh script nhúng và ảnh động nặng.

## Đánh đổi / hệ quả
Mã mật mã tự duy trì (rủi ro nếu MinIO đổi quy tắc ký; đã có test với vector chuẩn). Giới hạn chặt vì buộc `Content-Length`: client phải khai đúng số byte. Ảnh cũ bị xóa khi thay (cố gắng hết sức: lỗi xóa chỉ ghi log nên có thể còn ảnh mồ côi; xóa Khóa học chưa dọn ảnh). Chưa kiểm tra magic bytes của ảnh đã tải. Bucket cần đọc ẩn danh và CORS cho `PUT` (ghi ở deploy.md).

## Bằng chứng
Chủ dự án xác nhận giới hạn kiểu ảnh, dung lượng và thời hạn url (2026-11-27); đồng ý dọn ảnh cũ khi thay.
