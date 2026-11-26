import type { AdminAppRole } from '../sidebar-items';

function toRoleList(claim: unknown): string[] {
  if (typeof claim === 'string') {
    return [claim];
  }
  if (Array.isArray(claim)) {
    return claim.filter((role): role is string => typeof role === 'string');
  }
  return [];
}

/**
 * Vai trò dùng App quản trị lấy từ claim `role` của id_token. Instructor giữ thêm vai trò Student;
 * Học viên không có quyền vào App quản trị nên trả `null`. API vẫn kiểm quyền (FR-05).
 */
export function resolveAdminAppRole(roleClaim: unknown): AdminAppRole | null {
  const roles = toRoleList(roleClaim);
  if (roles.includes('Admin')) {
    return 'Admin';
  }
  return roles.includes('Instructor') ? 'Instructor' : null;
}
