import { Toaster } from '@tedu-market/ui';
import { Outlet } from 'react-router';
import type { AdminAppRole } from '../lib/sidebar-items';
import { Sidebar } from './sidebar';
import { SmallScreenNotice } from './small-screen-notice';

export interface AdminLayoutProps {
  role: AdminAppRole;
  onSignOut: () => void;
}

/** Khung App quản trị, desktop-first (ux.md mục 8): sidebar + vùng nội dung; dưới `sm` chỉ hiện thông báo. */
export function AdminLayout({ role, onSignOut }: AdminLayoutProps) {
  return (
    <>
      <SmallScreenNotice onSignOut={onSignOut} />
      <div className="hidden min-h-screen sm:flex">
        <Sidebar role={role} />
        <main className="min-w-0 flex-1 overflow-x-auto p-6">
          <Outlet />
        </main>
      </div>
      <Toaster />
    </>
  );
}
