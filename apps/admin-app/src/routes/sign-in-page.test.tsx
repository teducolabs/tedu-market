import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { useAuth } from 'react-oidc-context';
import { MemoryRouter, Route, Routes } from 'react-router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createFakeAuth, type FakeAuthOptions } from '../test-utils/fake-auth';
import { SignInPage, type SignInLocationState } from './sign-in-page';

vi.mock('react-oidc-context', () => ({ useAuth: vi.fn() }));

const renderPage = (
  options: FakeAuthOptions = { isAuthenticated: false },
  state?: SignInLocationState,
) => {
  const auth = createFakeAuth(options);
  vi.mocked(useAuth).mockReturnValue(auth as never);
  render(
    <MemoryRouter initialEntries={[{ pathname: '/sign-in', state }]}>
      <Routes>
        <Route path="/sign-in" element={<SignInPage />} />
        <Route path="/coupons" element={<h1>Coupon</h1>} />
        <Route path="/" element={<h1>Trang chủ</h1>} />
      </Routes>
    </MemoryRouter>,
  );
  return auth;
};

describe('SignInPage', () => {
  beforeEach(() => {
    vi.mocked(useAuth).mockReset();
  });

  it('starts Google sign in with the page to return to', async () => {
    // Arrange
    const auth = renderPage(undefined, { returnTo: '/coupons' });

    // Act
    await userEvent.setup().click(screen.getByRole('button', { name: 'Đăng nhập bằng Google' }));

    // Assert
    expect(auth.signinRedirect).toHaveBeenCalledWith({
      state: { returnTo: '/coupons' },
      extraQueryParams: { identity_provider: 'google' },
    });
  });

  it('starts email sign in without the Google provider', async () => {
    // Arrange
    const auth = renderPage();

    // Act
    await userEvent.setup().click(screen.getByRole('button', { name: 'Đăng nhập bằng email' }));

    // Assert
    expect(auth.signinRedirect).toHaveBeenCalledWith({ state: { returnTo: '/' } });
  });

  it('shows the retry banner when sign in did not complete', () => {
    // Arrange / Act
    renderPage(undefined, { hasFailed: true });

    // Assert
    expect(screen.getByRole('alert')).toHaveTextContent('Đăng nhập chưa hoàn tất. Bạn thử lại nhé');
  });

  it('shows the retry banner when the redirect cannot start', async () => {
    // Arrange
    const auth = renderPage();
    auth.signinRedirect.mockRejectedValue(new Error('offline'));

    // Act
    await userEvent.setup().click(screen.getByRole('button', { name: 'Đăng nhập bằng Google' }));

    // Assert
    expect(await screen.findByRole('alert')).toHaveTextContent('Đăng nhập chưa hoàn tất');
  });

  it('goes back to the requested page when already signed in', () => {
    // Arrange / Act
    renderPage({ isAuthenticated: true }, { returnTo: '/coupons' });

    // Assert
    expect(screen.getByRole('heading', { name: 'Coupon' })).toBeInTheDocument();
  });
});
