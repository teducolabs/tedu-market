import type { Page } from '@playwright/test';

/** API giả: test chặn mọi request tới đây, không cần chạy API thật (khớp `playwright.config.ts`). */
export const FAKE_API_URL = 'http://127.0.0.1:5999';
const CORS_HEADERS = {
  'access-control-allow-origin': '*',
  'access-control-allow-headers': '*',
  'access-control-allow-methods': '*',
};

export type E2eRole = 'Admin' | 'Instructor' | 'Student';

export interface AuthorizationServerMock {
  /** URL các request đã chuyển tới Authorization Server giả, theo thứ tự. */
  requests: URL[];
  /** Kết quả đổi mật khẩu kế tiếp: lỗi theo trường hoặc thành công. */
  setChangePasswordResponse: (status: number, body?: unknown) => void;
}

const base64Url = (value: unknown): string =>
  btoa(JSON.stringify(value)).replaceAll('+', '-').replaceAll('/', '_').replace(/=+$/, '');

/** id_token không ký: oidc-client-ts chỉ kiểm tra iss, aud, nonce, exp ở phía trình duyệt. */
function createIdToken(role: E2eRole, nonce: string): string {
  const now = Math.floor(Date.now() / 1000);
  const roles = role === 'Instructor' ? ['Instructor', 'Student'] : [role];
  const claims = {
    iss: FAKE_API_URL,
    aud: 'admin-app',
    sub: '00000000-0000-0000-0000-000000000001',
    name: `${role.toLowerCase()}@tedu.test`,
    role: roles,
    nonce,
    iat: now,
    exp: now + 3600,
  };
  return `${base64Url({ alg: 'none', typ: 'JWT' })}.${base64Url(claims)}.sig`;
}

/**
 * Giả lập Authorization Server: discovery, `/connect/authorize` (chuyển ngay về `/auth/callback` với code,
 * hoặc `error=access_denied` khi `isCancelled`), `/connect/token`, `/connect/logout` và đổi mật khẩu.
 */
export async function mockAuthorizationServer(
  page: Page,
  options: { role: E2eRole; isCancelled?: boolean },
): Promise<AuthorizationServerMock> {
  const requests: URL[] = [];
  let nonce = '';
  let changePassword = { status: 204, body: undefined as unknown };

  await page.route(`${FAKE_API_URL}/**`, async (route) => {
    const request = route.request();
    const url = new URL(request.url());
    requests.push(url);
    if (request.method() === 'OPTIONS') {
      await route.fulfill({ status: 204, headers: CORS_HEADERS });
      return;
    }
    switch (url.pathname) {
      case '/.well-known/openid-configuration':
        await route.fulfill({
          headers: CORS_HEADERS,
          json: {
            issuer: FAKE_API_URL,
            authorization_endpoint: `${FAKE_API_URL}/connect/authorize`,
            token_endpoint: `${FAKE_API_URL}/connect/token`,
            end_session_endpoint: `${FAKE_API_URL}/connect/logout`,
            jwks_uri: `${FAKE_API_URL}/.well-known/jwks`,
            response_types_supported: ['code'],
            subject_types_supported: ['public'],
            id_token_signing_alg_values_supported: ['RS256'],
          },
        });
        return;
      case '/connect/authorize': {
        nonce = url.searchParams.get('nonce') ?? '';
        const state = url.searchParams.get('state') ?? '';
        const redirectUri = url.searchParams.get('redirect_uri') ?? '';
        const query = options.isCancelled === true ? 'error=access_denied' : 'code=e2e-code';
        await route.fulfill({
          status: 302,
          headers: { location: `${redirectUri}?${query}&state=${state}` },
        });
        return;
      }
      case '/connect/token':
        await route.fulfill({
          headers: CORS_HEADERS,
          json: {
            access_token: 'e2e-access-token',
            token_type: 'Bearer',
            expires_in: 3600,
            refresh_token: 'e2e-refresh-token',
            id_token: createIdToken(options.role, nonce),
            scope: 'openid profile email roles offline_access',
          },
        });
        return;
      case '/connect/logout': {
        const target = url.searchParams.get('post_logout_redirect_uri') ?? '/';
        await route.fulfill({ status: 302, headers: { location: target } });
        return;
      }
      case '/api/v1/account/change-password':
        await route.fulfill({
          status: changePassword.status,
          headers: CORS_HEADERS,
          ...(changePassword.body === undefined ? {} : { json: changePassword.body }),
        });
        return;
      default:
        await route.fulfill({ status: 404, headers: CORS_HEADERS });
    }
  });

  return {
    requests,
    setChangePasswordResponse: (status, body) => {
      changePassword = { status, body };
    },
  };
}

/** Mở App quản trị, bấm Đăng nhập bằng email và chờ vào trang chính (Authorization Server giả đã cấp token). */
export async function signInAs(page: Page, role: E2eRole): Promise<AuthorizationServerMock> {
  const server = await mockAuthorizationServer(page, { role });
  await page.goto('/');
  await page.getByRole('button', { name: 'Đăng nhập bằng email' }).click();
  await page.waitForURL(/\/revenue$/);
  return server;
}
