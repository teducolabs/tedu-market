'use client';

import { Button } from '@tedu-market/ui';
import { BookOpen, Menu, Search, ShoppingCart, X } from 'lucide-react';
import Link from 'next/link';
import { useEffect, useId, useState, type SyntheticEvent } from 'react';
import { useRouter } from 'next/navigation';
import { buildSearchUrl, SITE_NAME } from '../lib/site';

export interface SiteHeaderProps {
  /** Số khóa trong Giỏ hàng; nối dữ liệu thật thuộc thẻ T50. */
  cartCount?: number;
}

const NAV_LINK_CLASS =
  'inline-flex min-h-11 items-center gap-2 rounded-lg px-3 text-sm font-medium text-secondary-strong hover:bg-secondary-tint';

/** Header Web học viên (ux.md mục 2, 8): dưới `sm` thu gọn thành menu, từ `sm` hiện đủ một hàng. */
export function SiteHeader({ cartCount = 0 }: SiteHeaderProps) {
  const router = useRouter();
  const menuId = useId();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [keyword, setKeyword] = useState('');

  useEffect(() => {
    if (!isMenuOpen) {
      return;
    }
    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setIsMenuOpen(false);
      }
    };
    document.addEventListener('keydown', closeOnEscape);
    return () => {
      document.removeEventListener('keydown', closeOnEscape);
    };
  }, [isMenuOpen]);

  const submitSearch = (event: SyntheticEvent) => {
    event.preventDefault();
    setIsMenuOpen(false);
    router.push(buildSearchUrl(keyword));
  };

  const cartLabel = cartCount > 0 ? `Giỏ hàng, ${String(cartCount)} khóa học` : 'Giỏ hàng';

  return (
    <header className="sticky top-0 z-40 border-b border-border bg-card">
      <div className="mx-auto flex max-w-7xl flex-wrap items-center gap-x-4 gap-y-2 px-4 py-2 sm:flex-nowrap">
        <Link href="/" className="rounded-lg text-xl font-bold text-primary">
          {SITE_NAME}
        </Link>

        <Button
          tone="secondary"
          className="ml-auto px-3 sm:hidden"
          aria-label={isMenuOpen ? 'Đóng menu' : 'Mở menu'}
          aria-expanded={isMenuOpen}
          aria-controls={menuId}
          onClick={() => {
            setIsMenuOpen((isOpen) => !isOpen);
          }}
        >
          {isMenuOpen ? (
            <X aria-hidden className="size-5" />
          ) : (
            <Menu aria-hidden className="size-5" />
          )}
        </Button>

        <form
          role="search"
          onSubmit={submitSearch}
          className={`${isMenuOpen ? 'flex' : 'hidden'} order-last w-full items-center gap-2 sm:order-none sm:flex sm:w-auto sm:max-w-md sm:flex-1`}
        >
          <label htmlFor={`${menuId}-search`} className="sr-only">
            Tìm khóa học
          </label>
          <input
            id={`${menuId}-search`}
            type="search"
            name="search"
            value={keyword}
            onChange={(event) => {
              setKeyword(event.target.value);
            }}
            placeholder="Tìm khóa học"
            className="min-h-11 w-full rounded-lg border border-border-input bg-card px-3 text-base md:min-h-9"
          />
          <Button type="submit" tone="secondary" aria-label="Tìm kiếm" className="shrink-0 px-3">
            <Search aria-hidden className="size-4" />
          </Button>
        </form>

        <nav
          id={menuId}
          aria-label="Điều hướng chính"
          className={`${isMenuOpen ? 'flex' : 'hidden'} order-last w-full flex-col gap-1 sm:order-none sm:ml-auto sm:flex sm:w-auto sm:flex-row sm:items-center`}
        >
          <Link href="/cart" aria-label={cartLabel} className={NAV_LINK_CLASS}>
            <ShoppingCart aria-hidden className="size-5" />
            <span className="sm:hidden lg:inline">Giỏ hàng</span>
            {cartCount > 0 ? (
              <span className="rounded-full bg-primary-tint px-2 text-xs text-primary-strong">
                {cartCount}
              </span>
            ) : null}
          </Link>
          <Link href="/my-courses" aria-label="Khóa học của tôi" className={NAV_LINK_CLASS}>
            <BookOpen aria-hidden className="size-5" />
            <span className="sm:hidden lg:inline">Khóa học của tôi</span>
          </Link>
          {/* Nút chưa nối OIDC và menu tài khoản (Lịch sử đơn, Đăng xuất): thuộc thẻ T19. */}
          <Button tone="primary">Đăng nhập bằng Google</Button>
        </nav>
      </div>
    </header>
  );
}
