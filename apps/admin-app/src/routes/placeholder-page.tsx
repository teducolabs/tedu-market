import { useLocation } from 'react-router';
import { SIDEBAR_ITEMS } from '../lib/sidebar-items';

/** Trang chờ của mục sidebar; nội dung thật thuộc các thẻ của từng mục. */
export function PlaceholderPage() {
  const { pathname } = useLocation();
  const title = SIDEBAR_ITEMS.find((item) => item.path === pathname)?.label ?? 'Trang quản trị';

  return <h1 className="text-2xl font-semibold text-text">{title}</h1>;
}
