import { useAuth } from 'react-oidc-context';
import { Navigate } from 'react-router';
import { FullPageMessage } from '../components/full-page-message';
import { SIGN_IN_PATH } from '../lib/auth/auth-settings';
import { readReturnPath } from '../lib/auth/return-path';

/** Trang `/auth/callback`: react-oidc-context đổi code lấy token; xong thì về trang đang xem trước khi đăng nhập. */
export function AuthCallbackPage() {
  const auth = useAuth();

  if (auth.error) {
    return <Navigate to={SIGN_IN_PATH} replace state={{ hasFailed: true }} />;
  }
  if (auth.isAuthenticated) {
    return <Navigate to={readReturnPath(auth.user?.state)} replace />;
  }
  if (auth.isLoading) {
    return <FullPageMessage message="Đang đăng nhập…" />;
  }
  // Mở thẳng /auth/callback mà không có mã đăng nhập.
  return <Navigate to={SIGN_IN_PATH} replace />;
}
