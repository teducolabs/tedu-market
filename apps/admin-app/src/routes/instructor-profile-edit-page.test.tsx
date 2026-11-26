import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { showErrorToast, showSuccessToast } from '@tedu-market/ui';
import { MemoryRouter, Outlet, Route, Routes } from 'react-router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { getInstructorProfile, updateInstructorProfile } from '../lib/instructors/instructor-api';
import type { AdminAppRole } from '../lib/sidebar-items';
import { InstructorProfileEditPage } from './instructor-profile-edit-page';

vi.mock('../lib/instructors/instructor-api', () => ({
  getInstructorProfile: vi.fn(),
  updateInstructorProfile: vi.fn(),
}));
vi.mock('@tedu-market/ui', async (importOriginal) => ({
  ...(await importOriginal<typeof import('@tedu-market/ui')>()),
  showSuccessToast: vi.fn(),
  showErrorToast: vi.fn(),
}));

const profile = { instructorProfileId: 'p1', displayName: 'Cô Lan', biography: '', avatarUrl: '' };

const renderPage = (role: AdminAppRole = 'Admin') =>
  render(
    <MemoryRouter initialEntries={['/instructors/p1']}>
      <Routes>
        <Route element={<Outlet context={{ role }} />}>
          <Route path="/instructors" element={<p>Danh sách</p>} />
          <Route path="/instructors/:instructorProfileId" element={<InstructorProfileEditPage />} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );

describe('InstructorProfileEditPage', () => {
  beforeEach(() => {
    vi.mocked(getInstructorProfile).mockReset();
    vi.mocked(updateInstructorProfile).mockReset();
    vi.mocked(showSuccessToast).mockReset();
    vi.mocked(showErrorToast).mockReset();
  });

  it('shows no permission for Instructor', () => {
    // Arrange / Act
    renderPage('Instructor');

    // Assert
    expect(screen.getByText('Bạn không có quyền vào mục này.')).toBeInTheDocument();
    expect(getInstructorProfile).not.toHaveBeenCalled();
  });

  it('shows error and retries when loading fails', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(getInstructorProfile).mockResolvedValueOnce({ isSuccess: false });
    vi.mocked(getInstructorProfile).mockResolvedValueOnce({ isSuccess: true, value: profile });
    renderPage();

    // Act
    await user.click(await screen.findByRole('button', { name: 'Thử lại' }));

    // Assert
    expect(await screen.findByLabelText(/Tên hiển thị/)).toHaveValue('Cô Lan');
  });

  it('shows required error without calling the api when display name is empty', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(getInstructorProfile).mockResolvedValue({
      isSuccess: true,
      value: { ...profile, displayName: '' },
    });
    renderPage();
    await screen.findByLabelText(/Tên hiển thị/);

    // Act
    await user.click(screen.getByRole('button', { name: 'Lưu' }));

    // Assert
    expect(await screen.findByText('Bạn cần nhập Tên hiển thị')).toBeInTheDocument();
    expect(updateInstructorProfile).not.toHaveBeenCalled();
  });

  it('saves, shows toast and returns to the list when save succeeds', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(getInstructorProfile).mockResolvedValue({ isSuccess: true, value: profile });
    vi.mocked(updateInstructorProfile).mockResolvedValue({ isSuccess: true });
    renderPage();
    const input = await screen.findByLabelText(/Tên hiển thị/);

    // Act
    await user.clear(input);
    await user.type(input, 'Cô Hoa');
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
    expect(await screen.findByText('Danh sách')).toBeInTheDocument();
  });

  it('shows field error under the input when server returns 400', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(getInstructorProfile).mockResolvedValue({ isSuccess: true, value: profile });
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
    expect(showErrorToast).not.toHaveBeenCalled();
  });

  it('shows error toast when save fails without field errors', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(getInstructorProfile).mockResolvedValue({ isSuccess: true, value: profile });
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
