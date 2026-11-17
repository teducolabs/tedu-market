---
date: 2026-11-17
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [NFR-17, NFR-23]
cards: [T10]
---
# AdminApp dùng react-router, font Inter qua fontsource và vai trò giả cho khung

## Bối cảnh
T10 dựng khung App quản trị (React + Vite SPA) có sidebar nhiều mục, mỗi mục một route. Tech Stack chưa nêu thư viện routing; ux.md mục 4 yêu cầu font Inter self-host (Vietnamese subset) cho cả hai app, `next/font` chỉ dùng được ở StudentWeb. Chưa có đăng nhập (T26) nên chưa có vai trò thật để chọn mục sidebar.

## Các lựa chọn đã xem xét
- `react-router` (v7, chế độ khai báo `BrowserRouter`): chọn.
- TanStack Router: không chọn, thêm khái niệm sinh route/typegen không cần ở quy mô này.
- Tự viết routing bằng `window.location`: không chọn, mất `NavLink`, trạng thái trang hiện tại, history.
- Font: `@fontsource-variable/inter` (self-host, có subset vietnamese) chọn; tải Google Fonts lúc chạy không chọn vì ux.md yêu cầu self-host.
- Vai trò giả: tham số `?role=Instructor` ngoài bản production chọn; hardcode một vai trò không chọn vì không xem/kiểm thử được sidebar Instructor.

## Quyết định
Thêm `react-router` và `@fontsource-variable/inter` vào `apps/admin-app`; vai trò tạm lấy từ `?role=` khi mode khác `production`, mặc định Admin, cho đến khi T26 thay bằng OIDC.

## Lý do
Cần thư viện routing ổn định cho SPA nhiều màn hình (NFR-17, React Vite). Font self-host đúng ux.md mục 4. Vai trò giả chỉ tác động UI; API vẫn kiểm quyền (FR-05) và bản production bỏ qua tham số.

## Đánh đổi / hệ quả
Thêm 2 dependency ngoài Tech Stack (cần chủ dự án xác nhận). E2E phải build mode `e2e` riêng để đổi vai trò. Đường dẫn route của AdminApp (`/revenue`, `/courses`, `/instructor-profile`, `/course-approvals`, `/orders`, `/instructors`, `/pending-shares`, `/payouts`, `/categories`, `/coupons`, `/video-error-reports`) do T10 đặt vì ux.md chưa quy định.

## Bằng chứng
`apps/admin-app/package.json`, `apps/admin-app/src/lib/fake-session.ts`, `apps/admin-app/src/lib/sidebar-items.ts`.
