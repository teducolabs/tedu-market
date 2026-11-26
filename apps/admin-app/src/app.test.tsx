import { render, screen } from '@testing-library/react';
import type { UserManager } from 'oidc-client-ts';
import { useAuth } from 'react-oidc-context';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { App } from './app';
import { createFakeAuth } from './test-utils/fake-auth';

vi.mock('react-oidc-context', () => ({
  useAuth: vi.fn(),
  AuthProvider: ({ children }: { children: React.ReactNode }) => children,
}));

describe('App', () => {
  beforeEach(() => {
    window.history.pushState({}, '', '/');
  });

  it('redirects a signed in Admin to Doanh thu with the admin sidebar', () => {
    // Arrange
    vi.mocked(useAuth).mockReturnValue(createFakeAuth({ roles: ['Admin'] }) as never);

    // Act
    render(<App userManager={{} as UserManager} />);

    // Assert
    expect(screen.getByRole('heading', { name: 'Doanh thu' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Coupon' })).toBeInTheDocument();
  });

  it('shows the sign in screen to a guest', () => {
    // Arrange
    vi.mocked(useAuth).mockReturnValue(createFakeAuth({ isAuthenticated: false }) as never);

    // Act
    render(<App userManager={{} as UserManager} />);

    // Assert
    expect(screen.getByRole('button', { name: 'Đăng nhập bằng Google' })).toBeInTheDocument();
  });
});
