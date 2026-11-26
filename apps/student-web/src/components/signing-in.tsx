'use client';

import { Loader2 } from 'lucide-react';
import { useEffect, useRef } from 'react';
import { SIGN_IN_COMPLETE_PATH } from '../lib/oidc/oidc-options';

/** Toàn trang "Đang đăng nhập…" lúc quay lại từ API (ux.md); chuyển tiếp `code`/`state` sang route hoàn tất đăng nhập. */
export function SigningIn() {
  const hasStarted = useRef(false);

  useEffect(() => {
    if (hasStarted.current) {
      return;
    }
    hasStarted.current = true;
    // `code` chỉ dùng được một lần nên chỉ chuyển tiếp một lần.
    window.location.replace(`${SIGN_IN_COMPLETE_PATH}${window.location.search}`);
  }, []);

  return (
    <div role="status" className="flex min-h-[50vh] flex-col items-center justify-center gap-3">
      <Loader2
        aria-hidden
        className="size-8 animate-spin text-primary motion-reduce:animate-none"
      />
      <p className="text-lg">Đang đăng nhập…</p>
    </div>
  );
}
