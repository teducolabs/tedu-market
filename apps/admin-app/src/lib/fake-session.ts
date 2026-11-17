import type { AdminAppRole } from './sidebar-items';

const DEFAULT_FAKE_ROLE: AdminAppRole = 'Admin';

/**
 * Vai trò giả cho khung App quản trị (chưa có đăng nhập). Ngoài bản production, `?role=Instructor`
 * đổi sang Instructor để xem sidebar. Bản production luôn là Admin.
 * TODO(T26): thay bằng vai trò thật từ OIDC.
 */
export function resolveFakeRole(search: string, mode: string): AdminAppRole {
  if (mode === 'production') {
    return DEFAULT_FAKE_ROLE;
  }
  const requestedRole = new URLSearchParams(search).get('role');
  return requestedRole === 'Instructor' ? 'Instructor' : DEFAULT_FAKE_ROLE;
}
