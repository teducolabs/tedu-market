import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { showErrorToast, showSuccessToast } from '@tedu-market/ui';
import { MemoryRouter, Outlet, Route, Routes } from 'react-router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import {
  createCategory,
  deleteCategory,
  listCategories,
  updateCategory,
  type CategoryItem,
} from '../lib/categories/category-api';
import type { AdminAppRole } from '../lib/sidebar-items';
import { CategoriesPage, REQUIRED_NAME_MESSAGE } from './categories-page';

vi.mock('../lib/categories/category-api', () => ({
  listCategories: vi.fn(),
  createCategory: vi.fn(),
  updateCategory: vi.fn(),
  deleteCategory: vi.fn(),
}));
vi.mock('@tedu-market/ui', async (importOriginal) => ({
  ...(await importOriginal<typeof import('@tedu-market/ui')>()),
  showSuccessToast: vi.fn(),
  showErrorToast: vi.fn(),
}));

const programming: CategoryItem = { categoryId: 'c1', name: 'Lập trình' };
const design: CategoryItem = { categoryId: 'c2', name: 'Thiết kế' };
const listOf = (items: CategoryItem[]) => ({ isSuccess: true as const, value: items });

const renderPage = (role: AdminAppRole = 'Admin') =>
  render(
    <MemoryRouter initialEntries={['/categories']}>
      <Routes>
        <Route element={<Outlet context={{ role }} />}>
          <Route path="/categories" element={<CategoriesPage />} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );

