import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { useAuth } from 'react-oidc-context';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createFakeAuth, type FakeAuthOptions } from '../test-utils/fake-auth';
import { AuthenticatedLayout } from './authenticated-layout';

vi.mock('react-oidc-context', () => ({ useAuth: vi.fn() }));

function SignInProbe() {
  const location = useLocation();
  return <p>Màn đăng nhập, quay lại {(location.state as { returnTo: string }).returnTo}</p>;
}

const renderAt = (path: string, options: FakeAuthOptions) => {
  const auth = createFakeAuth(options);
  vi.mocked(useAuth).mockReturnValue(auth as never);
  render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/sign-in" element={<SignInProbe />} />
        <Route element={<AuthenticatedLayout />}>
          <Route path="/orders" element={<h1>Đơn</h1>} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );
  return auth;
};

describe('AuthenticatedLayout', () => {
  beforeEach(() => {
    vi.mocked(useAuth).mockReset();
  });

  it('shows the loading message while the session is being restored', () => {
    // Arrange / Act
    renderAt('/orders', { isLoading: true, isAuthenticated: false });

    // Assert
    expect(screen.getByRole('status')).toHaveTextContent('Đang đăng nhập…');
  });

  it('sends guests to sign in and remembers the page they opened', () => {
    // Arrange / Act
    renderAt('/orders?page=2', { isAuthenticated: false });

    // Assert
    expect(screen.getByText('Màn đăng nhập, quay lại /orders?page=2')).toBeInTheDocument();
  });

  it('shows the admin sidebar and page for Admin', () => {
    // Arrange / Act
    renderAt('/orders', { roles: ['Admin'] });

    // Assert
    expect(screen.getByRole('heading', { name: 'Đơn' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Coupon' })).toBeInTheDocument();
  });

  it('hides Admin only items from the sidebar for Instructor', () => {
    // Arrange / Act
    renderAt('/orders', { roles: ['Instructor', 'Student'] });

    // Assert
    expect(screen.getByRole('link', { name: 'Hồ sơ Instructor' })).toBeInTheDocument();
    expect(screen.queryByRole('link', { name: 'Coupon' })).not.toBeInTheDocument();
  });

  it('shows no permission with a sign out button for a Student', async () => {
    // Arrange
    const auth = renderAt('/orders', { roles: ['Student'] });

    // Act
    await userEvent.setup().click(screen.getByRole('button', { name: 'Đăng xuất' }));

    // Assert
    expect(screen.getByText('Tài khoản này không có quyền vào App quản trị.')).toBeInTheDocument();
    expect(auth.signoutRedirect).toHaveBeenCalledOnce();
  });

  it('signs out from the account menu', async () => {
    // Arrange
    const auth = renderAt('/orders', { roles: ['Admin'] });
    const user = userEvent.setup();
    await user.click(screen.getByRole('button', { name: 'Menu tài khoản' }));

    // Act
    await user.click(screen.getByRole('menuitem', { name: 'Đăng xuất' }));

    // Assert
    expect(auth.signoutRedirect).toHaveBeenCalledOnce();
  });
});
