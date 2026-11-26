import { Banner, Button, showSuccessToast } from '@tedu-market/ui';
import { useEffect, useState } from 'react';
import { useAuth } from 'react-oidc-context';
import { Navigate, useLocation } from 'react-router';
import { readReturnPath } from '../lib/auth/return-path';

export interface SignInLocationState {
  returnTo?: string;
  /** Đăng nhập chưa hoàn tất (hủy ở Google hoặc lỗi) → hiện banner thử lại. */
  hasFailed?: boolean;
  /** Vừa đổi mật khẩu → toast rồi đăng nhập lại. */
  hasChangedPassword?: boolean;
}

const FAILED_MESSAGE = 'Đăng nhập chưa hoàn tất. Bạn thử lại nhé';
const PASSWORD_CHANGED_MESSAGE = 'Đã đổi mật khẩu. Bạn đăng nhập lại nhé';

/**
 * Màn hình đăng nhập của App quản trị: Instructor bấm Đăng nhập bằng Google, Admin bấm Đăng nhập bằng
 * email (chuyển sang `/account/login` của Authorization Server, FR-04).
 */
export function SignInPage() {
  const auth = useAuth();
  const location = useLocation();
  const state = (location.state ?? {}) as SignInLocationState;
  const returnTo = readReturnPath(state);
  const [hasStartFailed, setHasStartFailed] = useState(false);
  const hasChangedPassword = state.hasChangedPassword === true;

  useEffect(() => {
    if (hasChangedPassword) {
      showSuccessToast(PASSWORD_CHANGED_MESSAGE);
    }
  }, [hasChangedPassword]);

  if (auth.isAuthenticated) {
    return <Navigate to={returnTo} replace />;
  }

  const startSignIn = (extraQueryParams?: Record<string, string>) => {
    setHasStartFailed(false);
    auth
      .signinRedirect({ state: { returnTo }, ...(extraQueryParams ? { extraQueryParams } : {}) })
      .catch(() => {
        setHasStartFailed(true);
      });
  };

  return (
    <main className="mx-auto flex min-h-screen max-w-sm flex-col justify-center gap-6 px-4">
      <h1 className="text-2xl font-semibold text-text">Đăng nhập App quản trị</h1>
      {state.hasFailed === true || hasStartFailed ? (
        <Banner tone="error">{FAILED_MESSAGE}</Banner>
      ) : null}
      <div className="flex flex-col gap-3">
        <Button
          onClick={() => {
            startSignIn({ identity_provider: 'google' });
          }}
        >
          Đăng nhập bằng Google
        </Button>
        <Button
          tone="secondary"
          onClick={() => {
            startSignIn();
          }}
        >
          Đăng nhập bằng email
        </Button>
      </div>
      <p className="text-sm text-text-muted">
        Instructor đăng nhập bằng Google. Admin đăng nhập bằng email và mật khẩu.
      </p>
    </main>
  );
}
