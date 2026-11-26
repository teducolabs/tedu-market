import { render, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { SignInFailedBanner } from './sign-in-failed-banner';

let currentSearch = '';
vi.mock('next/navigation', () => ({
  usePathname: () => '/cart',
  useSearchParams: () => new URLSearchParams(currentSearch),
}));

describe('SignInFailedBanner', () => {
  beforeEach(() => {
    currentSearch = '';
  });

  it('shows nothing when sign in did not fail', () => {
    // Arrange / Act
    render(<SignInFailedBanner />);

    // Assert
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });

  it('shows message and retry button that returns to the same page without the flag', () => {
    // Arrange
    currentSearch = 'signIn=failed';

    // Act
    render(<SignInFailedBanner />);

    // Assert
    expect(screen.getByRole('alert')).toHaveTextContent('Đăng nhập chưa hoàn tất. Bạn thử lại nhé');
    expect(screen.getByRole('button', { name: 'Đăng nhập bằng Google' })).toBeInTheDocument();
    const returnTo = document.querySelector<HTMLInputElement>('input[name="returnTo"]');
    expect(returnTo?.value).toBe('/cart');
  });
});
