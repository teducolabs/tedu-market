import { InMemoryWebStorage, WebStorageStateStore, type UserManagerSettings } from 'oidc-client-ts';

export const ADMIN_APP_CLIENT_ID = 'admin-app';
export const AUTH_CALLBACK_PATH = '/auth/callback';
export const SIGN_IN_PATH = '/sign-in';
const OIDC_SCOPE = 'openid profile email roles offline_access';

/**
 * Cấu hình OIDC Code + PKCE cho client public `admin-app` (NFR-06). Token giữ trong bộ nhớ
 * (`InMemoryWebStorage`), không ghi `localStorage`/`sessionStorage`; chỉ trạng thái yêu cầu đăng nhập
 * (state, code verifier) nằm ở kho mặc định của thư viện để sống qua lần chuyển trang.
 */
export function createUserManagerSettings(apiUrl: string, origin: string): UserManagerSettings {
  return {
    authority: apiUrl,
    client_id: ADMIN_APP_CLIENT_ID,
    redirect_uri: `${origin}${AUTH_CALLBACK_PATH}`,
    post_logout_redirect_uri: `${origin}/`,
    response_type: 'code',
    scope: OIDC_SCOPE,
    userStore: new WebStorageStateStore({ store: new InMemoryWebStorage() }),
    automaticSilentRenew: true,
  };
}
