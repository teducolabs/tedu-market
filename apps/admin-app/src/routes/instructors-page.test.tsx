import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { showErrorToast, showSuccessToast } from '@tedu-market/ui';
import { MemoryRouter, Outlet, Route, Routes } from 'react-router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import {
  approveInstructor,
  deleteInstructorProfile,
  listUsers,
  type UserItem,
} from '../lib/instructors/instructor-api';
import type { AdminAppRole } from '../lib/sidebar-items';
import { InstructorsPage } from './instructors-page';

vi.mock('../lib/instructors/instructor-api', async (importOriginal) => ({
  ...(await importOriginal<typeof import('../lib/instructors/instructor-api')>()),
  listUsers: vi.fn(),
  approveInstructor: vi.fn(),
  deleteInstructorProfile: vi.fn(),
}));
vi.mock('@tedu-market/ui', async (importOriginal) => ({
  ...(await importOriginal<typeof import('@tedu-market/ui')>()),
  showSuccessToast: vi.fn(),
  showErrorToast: vi.fn(),
}));

const student: UserItem = {
  userId: 'u1',
  email: 'hv@x.vn',
  roles: ['Student'],
  instructorProfileId: null,
  displayName: null,
};
const instructor: UserItem = {
  userId: 'u2',
  email: 'gv@x.vn',
  roles: ['Student', 'Instructor'],
  instructorProfileId: 'p2',
  displayName: 'Cô Lan',
};
const pageOf = (items: UserItem[], totalCount = items.length) => ({
  isSuccess: true as const,
  value: { items, pageNumber: 1, pageSize: 20, totalCount },
});

const renderPage = (role: AdminAppRole = 'Admin') =>
  render(
    <MemoryRouter initialEntries={['/instructors']}>
      <Routes>
        <Route element={<Outlet context={{ role }} />}>
          <Route path="/instructors" element={<InstructorsPage />} />
          <Route path="/instructors/:instructorProfileId" element={<p>Trang sửa</p>} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );

