import { EmptyState, showErrorToast } from '@tedu-market/ui';
import { useAuth } from 'react-oidc-context';
import { Navigate, useLocation } from 'react-router';
import { resolveAdminAppRole } from '../lib/auth/user-role';
import { SIGN_IN_PATH } from '../lib/auth/auth-settings';
import { AdminLayout } from './admin-layout';
import { FullPageMessage } from './full-page-message';

const SIGN_OUT_ERROR_MESSAGE = 'Có sự cố khi đăng xuất. Bạn thử lại sau ít phút.';

/**
 * Chặn trang quản trị khi chưa đăng nhập (chuyển sang `/sign-in`, quay lại đúng trang sau đó) và khi
 * tài khoản không phải Admin/Instructor (ux.md: không có quyền). Vai trò chỉ quyết định UI; API vẫn kiểm quyền (FR-05).
 */
export function AuthenticatedLayout() {
  const auth = useAuth();
  const location = useLocation();

  const signOut = () => {
    auth.signoutRedirect().catch(() => {
      showErrorToast(SIGN_OUT_ERROR_MESSAGE);
    });
  };

  if (auth.isLoading) {
    return <FullPageMessage message="Đang đăng nhập…" />;
  }
  if (!auth.isAuthenticated) {
    const returnTo = `${location.pathname}${location.search}${location.hash}`;
    return <Navigate to={SIGN_IN_PATH} replace state={{ returnTo }} />;
  }

  const role = resolveAdminAppRole(auth.user?.profile.role);
  if (role === null) {
    return (
      <EmptyState
        kind="NoPermission"
        message="Tài khoản này không có quyền vào App quản trị."
        actionLabel="Đăng xuất"
        onAction={signOut}
      />
    );
  }

  const userName = auth.user?.profile.name ?? auth.user?.profile.email ?? 'Tài khoản';
  return <AdminLayout role={role} userName={userName} onSignOut={signOut} />;
}
