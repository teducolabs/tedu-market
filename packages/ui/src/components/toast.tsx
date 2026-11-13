import { useSyncExternalStore } from 'react';
import { toast, Toaster as SonnerToaster } from 'sonner';

const MOBILE_QUERY = '(max-width: 639px)';

function subscribe(onChange: () => void): () => void {
  const query = window.matchMedia(MOBILE_QUERY);
  query.addEventListener('change', onChange);
  return () => {
    query.removeEventListener('change', onChange);
  };
}

function useIsMobile(): boolean {
  return useSyncExternalStore(
    subscribe,
    () => window.matchMedia(MOBILE_QUERY).matches,
    () => false,
  );
}

/** Đặt 1 lần ở gốc app. Góc dưới phải (mobile: trên cùng), tối đa 3 cái xếp chồng (ux.md mục 5). */
export function Toaster() {
  const isMobile = useIsMobile();
  return (
    <SonnerToaster
      position={isMobile ? 'top-center' : 'bottom-right'}
      visibleToasts={3}
      duration={4000}
      closeButton
    />
  );
}

/** Thành công tự tắt sau 4 giây. */
export function showSuccessToast(message: string): void {
  toast.success(message);
}

/** Lỗi không tự tắt, có nút đóng. */
export function showErrorToast(message: string): void {
  toast.error(message, { duration: Infinity, closeButton: true });
}
