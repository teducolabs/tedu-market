'use client';

import { Banner, Button } from '@tedu-market/ui';
import { usePathname, useSearchParams } from 'next/navigation';
import { SIGN_IN_PATH } from '../lib/oidc/oidc-options';
import {
  SIGN_IN_FAILED_PARAM,
  SIGN_IN_FAILED_VALUE,
  withoutSignInFailed,
} from '../lib/oidc/return-path';

/** Banner khi người dùng hủy ở Google hoặc Google lỗi (ux.md, luồng Đăng nhập bằng Google). */
export function SignInFailedBanner() {
  const pathname = usePathname();
  const searchParams = useSearchParams();
  if (searchParams.get(SIGN_IN_FAILED_PARAM) !== SIGN_IN_FAILED_VALUE) {
    return null;
  }
  const returnPath = withoutSignInFailed(`${pathname}?${searchParams.toString()}`);

  return (
    <div className="mb-6">
      <Banner tone="error">
        <p>Đăng nhập chưa hoàn tất. Bạn thử lại nhé</p>
        <form method="get" action={SIGN_IN_PATH} className="mt-2">
          <input type="hidden" name="returnTo" value={returnPath} />
          <Button type="submit" tone="primary">
            Đăng nhập bằng Google
          </Button>
        </form>
      </Banner>
    </div>
  );
}
