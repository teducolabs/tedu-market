export const OIDC_CLIENT_ID = 'student-web';
export const SIGN_IN_PATH = '/sign-in';
export const SIGN_IN_CALLBACK_PATH = '/sign-in/callback';
/** Route handler hoàn tất đăng nhập; trang `/sign-in/callback` (redirect_uri) chuyển tiếp sang đây sau khi hiện "Đang đăng nhập…". */
export const SIGN_IN_COMPLETE_PATH = '/sign-in/complete';
export const SIGN_OUT_PATH = '/sign-out';

export interface OidcOptions {
  /** URL công khai của API (Authorization Server), không có dấu `/` cuối. */
  apiUrl: string;
  /** URL công khai của StudentWeb, không có dấu `/` cuối. */
  siteUrl: string;
  clientId: string;
  clientSecret: string;
  sessionSecret: string;
}

export const MIN_SESSION_SECRET_LENGTH = 32;

type Environment = Readonly<Record<string, string | undefined>>;

function readRequired(environment: Environment, name: string): string {
  const value = environment[name];
  if (value === undefined || value.trim() === '') {
    throw new Error(`Thiếu biến môi trường ${name}`);
  }
  return value.trim();
}

function trimTrailingSlash(url: string): string {
  return url.replace(/\/+$/, '');
}

/** Đọc cấu hình OIDC lúc chạy (server-only). Tên biến theo docs/deploy.md; thiếu thì ném lỗi. */
export function getOidcOptions(environment: Environment = process.env): OidcOptions {
  const sessionSecret = readRequired(environment, 'StudentWeb__SessionSecret');
  if (sessionSecret.length < MIN_SESSION_SECRET_LENGTH) {
    throw new Error(
      `StudentWeb__SessionSecret phải có ít nhất ${String(MIN_SESSION_SECRET_LENGTH)} ký tự`,
    );
  }
  return {
    apiUrl: trimTrailingSlash(readRequired(environment, 'API_URL')),
    siteUrl: trimTrailingSlash(environment.SITE_URL?.trim() || 'http://localhost:3000'),
    clientId: OIDC_CLIENT_ID,
    clientSecret: readRequired(environment, 'StudentWeb__OidcClientSecret'),
    sessionSecret,
  };
}

export function getRedirectUri(options: OidcOptions): string {
  return `${options.siteUrl}${SIGN_IN_CALLBACK_PATH}`;
}
