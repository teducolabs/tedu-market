import { ChevronUp, CircleUserRound, KeyRound, LogOut } from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router';
import type { AdminAppRole } from '../lib/sidebar-items';

export interface AccountMenuProps {
  role: AdminAppRole;
  userName: string;
  /** Nhãn chỉ hiện khi sidebar mở rộng (cùng quy tắc với các mục sidebar). */
  labelClass: string;
  onSignOut: () => void;
}

const ITEM_CLASS =
  'flex min-h-9 w-full items-center gap-3 rounded-lg px-3 text-sm font-medium text-secondary-strong hover:bg-secondary-tint';

/** Menu tài khoản (ux.md mục 2): Đổi mật khẩu (chỉ Admin, FR-42) và Đăng xuất (FR-04). */
export function AccountMenu({ role, userName, labelClass, onSignOut }: AccountMenuProps) {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className="relative mt-auto border-t border-border pt-2">
      {isOpen ? (
        <ul
          role="menu"
          aria-label="Menu tài khoản"
          className="absolute bottom-full left-0 mb-2 w-56 rounded-lg border border-border bg-card p-1 shadow-md"
        >
          {role === 'Admin' ? (
            <li role="none">
              <Link
                role="menuitem"
                to="/change-password"
                className={ITEM_CLASS}
                onClick={() => {
                  setIsOpen(false);
                }}
              >
                <KeyRound aria-hidden className="size-5 shrink-0" />
                Đổi mật khẩu
              </Link>
            </li>
          ) : null}
          <li role="none">
            <button
              type="button"
              role="menuitem"
              className={ITEM_CLASS}
              onClick={() => {
                setIsOpen(false);
                onSignOut();
              }}
            >
              <LogOut aria-hidden className="size-5 shrink-0" />
              Đăng xuất
            </button>
          </li>
        </ul>
      ) : null}
      <button
        type="button"
        aria-label="Menu tài khoản"
        aria-haspopup="menu"
        aria-expanded={isOpen}
        onClick={() => {
          setIsOpen((wasOpen) => !wasOpen);
        }}
        className={ITEM_CLASS}
      >
        <CircleUserRound aria-hidden className="size-5 shrink-0" />
        <span className={`${labelClass} min-w-0 flex-1 truncate text-left`}>{userName}</span>
        <ChevronUp aria-hidden className={`${labelClass} size-4 shrink-0`} />
      </button>
    </div>
  );
}