describe('InstructorsPage', () => {
  beforeEach(() => {
    vi.mocked(listUsers).mockReset();
    vi.mocked(approveInstructor).mockReset();
    vi.mocked(deleteInstructorProfile).mockReset();
    vi.mocked(showSuccessToast).mockReset();
    vi.mocked(showErrorToast).mockReset();
  });

  it('shows no permission for Instructor without calling the api', () => {
    // Arrange / Act
    renderPage('Instructor');

    // Assert
    expect(screen.getByText('Bạn không có quyền vào mục này.')).toBeInTheDocument();
    expect(listUsers).not.toHaveBeenCalled();
  });

  it('lists users with actions per row', async () => {
    // Arrange
    vi.mocked(listUsers).mockResolvedValue(pageOf([student, instructor]));

    // Act
    renderPage();

    // Assert
    expect(await screen.findByText('hv@x.vn')).toBeInTheDocument();
    expect(screen.getByRole('columnheader', { name: 'Tên hiển thị' })).toBeInTheDocument();
    expect(screen.getByText('Cô Lan')).toBeInTheDocument();
    expect(screen.getAllByRole('button', { name: 'Duyệt Instructor' })).toHaveLength(1);
    expect(screen.getByRole('button', { name: 'Sửa hồ sơ gv@x.vn' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Xóa hồ sơ gv@x.vn' })).toBeInTheDocument();
  });

  it('shows empty state when there are no users', async () => {
    // Arrange
    vi.mocked(listUsers).mockResolvedValue(pageOf([]));

    // Act
    renderPage();

    // Assert
    expect(await screen.findByText('Chưa có người dùng nào.')).toBeInTheDocument();
  });

  it('shows no-results state and clears filter when filter matches nothing', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listUsers).mockResolvedValueOnce(pageOf([student]));
    vi.mocked(listUsers).mockResolvedValueOnce(pageOf([]));
    vi.mocked(listUsers).mockResolvedValueOnce(pageOf([student]));
    renderPage();
    await screen.findByText('hv@x.vn');

    // Act
    await user.selectOptions(screen.getByLabelText('Vai trò'), 'Admin');
    await user.click(await screen.findByRole('button', { name: 'Xóa bộ lọc' }));

    // Assert
    expect(listUsers).toHaveBeenNthCalledWith(2, 'Admin', 1);
    expect(await screen.findByText('hv@x.vn')).toBeInTheDocument();
    expect(listUsers).toHaveBeenNthCalledWith(3, null, 1);
  });

  it('shows error and retries when loading fails', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listUsers).mockResolvedValueOnce({ isSuccess: false });
    vi.mocked(listUsers).mockResolvedValueOnce(pageOf([student]));
    renderPage();

    // Act
    await user.click(await screen.findByRole('button', { name: 'Thử lại' }));

    // Assert
    expect(await screen.findByText('hv@x.vn')).toBeInTheDocument();
  });

  it('approves instructor after confirmation and reloads list', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listUsers).mockResolvedValueOnce(pageOf([student]));
    vi.mocked(listUsers).mockResolvedValueOnce(
      pageOf([{ ...student, roles: ['Student', 'Instructor'], instructorProfileId: 'p1' }]),
    );
    vi.mocked(approveInstructor).mockResolvedValue({ isSuccess: true });
    renderPage();
    await user.click(await screen.findByRole('button', { name: 'Duyệt Instructor' }));

    // Act
    await user.click(
      within(await screen.findByRole('dialog')).getByRole('button', { name: 'Duyệt Instructor' }),
    );

    // Assert
    await waitFor(() => {
      expect(approveInstructor).toHaveBeenCalledWith('u1');
    });
    expect(showSuccessToast).toHaveBeenCalledWith('Đã duyệt Instructor');
    expect(await screen.findByRole('button', { name: 'Sửa hồ sơ hv@x.vn' })).toBeInTheDocument();
  });

  it('shows error toast when approval fails', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listUsers).mockResolvedValue(pageOf([student]));
    vi.mocked(approveInstructor).mockResolvedValue({ isSuccess: false });
    renderPage();
    await user.click(await screen.findByRole('button', { name: 'Duyệt Instructor' }));

    // Act
    await user.click(
      within(await screen.findByRole('dialog')).getByRole('button', { name: 'Duyệt Instructor' }),
    );

    // Assert
    await waitFor(() => {
      expect(showErrorToast).toHaveBeenCalledWith(
        'Không thể duyệt Instructor. Bạn thử lại sau ít phút.',
      );
    });
  });

  it('deletes profile only after confirmation', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listUsers).mockResolvedValue(pageOf([instructor]));
    vi.mocked(deleteInstructorProfile).mockResolvedValue({ isSuccess: true });
    renderPage();
    await user.click(await screen.findByRole('button', { name: 'Xóa hồ sơ gv@x.vn' }));
    expect(deleteInstructorProfile).not.toHaveBeenCalled();

    // Act
    await user.click(
      within(await screen.findByRole('dialog')).getByRole('button', { name: 'Xóa hồ sơ' }),
    );

    // Assert
    await waitFor(() => {
      expect(deleteInstructorProfile).toHaveBeenCalledWith('p2');
    });
    expect(showSuccessToast).toHaveBeenCalledWith('Đã xóa Hồ sơ Instructor');
  });

  it('navigates to the edit page when clicking edit', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listUsers).mockResolvedValue(pageOf([instructor]));
    renderPage();

    // Act
    await user.click(await screen.findByRole('button', { name: 'Sửa hồ sơ gv@x.vn' }));

    // Assert
    expect(await screen.findByText('Trang sửa')).toBeInTheDocument();
  });

  it('moves to next page when clicking next', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listUsers).mockResolvedValue(pageOf([student], 25));
    renderPage();

    // Act
    await user.click(await screen.findByRole('button', { name: 'Trang sau' }));

    // Assert
    await waitFor(() => {
      expect(listUsers).toHaveBeenLastCalledWith(null, 2);
    });
  });
});
