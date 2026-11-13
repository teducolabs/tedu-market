---
date: 2026-11-11
status: Active
decided_on: 2026-11-13
reconstructed: false
supersedes: []
superseded_by:
related: [NFR-03, NFR-11, NFR-13, NFR-23]
cards: [T07]
---
# Dùng Tailwind CSS + shadcn/ui, chia sẻ qua package `ui` cho cả hai frontend

## Bối cảnh
Chưa có brand guideline hay design system nền. StudentWeb (Next.js 16, SSR/ISR) và AdminApp (React + Vite) nằm chung monorepo pnpm + Turborepo, cần giao diện nhất quán và đạt LCP ≤ 2,5 giây (NFR-03). Cần chọn UI library trước khi sinh UI ([ux.md](../ux.md)).

## Các lựa chọn đã xem xét
- Tailwind CSS v4 + shadcn/ui (Radix UI): được đề xuất.
- MUI: không chọn (CSS-in-JS runtime, bundle nặng, vướng khi dùng với Server Components).
- Ant Design: không chọn (bundle nặng, kiểu dáng riêng khó đưa về design tokens của dự án).
- Mantine/Chakra: không chọn (không có lợi thế so với phương án đề xuất ở quy mô này).

## Quyết định
Tailwind CSS v4 + shadcn/ui, code component đặt ở `packages/ui` dùng chung; design tokens khai báo một lần (xem [ux.md](../ux.md) mục 4). Bổ trợ: `lucide-react` (icon), `sonner` (toast), React Hook Form + Zod (form), TanStack Table (bảng AdminApp).

## Lý do
- Không có runtime CSS-in-JS nên hợp SSR/Server Components và giữ LCP thấp (NFR-03).
- Radix cho sẵn focus trap, keyboard, ARIA cho Dialog/Select/Tabs, giảm công sức đạt mức accessibility tối thiểu.
- Component là source nằm trong repo, sửa theo token dự án được; một package dùng cho cả hai app (NFR-13 cùng tinh thần dùng chung).

## Đánh đổi / hệ quả
- Team tự duy trì code component (không nâng cấp qua npm).
- Bảng dữ liệu phức tạp của AdminApp phải ghép TanStack Table thay vì dùng bảng dựng sẵn.
- Nếu chủ dự án có brand guideline sau này, chỉ cần đổi token, không đổi component.

## Bằng chứng
Chủ dự án đã phê duyệt trong phiên làm việc ngày 13/11/2026. Ràng buộc stack: [architecture.md](../architecture.md) mục 2 (Next.js 16, React Vite, monorepo pnpm + Turborepo).
