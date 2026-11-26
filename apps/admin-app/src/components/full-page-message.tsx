import { Loader2 } from 'lucide-react';

/** Thông báo toàn trang khi đang đăng nhập (ux.md: "Đang đăng nhập…"). */
export function FullPageMessage({ message }: { message: string }) {
  return (
    <div
      role="status"
      className="flex min-h-screen items-center justify-center gap-2 text-base text-text-muted"
    >
      <Loader2 aria-hidden className="size-5 animate-spin" />
      {message}
    </div>
  );
}