describe('CategoriesPage', () => {
  beforeEach(() => {
    vi.mocked(listCategories).mockReset();
    vi.mocked(createCategory).mockReset();
    vi.mocked(updateCategory).mockReset();
    vi.mocked(deleteCategory).mockReset();
    vi.mocked(showSuccessToast).mockReset();
    vi.mocked(showErrorToast).mockReset();
  });

  it('shows no permission for Instructor without calling the api', () => {
    // Arrange / Act
    renderPage('Instructor');

    // Assert
    expect(screen.getByText('Bạn không có quyền vào mục này.')).toBeInTheDocument();
    expect(listCategories).not.toHaveBeenCalled();
  });

  it('shows the loading skeleton after the delay while categories are loading', async () => {
    // Arrange
    vi.mocked(listCategories).mockReturnValue(new Promise(() => undefined));

    // Act
    renderPage();

    // Assert
    expect(await screen.findByText('Đang tải…')).toBeInTheDocument();
    expect(screen.queryByText('Chưa có Category nào.')).not.toBeInTheDocument();
  });

  it('lists categories with edit and delete actions per row', async () => {
    // Arrange
    vi.mocked(listCategories).mockResolvedValue(listOf([programming, design]));

    // Act
    renderPage();

    // Assert
    expect(await screen.findByText('Lập trình')).toBeInTheDocument();
    expect(screen.getByText('Thiết kế')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Sửa Category Lập trình' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Xóa Category Thiết kế' })).toBeInTheDocument();
  });

  it('shows empty state with a create action when there are no categories', async () => {
    // Arrange
    vi.mocked(listCategories).mockResolvedValue(listOf([]));

    // Act
    renderPage();

    // Assert
    expect(await screen.findByText('Chưa có Category nào.')).toBeInTheDocument();
    expect(screen.getAllByRole('button', { name: 'Tạo Category' })).toHaveLength(2);
  });

  it('shows error and retries when loading fails', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listCategories).mockResolvedValueOnce({ isSuccess: false });
    vi.mocked(listCategories).mockResolvedValueOnce(listOf([programming]));
    renderPage();

    // Act
    await user.click(await screen.findByRole('button', { name: 'Thử lại' }));

    // Assert
    expect(await screen.findByText('Lập trình')).toBeInTheDocument();
  });

  it('creates a category and shows it in the reloaded list', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listCategories).mockResolvedValueOnce(listOf([]));
    vi.mocked(listCategories).mockResolvedValueOnce(listOf([design]));
    vi.mocked(createCategory).mockResolvedValue({ isSuccess: true });
    renderPage();
    await screen.findByText('Chưa có Category nào.');
    await user.click(screen.getAllByRole('button', { name: 'Tạo Category' })[0] as HTMLElement);

    // Act
    await user.type(screen.getByLabelText(/Tên/), 'Thiết kế');
    await user.click(screen.getByRole('button', { name: 'Lưu' }));

    // Assert
    await waitFor(() => {
      expect(createCategory).toHaveBeenCalledWith('Thiết kế');
    });
    expect(showSuccessToast).toHaveBeenCalledWith('Đã lưu');
    expect(await screen.findByText('Thiết kế')).toBeInTheDocument();
    expect(screen.queryByRole('form', { name: 'Tạo Category' })).not.toBeInTheDocument();
  });

  it('shows required error under the input without calling the api when name is blank', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listCategories).mockResolvedValue(listOf([programming]));
    renderPage();
    await user.click(await screen.findByRole('button', { name: 'Tạo Category' }));

    // Act
    await user.click(screen.getByRole('button', { name: 'Lưu' }));

    // Assert
    expect(screen.getByText(REQUIRED_NAME_MESSAGE)).toBeInTheDocument();
    expect(screen.getByLabelText(/Tên/)).toHaveAttribute('aria-invalid', 'true');
    expect(createCategory).not.toHaveBeenCalled();
  });

  it('shows field error under the input when server returns 400', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listCategories).mockResolvedValue(listOf([programming]));
    vi.mocked(createCategory).mockResolvedValue({
      isSuccess: false,
      fieldErrors: { name: 'Tên Category đã tồn tại' },
      message: null,
    });
    renderPage();
    await user.click(await screen.findByRole('button', { name: 'Tạo Category' }));
    await user.type(screen.getByLabelText(/Tên/), 'Lập trình');

    // Act
    await user.click(screen.getByRole('button', { name: 'Lưu' }));

    // Assert
    expect(await screen.findByText('Tên Category đã tồn tại')).toBeInTheDocument();
    expect(showErrorToast).not.toHaveBeenCalled();
  });

  it('shows error toast when saving fails without a field error', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listCategories).mockResolvedValue(listOf([programming]));
    vi.mocked(updateCategory).mockResolvedValue({
      isSuccess: false,
      fieldErrors: {},
      message: 'Có sự cố khi lưu Category. Bạn thử lại sau ít phút.',
    });
    renderPage();
    await user.click(await screen.findByRole('button', { name: 'Sửa Category Lập trình' }));

    // Act
    await user.click(screen.getByRole('button', { name: 'Lưu' }));

    // Assert
    await waitFor(() => {
      expect(showErrorToast).toHaveBeenCalledWith(
        'Có sự cố khi lưu Category. Bạn thử lại sau ít phút.',
      );
    });
  });

  it('edits a category by its id with the name prefilled', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listCategories).mockResolvedValueOnce(listOf([programming]));
    vi.mocked(listCategories).mockResolvedValueOnce(
      listOf([{ categoryId: 'c1', name: 'Lập trình web' }]),
    );
    vi.mocked(updateCategory).mockResolvedValue({ isSuccess: true });
    renderPage();
    await user.click(await screen.findByRole('button', { name: 'Sửa Category Lập trình' }));
    expect(screen.getByLabelText(/Tên/)).toHaveValue('Lập trình');

    // Act
    await user.type(screen.getByLabelText(/Tên/), ' web');
    await user.click(screen.getByRole('button', { name: 'Lưu' }));

    // Assert
    await waitFor(() => {
      expect(updateCategory).toHaveBeenCalledWith('c1', 'Lập trình web');
    });
    expect(await screen.findByText('Lập trình web')).toBeInTheDocument();
  });

  it('deletes a category only after confirmation and reloads the list', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listCategories).mockResolvedValueOnce(listOf([programming, design]));
    vi.mocked(listCategories).mockResolvedValueOnce(listOf([design]));
    vi.mocked(deleteCategory).mockResolvedValue({ isSuccess: true });
    renderPage();
    await user.click(await screen.findByRole('button', { name: 'Xóa Category Lập trình' }));
    expect(deleteCategory).not.toHaveBeenCalled();

    // Act
    await user.click(
      within(await screen.findByRole('dialog')).getByRole('button', { name: 'Xóa Category' }),
    );

    // Assert
    await waitFor(() => {
      expect(deleteCategory).toHaveBeenCalledWith('c1');
    });
    expect(showSuccessToast).toHaveBeenCalledWith('Đã xóa Category');
    await waitFor(() => {
      expect(screen.queryByText('Lập trình')).not.toBeInTheDocument();
    });
  });

  it('shows error toast when deleting fails', async () => {
    // Arrange
    const user = userEvent.setup();
    vi.mocked(listCategories).mockResolvedValue(listOf([programming]));
    vi.mocked(deleteCategory).mockResolvedValue({ isSuccess: false });
    renderPage();
    await user.click(await screen.findByRole('button', { name: 'Xóa Category Lập trình' }));

    // Act
    await user.click(
      within(await screen.findByRole('dialog')).getByRole('button', { name: 'Xóa Category' }),
    );

    // Assert
    await waitFor(() => {
      expect(showErrorToast).toHaveBeenCalledWith(
        'Không thể xóa Category. Bạn thử lại sau ít phút.',
      );
    });
  });
});
