import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { SiteHeader } from './site-header';

const pushMock = vi.fn();
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: pushMock }),
  usePathname: () => '/courses',
  useSearchParams: () => new URLSearchParams('search=react&signIn=failed'),
}));

describe('SiteHeader', () => {
  beforeEach(() => {
    pushMock.mockClear();
  });

  it('shows logo, search, cart, my courses and sign in button', () => {
    // Arrange / Act
    render(<SiteHeader />);

    // Assert
    expect(screen.getByRole('link', { name: 'TEDU Market' })).toHaveAttribute('href', '/');
    expect(screen.getByRole('search')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Giỏ hàng' })).toHaveAttribute('href', '/cart');
    expect(screen.getByRole('link', { name: 'Khóa học của tôi' })).toHaveAttribute(
      'href',
      '/my-courses',
    );
    expect(screen.getByRole('button', { name: 'Đăng nhập bằng Google' })).toBeInTheDocument();
  });

  it('shows course count in cart label when cart is not empty', () => {
    // Arrange / Act
    render(<SiteHeader cartCount={3} />);

    // Assert
    expect(screen.getByRole('link', { name: 'Giỏ hàng, 3 khóa học' })).toBeInTheDocument();
  });

  it('toggles menu button state and closes with Escape', async () => {
    // Arrange
    const user = userEvent.setup();
    render(<SiteHeader />);
    const menuButton = screen.getByRole('button', { name: 'Mở menu' });

    // Act
    await user.click(menuButton);

    // Assert
    expect(screen.getByRole('button', { name: 'Đóng menu' })).toHaveAttribute(
      'aria-expanded',
      'true',
    );
    await user.keyboard('{Escape}');
    expect(screen.getByRole('button', { name: 'Mở menu' })).toHaveAttribute(
      'aria-expanded',
      'false',
    );
  });

  it('navigates to course list with encoded keyword when searching', async () => {
    // Arrange
    const user = userEvent.setup();
    render(<SiteHeader />);

    // Act
    await user.type(screen.getByRole('searchbox', { name: 'Tìm khóa học' }), 'lập trình{Enter}');

    // Assert
    expect(pushMock).toHaveBeenCalledWith('/courses?search=l%E1%BA%ADp%20tr%C3%ACnh');
  });

  it('sends guests to sign in and remembers the current page', () => {
    // Arrange / Act
    render(<SiteHeader />);

    // Assert
    const form = screen.getByRole('button', { name: 'Đăng nhập bằng Google' }).closest('form');
    expect(form).toHaveAttribute('action', '/sign-in');
    expect(form).toHaveAttribute('method', 'get');
    expect(form?.querySelector('input[name="returnTo"]')).toHaveValue('/courses?search=react');
  });

  it('shows account menu with order history and sign out when student is signed in', () => {
    // Arrange / Act
    render(<SiteHeader user={{ name: 'Học viên A', email: 'a@example.com' }} />);

    // Assert
    expect(screen.queryByRole('button', { name: 'Đăng nhập bằng Google' })).not.toBeInTheDocument();
    expect(screen.getByText('Học viên A')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Lịch sử đơn' })).toBeInTheDocument();
    const signOutButton = screen.getByRole('button', { name: 'Đăng xuất' });
    expect(signOutButton.closest('form')).toHaveAttribute('action', '/sign-out');
    expect(signOutButton.closest('form')).toHaveAttribute('method', 'post');
  });
});
