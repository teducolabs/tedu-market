'use client';

import { showSuccessToast } from '@tedu-market/ui';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import { useEffect, useRef } from 'react';
import { WELCOME_PARAM, withoutWelcome } from '../lib/oidc/return-path';

export const WELCOME_MESSAGE = 'Chào mừng bạn đến TEDU Market';

/** Toast lần đăng nhập Google đầu tiên (ux.md); bỏ `welcome` khỏi URL để tải lại không hiện lại. */
export function WelcomeToast() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const hasShown = useRef(false);
  const isWelcome = searchParams.has(WELCOME_PARAM);

  useEffect(() => {
    if (!isWelcome || hasShown.current) {
      return;
    }
    hasShown.current = true;
    showSuccessToast(WELCOME_MESSAGE);
    router.replace(withoutWelcome(`${pathname}?${searchParams.toString()}`));
  }, [isWelcome, pathname, router, searchParams]);

  return null;
}
