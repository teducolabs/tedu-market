import { SIGN_IN_PATH, SIGN_OUT_PATH } from './oidc-options';

export const SIGN_IN_FAILED_PARAM = 'signIn';
export const SIGN_IN_FAILED_VALUE = 'failed';

const FALLBACK_PATH = '/';

function isLoopPath(path: string): boolean {
  return [SIGN_IN_PATH, SIGN_OUT_PATH].some(
    (prefix) => path === prefix || path.startsWith(`${prefix}/`) || path.startsWith(`${prefix}?`),
  );
}

/** Chỉ nhận đường dẫn nội bộ của site; mọi giá trị khác (URL ngoài, `//host`, ký tự điều khiển) về `/` để chống open redirect. */
export function sanitizeReturnPath(rawPath: string | null | undefined): string {
  if (rawPath === null || rawPath === undefined) {
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

/** Thêm `signIn=failed` vào đường dẫn quay lại để banner "Đăng nhập chưa hoàn tất" hiện (ux.md). */
export function withSignInFailed(path: string): string {
  const url = new URL(path, 'http://placeholder.invalid');
  url.searchParams.set(SIGN_IN_FAILED_PARAM, SIGN_IN_FAILED_VALUE);
  return `${url.pathname}${url.search}${url.hash}`;
}

/** Bỏ `signIn=failed` khỏi đường dẫn để nút thử lại không giữ lại banner. */
export function withoutSignInFailed(path: string): string {
  const url = new URL(path, 'http://placeholder.invalid');
  url.searchParams.delete(SIGN_IN_FAILED_PARAM);
  return `${url.pathname}${url.search}${url.hash}`;
}

export const WELCOME_PARAM = 'welcome';

/** Báo trang quay lại hiện toast chào mừng (chỉ lần đăng nhập Google đầu tiên). */
export function withWelcome(path: string): string {
  const url = new URL(path, 'http://placeholder.invalid');
  url.searchParams.set(WELCOME_PARAM, '1');
  return `${url.pathname}${url.search}${url.hash}`;
}

/** Bỏ `welcome` khỏi đường dẫn để tải lại trang không hiện toast lần nữa. */
export function withoutWelcome(path: string): string {
  const url = new URL(path, 'http://placeholder.invalid');
  url.searchParams.delete(WELCOME_PARAM);
  return `${url.pathname}${url.search}${url.hash}`;
}
