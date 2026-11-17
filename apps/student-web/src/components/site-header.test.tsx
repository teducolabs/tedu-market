import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { SiteHeader } from './site-header';

const pushMock = vi.fn();
vi.mock('next/navigation', () => ({ useRouter: () => ({ push: pushMock }) }));

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
});
