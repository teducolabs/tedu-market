import {
  deleteApiV1CategoriesByCategoryId,
  getApiV1Categories,
  postApiV1Categories,
  putApiV1CategoriesByCategoryId,
} from '@tedu-market/api-client';

export interface CategoryItem {
  categoryId: string;
  name: string;
}

export interface CategoryFieldErrors {
  name?: string;
}

export type LoadResult<T> = { isSuccess: true; value: T } | { isSuccess: false };
export type ActionResult = { isSuccess: true } | { isSuccess: false };
export type SaveCategoryResult =
  | { isSuccess: true }
  | { isSuccess: false; fieldErrors: CategoryFieldErrors; message: string | null };

const GENERIC_SAVE_ERROR = 'Có sự cố khi lưu Category. Bạn thử lại sau ít phút.';

/** Gọi `GET /api/v1/categories` (công khai): mọi Category, xếp theo tên. */
export async function listCategories(): Promise<LoadResult<CategoryItem[]>> {
  const { data, response } = await getApiV1Categories();
  if (response?.ok !== true || data === undefined) {
    return { isSuccess: false };
  }
  return { isSuccess: true, value: data.map(({ categoryId, name }) => ({ categoryId, name })) };
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}

function toSaveResult(response: Response | undefined, error: unknown): SaveCategoryResult {
  if (response?.ok === true) {
    return { isSuccess: true };
  }
  const errors = response?.status === 400 && isRecord(error) ? error.errors : null;
  const messages = isRecord(errors) ? errors.name : undefined;
  const nameError =
    Array.isArray(messages) && typeof messages[0] === 'string' ? messages[0] : undefined;
  return nameError === undefined
    ? { isSuccess: false, fieldErrors: {}, message: GENERIC_SAVE_ERROR }
    : { isSuccess: false, fieldErrors: { name: nameError }, message: null };
}

/** Gọi `POST /api/v1/categories` (Admin, FR-17); lỗi 400 theo trường gắn đúng ô (FR-26). */
export async function createCategory(name: string): Promise<SaveCategoryResult> {
  const { response, error } = await postApiV1Categories({ body: { name } });
  return toSaveResult(response, error);
}

/** Gọi `PUT /api/v1/categories/{categoryId}` (Admin, FR-17). */
export async function updateCategory(
  categoryId: string,
  name: string,
): Promise<SaveCategoryResult> {
  const { response, error } = await putApiV1CategoriesByCategoryId({
    path: { categoryId },
    body: { name },
  });
  return toSaveResult(response, error);
}

/** Gọi `DELETE /api/v1/categories/{categoryId}` (Admin, FR-17). */
export async function deleteCategory(categoryId: string): Promise<ActionResult> {
  const { response } = await deleteApiV1CategoriesByCategoryId({ path: { categoryId } });
  return { isSuccess: response?.ok === true };
}
