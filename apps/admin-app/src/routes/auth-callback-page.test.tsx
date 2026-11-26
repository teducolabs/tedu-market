import { render, screen } from '@testing-library/react';
import { useAuth } from 'react-oidc-context';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createFakeAuth, type FakeAuthOptions } from '../test-utils/fake-auth';
import { AuthCallbackPage } from './auth-callback-page';

vi.mock('react-oidc-context', () => ({ useAuth: vi.fn() }));

function SignInProbe() {
  const location = useLocation();
  const hasFailed = (location.state as { hasFailed?: boolean } | null)?.hasFailed === true;
  return <p>{hasFailed ? 'Đăng nhập thất bại' : 'Màn đăng nhập'}</p>;
}

const renderPage = (options: FakeAuthOptions) => {
  vi.mocked(useAuth).mockReturnValue(createFakeAuth(options) as never);
  render(
    <MemoryRouter initialEntries={['/auth/callback']}>
      <Routes>
        <Route path="/auth/callback" element={<AuthCallbackPage />} />
        <Route path="/sign-in" element={<SignInProbe />} />
        <Route path="/orders" element={<h1>Đơn</h1>} />
      </Routes>
    </MemoryRouter>,
  );
};

describe('AuthCallbackPage', () => {
  beforeEach(() => {
    vi.mocked(useAuth).mockReset();
  });

  it('shows the loading message while the code is exchanged', () => {
    // Arrange / Act
    renderPage({ isLoading: true, isAuthenticated: false });

    // Assert
    expect(screen.getByRole('status')).toHaveTextContent('Đang đăng nhập…');
  });

  it('returns to the page stored in the sign in state', () => {
    // Arrange / Act
    renderPage({ isAuthenticated: true, state: { returnTo: '/orders' } });

    // Assert
    expect(screen.getByRole('heading', { name: 'Đơn' })).toBeInTheDocument();
  });

  it('sends the user back to sign in with a failure flag when the server returns an error', () => {
    // Arrange / Act
    renderPage({ isAuthenticated: false, error: new Error('access_denied') });

    // Assert
    expect(screen.getByText('Đăng nhập thất bại')).toBeInTheDocument();
  });

  it('sends the user to sign in when opened without a code', () => {
    // Arrange / Act
    renderPage({ isAuthenticated: false });

    // Assert
    expect(screen.getByText('Màn đăng nhập')).toBeInTheDocument();
  });
});
