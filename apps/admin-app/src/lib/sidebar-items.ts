import {
  Banknote,
  BookOpen,
  ChartColumn,
  ClipboardCheck,
  Flag,
  Receipt,
  Tags,
  TicketPercent,
  UserRound,
  Users,
  BadgeCheck,
  type LucideIcon,
} from 'lucide-react';

export type AdminAppRole = 'Admin' | 'Instructor';

export interface SidebarItem {
  label: string;
  path: string;
  icon: LucideIcon;
  roles: readonly AdminAppRole[];
}

const BOTH_ROLES = ['Admin', 'Instructor'] as const;

/** Mục sidebar theo ux.md mục 2 (App quản trị); nhãn lấy từ glossary. API vẫn kiểm quyền (FR-05). */
export const SIDEBAR_ITEMS: readonly SidebarItem[] = [
  { label: 'Doanh thu', path: '/revenue', icon: ChartColumn, roles: BOTH_ROLES },
  { label: 'Khóa học', path: '/courses', icon: BookOpen, roles: BOTH_ROLES },
  {
    label: 'Hồ sơ Instructor',
    path: '/instructor-profile',
    icon: UserRound,
    roles: ['Instructor'],
  },
  { label: 'Duyệt khóa học', path: '/course-approvals', icon: ClipboardCheck, roles: ['Admin'] },
  { label: 'Đơn', path: '/orders', icon: Receipt, roles: ['Admin'] },
  { label: 'Instructor', path: '/instructors', icon: Users, roles: ['Admin'] },
  { label: 'Khoản chia chờ duyệt', path: '/pending-shares', icon: BadgeCheck, roles: ['Admin'] },
  { label: 'Phiếu chi', path: '/payouts', icon: Banknote, roles: ['Admin'] },
  { label: 'Category', path: '/categories', icon: Tags, roles: ['Admin'] },
  { label: 'Coupon', path: '/coupons', icon: TicketPercent, roles: ['Admin'] },
  { label: 'Báo lỗi video', path: '/video-error-reports', icon: Flag, roles: ['Admin'] },
];

export function getSidebarItems(role: AdminAppRole): SidebarItem[] {
  return SIDEBAR_ITEMS.filter((item) => item.roles.includes(role));
}
