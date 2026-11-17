import { PanelLeft } from 'lucide-react';
import { useState } from 'react';
import { NavLink } from 'react-router';
import { getSidebarItems, type AdminAppRole } from '../lib/sidebar-items';

export interface SidebarProps {
  role: AdminAppRole;
}

const LINK_CLASS =
  'flex min-h-9 items-center gap-3 rounded-lg px-3 text-sm font-medium text-secondary-strong hover:bg-secondary-tint';

/**
 * Sidebar trái theo vai trò (ux.md mục 2, 8): từ `lg` mở rộng cố định; dưới `lg` thu thành icon,
 * bấm nút để mở rộng.
 */
export function Sidebar({ role }: SidebarProps) {
  const [isExpanded, setIsExpanded] = useState(false);
  const labelClass = isExpanded ? 'inline' : 'hidden lg:inline';

  return (
    <aside
      className={`${isExpanded ? 'w-60' : 'w-16 lg:w-60'} sticky top-0 h-screen shrink-0 overflow-y-auto border-r border-border bg-card p-2`}
    >
      <button
        type="button"
        aria-label={isExpanded ? 'Thu gọn thanh bên' : 'Mở rộng thanh bên'}
        aria-expanded={isExpanded}
        onClick={() => {
          setIsExpanded((isOpen) => !isOpen);
        }}
        className={`${LINK_CLASS} mb-2 w-full lg:hidden`}
      >
        <PanelLeft aria-hidden className="size-5 shrink-0" />
        <span className={labelClass}>Thu gọn</span>
      </button>
      <nav aria-label="Điều hướng quản trị">
        <ul className="flex flex-col gap-1">
          {getSidebarItems(role).map((item) => (
            <li key={item.path}>
              <NavLink
                to={item.path}
                aria-label={item.label}
                className={({ isActive }) =>
                  `${LINK_CLASS} ${isActive ? 'bg-primary-tint text-primary-strong' : ''}`
                }
              >
                <item.icon aria-hidden className="size-5 shrink-0" />
                <span className={labelClass}>{item.label}</span>
              </NavLink>
            </li>
          ))}
        </ul>
      </nav>
    </aside>
  );
}
