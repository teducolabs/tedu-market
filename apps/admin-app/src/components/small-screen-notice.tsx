import { Button } from '@tedu-market/ui';
import { Monitor } from 'lucide-react';

export interface SmallScreenNoticeProps {
  onSignOut: () => void;
}

/** Dưới `sm` (640px) App quản trị không dùng được: hiện thông báo và nút Đăng xuất (ux.md mục 8). */
export function SmallScreenNotice({ onSignOut }: SmallScreenNoticeProps) {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 px-4 text-center sm:hidden">
      <Monitor aria-hidden className="size-10 text-text-muted" />
      <p role="status" className="text-base text-text">
        Vui lòng dùng máy tính hoặc máy tính bảng để quản trị
      </p>
      <Button tone="secondary" onClick={onSignOut}>
        Đăng xuất
      </Button>
    </div>
  );
}
