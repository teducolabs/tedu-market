import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { showErrorToast, showSuccessToast } from '@tedu-market/ui';
import { useAuth } from 'react-oidc-context';
import { MemoryRouter, Outlet, Route, Routes } from 'react-router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { changePassword } from '../lib/auth/change-password';
import type { AdminAppRole } from '../lib/sidebar-items';
import { createFakeAuth } from '../test-utils/fake-auth';
import { ChangePasswordPage, SIGN_OUT_AFTER_CHANGE_DELAY_MS } from './change-password-page';

vi.mock('react-oidc-context', () => ({ useAuth: vi.fn() }));
vi.mock('../lib/auth/change-password', () => ({ changePassword: vi.fn() }));
vi.mock('@tedu-market/ui', async (importOriginal) => ({
  ...(await importOriginal<typeof import('@tedu-market/ui')>()),
  showSuccessToast: vi.fn(),
  showErrorToast: vi.fn(),
}));

const renderPage = (role: AdminAppRole = 'Admin') => {
  const auth = createFakeAuth();
  vi.mocked(useAuth).mockReturnValue(auth as never);
  render(
    <MemoryRouter initialEntries={['/change-password']}>
      <Routes>
        <Route element={<Outlet context={{ role }} />}>
          <Route path="/change-password" element={<ChangePasswordPage />} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );
  return auth;
};

async function fillAndSubmit(current: string, next: string) {
  const user = userEvent.setup();
  if (current) await user.type(screen.getByLabelText(/Mật khẩu hiện tại/), current);
  if (next) await user.type(screen.getByLabelText(/Mật khẩu mới/), next);
  await user.click(screen.getByRole('button', { name: 'Lưu' }));
}

describe('ChangePasswordPage', () => {
  beforeEach(() => {
    vi.mocked(useAuth).mockReset();
    vi.mocked(changePassword).mockReset();
    vi.mocked(showSuccessToast).mockReset();
    vi.mocked(showErrorToast).mockReset();
  });

  it('shows no permission for Instructor', () => {
    // Arrange / Act
    renderPage('Instructor');

    // Assert
    expect(screen.getByText('Chỉ Admin được đổi mật khẩu.')).toBeInTheDocument();
    expect(screen.queryByLabelText(/Mật khẩu mới/)).not.toBeInTheDocument();
  });

  it('shows field errors without calling the api when fields are empty', async () => {
    // Arrange
    renderPage();

    // Act
    await fillAndSubmit('', '');

    // Assert
    expect(screen.getByText('Bạn cần nhập mật khẩu hiện tại')).toBeInTheDocument();
    expect(screen.getByText('Bạn cần nhập mật khẩu mới')).toBeInTheDocument();
    expect(changePassword).not.toHaveBeenCalled();
  });

  it('shows the server error under the current password field', async () => {
    // Arrange
    vi.mocked(changePassword).mockResolvedValue({
      isSuccess: false,
      fieldErrors: { currentPassword: 'Mật khẩu hiện tại chưa đúng' },
      message: null,
    });
    renderPage();

    // Act
    await fillAndSubmit('sai', 'Moi-123!');

    // Assert
    expect(await screen.findByText('Mật khẩu hiện tại chưa đúng')).toBeInTheDocument();
    expect(screen.getByLabelText(/Mật khẩu hiện tại/)).toHaveAttribute('aria-invalid', 'true');
    expect(showErrorToast).not.toHaveBeenCalled();
  });

  it('shows an error toast when the failure fits no field', async () => {
    // Arrange
    vi.mocked(changePassword).mockResolvedValue({
      isSuccess: false,
      fieldErrors: {},
      message: 'Có sự cố khi đổi mật khẩu. Bạn thử lại sau ít phút.',
    });
    renderPage();

    // Act
    await fillAndSubmit('cu', 'Moi-123!');

    // Assert
    await waitFor(() => {
      expect(showErrorToast).toHaveBeenCalledWith(
        'Có sự cố khi đổi mật khẩu. Bạn thử lại sau ít phút.',
      );
    });
  });

  it('shows the success toast then signs out so the Admin logs in again', async () => {
    // Arrange
    vi.mocked(changePassword).mockResolvedValue({ isSuccess: true });
    const auth = renderPage();

    // Act
    await fillAndSubmit('cu', 'Moi-123!');

    // Assert
    await waitFor(() => {
      expect(showSuccessToast).toHaveBeenCalledWith('Đã đổi mật khẩu. Bạn đăng nhập lại nhé');
    });
    await waitFor(
      () => {
        expect(auth.signoutRedirect).toHaveBeenCalledOnce();
      },
      { timeout: SIGN_OUT_AFTER_CHANGE_DELAY_MS + 1500 },
    );
  });
});
