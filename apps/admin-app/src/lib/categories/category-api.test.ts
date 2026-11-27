import * as apiClient from '@tedu-market/api-client';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createCategory, deleteCategory, listCategories, updateCategory } from './category-api';

vi.mock('@tedu-market/api-client', () => ({
  getApiV1Categories: vi.fn(),
  postApiV1Categories: vi.fn(),
  putApiV1CategoriesByCategoryId: vi.fn(),
  deleteApiV1CategoriesByCategoryId: vi.fn(),
}));

const ok = { ok: true, status: 200 };
const asResult = (value: unknown) => value as never;

describe('category-api', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('returns categories when listing succeeds', async () => {
    // Arrange
    vi.mocked(apiClient.getApiV1Categories).mockResolvedValue(
      asResult({ response: ok, data: [{ categoryId: 'c1', name: 'Lập trình' }] }),
    );

    // Act
    const result = await listCategories();

    // Assert
    expect(result).toEqual({ isSuccess: true, value: [{ categoryId: 'c1', name: 'Lập trình' }] });
  });

  it('returns failure when listing responds with an error', async () => {
    // Arrange
    vi.mocked(apiClient.getApiV1Categories).mockResolvedValue(
      asResult({ response: { ok: false, status: 500 } }),
    );

    // Act
    const result = await listCategories();

    // Assert
    expect(result).toEqual({ isSuccess: false });
  });

  it('sends the name and reports success when creating', async () => {
    // Arrange
    vi.mocked(apiClient.postApiV1Categories).mockResolvedValue(
      asResult({ response: { ok: true, status: 201 } }),
    );

    // Act
    const result = await createCategory('Thiết kế');

    // Assert
    expect(apiClient.postApiV1Categories).toHaveBeenCalledWith({ body: { name: 'Thiết kế' } });
    expect(result).toEqual({ isSuccess: true });
  });

  it('maps the 400 name error to the field when creating', async () => {
    // Arrange
    vi.mocked(apiClient.postApiV1Categories).mockResolvedValue(
      asResult({
        response: { ok: false, status: 400 },
        error: { errors: { name: ['Tên Category đã tồn tại'] } },
      }),
    );

    // Act
    const result = await createCategory('Trùng');

    // Assert
    expect(result).toEqual({
      isSuccess: false,
      fieldErrors: { name: 'Tên Category đã tồn tại' },
      message: null,
    });
  });

  it('returns the generic message when saving fails without a field error', async () => {
    // Arrange
    vi.mocked(apiClient.putApiV1CategoriesByCategoryId).mockResolvedValue(
      asResult({ response: { ok: false, status: 500 }, error: {} }),
    );

    // Act
    const result = await updateCategory('c1', 'Mới');

    // Assert
    expect(result).toEqual({
      isSuccess: false,
      fieldErrors: {},
      message: 'Có sự cố khi lưu Category. Bạn thử lại sau ít phút.',
    });
  });

  it('sends id and name when updating', async () => {
    // Arrange
    vi.mocked(apiClient.putApiV1CategoriesByCategoryId).mockResolvedValue(
      asResult({ response: ok }),
    );

    // Act
    const result = await updateCategory('c1', 'Mới');

    // Assert
    expect(apiClient.putApiV1CategoriesByCategoryId).toHaveBeenCalledWith({
      path: { categoryId: 'c1' },
      body: { name: 'Mới' },
    });
    expect(result).toEqual({ isSuccess: true });
  });

  it('reports success and failure when deleting', async () => {
    // Arrange
    vi.mocked(apiClient.deleteApiV1CategoriesByCategoryId)
      .mockResolvedValueOnce(asResult({ response: { ok: true, status: 204 } }))
      .mockResolvedValueOnce(asResult({ response: { ok: false, status: 404 } }));

    // Act
    const succeeded = await deleteCategory('c1');
    const failed = await deleteCategory('c2');

    // Assert
    expect(succeeded).toEqual({ isSuccess: true });
    expect(failed).toEqual({ isSuccess: false });
  });
});
