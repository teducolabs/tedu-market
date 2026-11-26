import {
  deleteApiV1InstructorProfilesByInstructorProfileId,
  getApiV1InstructorProfilesByInstructorProfileId,
  getApiV1Users,
  postApiV1UsersByUserIdInstructorApproval,
  putApiV1InstructorProfilesByInstructorProfileId,
  type UserDto,
} from '@tedu-market/api-client';

export type UserRoleFilter = 'Student' | 'Instructor' | 'Admin';

export type UserItem = UserDto;

export interface UserPage {
  items: UserItem[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

export interface InstructorProfileItem {
  instructorProfileId: string;
  displayName: string;
  biography: string;
  avatarUrl: string;
}

export interface InstructorProfileInput {
  displayName: string;
  biography: string;
  avatarUrl: string;
}

export interface InstructorProfileFieldErrors {
  displayName?: string;
  biography?: string;
  avatarUrl?: string;
}

export type LoadResult<T> = { isSuccess: true; value: T } | { isSuccess: false };
export type ActionResult = { isSuccess: true } | { isSuccess: false };
export type SaveProfileResult =
  | { isSuccess: true }
  | { isSuccess: false; fieldErrors: InstructorProfileFieldErrors; message: string | null };

export const USERS_PAGE_SIZE = 20;
const GENERIC_SAVE_ERROR = 'Có sự cố khi lưu hồ sơ. Bạn thử lại sau ít phút.';

/** Gọi `GET /api/v1/users` (Admin): danh sách người dùng, lọc theo vai trò, 20 dòng mỗi trang. */
export async function listUsers(
  role: UserRoleFilter | null,
  pageNumber: number,
): Promise<LoadResult<UserPage>> {
  const { data, response } = await getApiV1Users({
    query: { pageNumber, pageSize: USERS_PAGE_SIZE, ...(role === null ? {} : { role }) },
  });
  if (response?.ok !== true || data === undefined) {
    return { isSuccess: false };
  }
  return {
    isSuccess: true,
    value: {
      items: data.items,
      pageNumber: Number(data.pageNumber),
      pageSize: Number(data.pageSize),
      totalCount: Number(data.totalCount),
    },
  };
}

/** Gọi `POST /api/v1/users/{userId}/instructor-approval` (FR-40). */
export async function approveInstructor(userId: string): Promise<ActionResult> {
  const { response } = await postApiV1UsersByUserIdInstructorApproval({ path: { userId } });
  return { isSuccess: response?.ok === true };
}

/** Gọi `GET /api/v1/instructor-profiles/{id}`; hồ sơ chưa cập nhật có trường trống. */
export async function getInstructorProfile(
  instructorProfileId: string,
): Promise<LoadResult<InstructorProfileItem>> {
  const { data, response } = await getApiV1InstructorProfilesByInstructorProfileId({
    path: { instructorProfileId },
  });
  if (response?.ok !== true || data === undefined) {
    return { isSuccess: false };
  }
  return {
    isSuccess: true,
    value: {
      instructorProfileId: data.instructorProfileId,
      displayName: data.displayName ?? '',
      biography: data.biography ?? '',
      avatarUrl: data.avatarUrl ?? '',
    },
  };
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}

function firstMessage(errors: unknown, field: string): string | undefined {
  if (!isRecord(errors)) {
    return undefined;
  }
  const messages = errors[field];
  return Array.isArray(messages) && typeof messages[0] === 'string' ? messages[0] : undefined;
}

/** Gọi `PUT /api/v1/instructor-profiles/{id}` (FR-18); lỗi 400 theo trường gắn đúng ô (FR-26). */
export async function updateInstructorProfile(
  instructorProfileId: string,
  input: InstructorProfileInput,
): Promise<SaveProfileResult> {
  const { response, error } = await putApiV1InstructorProfilesByInstructorProfileId({
    path: { instructorProfileId },
    body: input,
  });
  if (response?.ok === true) {
    return { isSuccess: true };
  }
  const errors = response?.status === 400 && isRecord(error) ? error.errors : null;
  const fieldErrors: InstructorProfileFieldErrors = {};
  for (const field of ['displayName', 'biography', 'avatarUrl'] as const) {
    const message = firstMessage(errors, field);
    if (message !== undefined) {
      fieldErrors[field] = message;
    }
  }
  const hasFieldError = Object.keys(fieldErrors).length > 0;
  return { isSuccess: false, fieldErrors, message: hasFieldError ? null : GENERIC_SAVE_ERROR };
}

/** Gọi `DELETE /api/v1/instructor-profiles/{id}`: xóa hồ sơ và gỡ role Instructor. */
export async function deleteInstructorProfile(instructorProfileId: string): Promise<ActionResult> {
  const { response } = await deleteApiV1InstructorProfilesByInstructorProfileId({
    path: { instructorProfileId },
  });
  return { isSuccess: response?.ok === true };
}
