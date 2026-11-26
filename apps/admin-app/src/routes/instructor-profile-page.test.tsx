import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { showErrorToast, showSuccessToast } from '@tedu-market/ui';
import { MemoryRouter, Outlet, Route, Routes } from 'react-router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { getMyInstructorProfile, updateInstructorProfile } from '../lib/instructors/instructor-api';
import type { AdminAppRole } from '../lib/sidebar-items';
import { InstructorProfilePage } from './instructor-profile-page';

vi.mock('../lib/instructors/instructor-api', () => ({
  getMyInstructorProfile: vi.fn(),
  updateInstructorProfile: vi.fn(),
}));
vi.mock('@tedu-market/ui', async (importOriginal) => ({
  ...(await importOriginal<typeof import('@tedu-market/ui')>()),
  showSuccessToast: vi.fn(),
  showErrorToast: vi.fn(),
}));

const profile = { instructorProfileId: 'p1', displayName: '', biography: '', avatarUrl: '' };

const renderPage = (role: AdminAppRole = 'Instructor') =>
  render(
    <MemoryRouter initialEntries={['/instructor-profile']}>
      <Routes>
        <Route element={<Outlet context={{ role }} />}>
          <Route path="/instructor-profile" element={<InstructorProfilePage />} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );

describe('InstructorProfilePage', () => {
  beforeEach(() => {
    vi.mocked(getMyInstructorProfile).mockReset();
    vi.mocked(updateInstructorProfile).mockReset();
    vi.mocked(showSuccessToast).mockReset();
    vi.mocked(showErrorToast).mockReset();
  });

  it('shows no permission for Admin without loading the profile', () => {
    // Arrange / Act
    renderPage('Admin');

    // Assert
    expect(screen.getByText('Bạn không có quyền vào mục này.')).toBeInTheDocument();
    expect(getMyInstructorProfile).not.toHaveBeenCalled();
  });

  it('shows error and retries when loading fails', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(getMyInstructorProfile).mockResolvedValueOnce({ isSuccess: false });
    vi.mocked(getMyInstructorProfile).mockResolvedValueOnce({
      isSuccess: true,
      value: { ...profile, displayName: 'Cô Lan' },
    });
    renderPage();

    // Act
    await user.click(await screen.findByRole('button', { name: 'Thử lại' }));

    // Assert
    expect(await screen.findByLabelText(/Tên hiển thị/)).toHaveValue('Cô Lan');
  });

  it('shows required error without calling the api when display name is empty', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(getMyInstructorProfile).mockResolvedValue({ isSuccess: true, value: profile });
    renderPage();
    await screen.findByLabelText(/Tên hiển thị/);

    // Act
    await user.click(screen.getByRole('button', { name: 'Lưu' }));

    // Assert
    expect(await screen.findByText('Bạn cần nhập Tên hiển thị')).toBeInTheDocument();
    expect(updateInstructorProfile).not.toHaveBeenCalled();
  });

  it('saves own profile by its id and shows the saved toast', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(getMyInstructorProfile).mockResolvedValue({ isSuccess: true, value: profile });
    vi.mocked(updateInstructorProfile).mockResolvedValue({ isSuccess: true });
    renderPage();

    // Act
    await user.type(await screen.findByLabelText(/Tên hiển thị/), 'Cô Hoa');
    await user.click(screen.getByRole('button', { name: 'Lưu' }));

    // Assert
    await waitFor(() => {
      expect(updateInstructorProfile).toHaveBeenCalledWith('p1', {
        displayName: 'Cô Hoa',
        biography: '',
        avatarUrl: '',
      });
    });
    expect(showSuccessToast).toHaveBeenCalledWith('Đã lưu');
  });

  it('shows field error under the input when server returns 400', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(getMyInstructorProfile).mockResolvedValue({
      isSuccess: true,
      value: { ...profile, displayName: 'Cô Lan' },
    });
    vi.mocked(updateInstructorProfile).mockResolvedValue({
      isSuccess: false,
      fieldErrors: { avatarUrl: 'Phải là đường dẫn http hoặc https hợp lệ' },
      message: null,
    });
    renderPage();
    await screen.findByLabelText(/Tên hiển thị/);

    // Act
    await user.click(screen.getByRole('button', { name: 'Lưu' }));

    // Assert
    expect(await screen.findByText('Phải là đường dẫn http hoặc https hợp lệ')).toBeInTheDocument();
    expect(showSuccessToast).not.toHaveBeenCalled();
    expect(showErrorToast).not.toHaveBeenCalled();
  });

  it('shows error toast when save fails without field errors', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(getMyInstructorProfile).mockResolvedValue({
      isSuccess: true,
      value: { ...profile, displayName: 'Cô Lan' },
    });
    vi.mocked(updateInstructorProfile).mockResolvedValue({
      isSuccess: false,
      fieldErrors: {},
      message: 'Có sự cố khi lưu hồ sơ. Bạn thử lại sau ít phút.',
    });
    renderPage();
    await screen.findByLabelText(/Tên hiển thị/);

    // Act
    await user.click(screen.getByRole('button', { name: 'Lưu' }));

    // Assert
    await waitFor(() => {
      expect(showErrorToast).toHaveBeenCalledWith(
        'Có sự cố khi lưu hồ sơ. Bạn thử lại sau ít phút.',
      );
    });
  });
});
