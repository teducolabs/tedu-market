import { postApiV1AccountChangePassword } from '@tedu-market/api-client';

export interface ChangePasswordFieldErrors {
  currentPassword?: string;
  newPassword?: string;
}

export type ChangePasswordResult =
  | { isSuccess: true }
  | { isSuccess: false; fieldErrors: ChangePasswordFieldErrors; message: string | null };

const GENERIC_ERROR_MESSAGE = 'Có sự cố khi đổi mật khẩu. Bạn thử lại sau ít phút.';

function firstMessage(errors: unknown, field: string): string | undefined {
  if (typeof errors !== 'object' || errors === null) {
    return undefined;
  }
  const messages: unknown = (errors as Record<string, unknown>)[field];
  if (Array.isArray(messages) && typeof messages[0] === 'string') {
    return messages[0];
  }
  return undefined;
}

function readErrors(problem: unknown): unknown {
  return typeof problem === 'object' && problem !== null && 'errors' in problem
    ? problem.errors
    : null;
}

/** Gọi `POST /api/v1/account/change-password` (FR-42); lỗi 400 theo trường gắn đúng ô (FR-26). */
export async function changePassword(
  currentPassword: string,
  newPassword: string,
): Promise<ChangePasswordResult> {
  const { response, error } = await postApiV1AccountChangePassword({
    body: { currentPassword, newPassword },
  });
  if (response?.ok === true) {
    return { isSuccess: true };
  }

  const errors = response?.status === 400 ? readErrors(error) : null;
  const fieldErrors: ChangePasswordFieldErrors = {};
  const currentError = firstMessage(errors, 'currentPassword');
  const newError = firstMessage(errors, 'newPassword');
  if (currentError !== undefined) {
    fieldErrors.currentPassword = currentError;
  }
  if (newError !== undefined) {
    fieldErrors.newPassword = newError;
  }
  const hasFieldError = currentError !== undefined || newError !== undefined;
  return { isSuccess: false, fieldErrors, message: hasFieldError ? null : GENERIC_ERROR_MESSAGE };
}
