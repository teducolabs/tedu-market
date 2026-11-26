import { AUTH_CALLBACK_PATH, SIGN_IN_PATH } from './auth-settings';

const FALLBACK_PATH = '/';

function isLoopPath(path: string): boolean {
  return [SIGN_IN_PATH, AUTH_CALLBACK_PATH].some(
    (prefix) => path === prefix || path.startsWith(`${prefix}/`) || path.startsWith(`${prefix}?`),
  );
}

/** Chỉ nhận đường dẫn nội bộ; mọi giá trị khác (URL ngoài, `//host`, ký tự điều khiển) về `/` để chống open redirect. */
export function sanitizeReturnPath(rawPath: unknown): string {
  if (typeof rawPath !== 'string') {
    return FALLBACK_PATH;
  }
  const hasForbiddenCharacter = Array.from({ length: rawPath.length }, (_, index) =>
    rawPath.charCodeAt(index),
  ).some((code) => code < 0x20 || code === 0x7f || code === 0x5c);
  if (
    hasForbiddenCharacter ||
    !rawPath.startsWith('/') ||
    rawPath.startsWith('//') ||
    isLoopPath(rawPath)
  ) {
    return FALLBACK_PATH;
  }
  return rawPath;
}

export interface SignInState {
  returnTo: string;
}

/** Đọc đường dẫn quay lại từ `state` người dùng đính vào yêu cầu đăng nhập. */
export function readReturnPath(state: unknown): string {
  if (typeof state === 'object' && state !== null && 'returnTo' in state) {
    return sanitizeReturnPath(state.returnTo);
  }
  return FALLBACK_PATH;
}
