---
date: 2026-11-11
status: Active
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-44, NFR-17]
cards: []
---
# Tìm khóa học bằng full-text search của PostgreSQL

## Bối cảnh
MVP có tìm theo từ khóa (không có trong outline); cần chọn cách tìm.

## Các lựa chọn đã xem xét
- Công cụ tìm kiếm riêng (Elasticsearch...): không chọn, thêm hạ tầng.
- `LIKE`/`ILIKE`: chủ dự án không chọn.
- Full-text search trong PostgreSQL: được chọn.

## Quyết định
Tìm theo từ khóa trên tên và nội dung khóa học bằng full-text search của PostgreSQL.

## Lý do
Chủ dự án chốt "tên khóa học, nội dung khóa học, áp dụng fulltext search trong postgres". Không thêm hạ tầng ngoài PostgreSQL 18.

## Đánh đổi / hệ quả
Chất lượng tìm tiếng Việt (không dấu, tách từ) phụ thuộc cấu hình PostgreSQL, chưa chốt (PRD Q-27).

## Bằng chứng
Câu trả lời của chủ dự án ngày 2026-11-11 (mục 27, lượt 5).
