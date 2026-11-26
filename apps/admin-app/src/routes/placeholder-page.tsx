import { EmptyState } from '@tedu-market/ui';
import { useLocation, useNavigate, useOutletContext } from 'react-router';
import { SIDEBAR_ITEMS, type AdminAppRole } from '../lib/sidebar-items';

/** Trang chờ của mục sidebar; nội dung thật thuộc các thẻ của từng mục. Vai trò không được phép thì báo không có quyền. */
export function PlaceholderPage() {
  const { pathname } = useLocation();
  const navigate = useNavigate();
  const { role } = useOutletContext<{ role: AdminAppRole }>();
  const item = SIDEBAR_ITEMS.find((sidebarItem) => sidebarItem.path === pathname);

  if (item !== undefined && !item.roles.includes(role)) {
    return (
      <EmptyState
        kind="NoPermission"
        message="Bạn không có quyền vào mục này."
        actionLabel="Quay lại"
        onAction={() => void navigate(-1)}
      />
    );
  }
  return <h1 className="text-2xl font-semibold text-text">{item?.label ?? 'Trang quản trị'}</h1>;
}
