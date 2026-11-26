import { createHash, randomBytes } from 'node:crypto';
import { getRedirectUri, type OidcOptions } from './oidc-options';

const TOKEN_ENDPOINT_PATH = '/connect/token';
const AUTHORIZE_ENDPOINT_PATH = '/connect/authorize';
const END_SESSION_ENDPOINT_PATH = '/connect/logout';
const SCOPE = 'openid profile email offline_access';
/** Chọn Google làm nơi đăng nhập (decision log T16): API chuyển thẳng sang Google. */
const IDENTITY_PROVIDER = 'google';

export interface OidcTokens {
  accessToken: string;
  /** Không có khi phản hồi refresh không kèm id_token. */
  idToken: string | null;
  refreshToken: string | null;
  expiresInSeconds: number;
}

export interface OidcUserClaims {
  name: string;
  email: string;
  /** Học viên vừa được tạo ở lần đăng nhập này (claim `first_sign_in`). */
  isFirstSignIn: boolean;
}

export class OidcExchangeError extends Error {
  readonly innerError: unknown;

  constructor(message: string, innerError?: unknown) {
    super(message);
    this.name = 'OidcExchangeError';
    this.innerError = innerError;
  }
}

export function createRandomToken(): string {
  return randomBytes(32).toString('base64url');
}

export function createCodeChallenge(codeVerifier: string): string {
  return createHash('sha256').update(codeVerifier).digest('base64url');
}

export function buildAuthorizeUrl(
  options: OidcOptions,
  parameters: { state: string; codeVerifier: string },
): URL {
  const url = new URL(`${options.apiUrl}${AUTHORIZE_ENDPOINT_PATH}`);
  url.searchParams.set('response_type', 'code');
  url.searchParams.set('client_id', options.clientId);
  url.searchParams.set('redirect_uri', getRedirectUri(options));
  url.searchParams.set('scope', SCOPE);
  url.searchParams.set('state', parameters.state);
  url.searchParams.set('code_challenge', createCodeChallenge(parameters.codeVerifier));
  url.searchParams.set('code_challenge_method', 'S256');
  url.searchParams.set('identity_provider', IDENTITY_PROVIDER);
  return url;
}

export function buildEndSessionUrl(options: OidcOptions, idToken: string): URL {
  const url = new URL(`${options.apiUrl}${END_SESSION_ENDPOINT_PATH}`);
  url.searchParams.set('client_id', options.clientId);
  url.searchParams.set('id_token_hint', idToken);
  url.searchParams.set('post_logout_redirect_uri', `${options.siteUrl}/`);
  return url;
}

function readString(value: unknown): string | null {
  return typeof value === 'string' && value !== '' ? value : null;
}

async function postToTokenEndpoint(
  options: OidcOptions,
  grantParameters: Record<string, string>,
  fetchImplementation: typeof fetch,
): Promise<OidcTokens> {
  const body = new URLSearchParams({
    ...grantParameters,
    client_id: options.clientId,
    client_secret: options.clientSecret,
  });

  let response: Response;
  try {
    response = await fetchImplementation(`${options.apiUrl}${TOKEN_ENDPOINT_PATH}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body,
      cache: 'no-store',
    });
  } catch (error) {
    throw new OidcExchangeError('Không gọi được token endpoint', error);
  }
  if (!response.ok) {
    throw new OidcExchangeError(`Token endpoint trả ${String(response.status)}`);
  }

  const payload = (await response.json()) as Record<string, unknown>;
  const accessToken = readString(payload.access_token);
  const expiresInSeconds = typeof payload.expires_in === 'number' ? payload.expires_in : 0;
  if (accessToken === null || expiresInSeconds <= 0) {
    throw new OidcExchangeError('Phản hồi token thiếu access_token hoặc expires_in');
  }
  return {
    accessToken,
    idToken: readString(payload.id_token),
    refreshToken: readString(payload.refresh_token),
    expiresInSeconds,
  };
}

/** Đổi `code` lấy token ở back-channel (client confidential, PKCE); lỗi mạng hoặc phản hồi sai đều thành `OidcExchangeError`. */
export async function exchangeCodeForTokens(
  options: OidcOptions,
  parameters: { code: string; codeVerifier: string },
  fetchImplementation: typeof fetch = fetch,
): Promise<OidcTokens & { idToken: string }> {
  const tokens = await postToTokenEndpoint(
    options,
    {
      grant_type: 'authorization_code',
      code: parameters.code,
      redirect_uri: getRedirectUri(options),
      code_verifier: parameters.codeVerifier,
    },
    fetchImplementation,
  );
  if (tokens.idToken === null) {
    throw new OidcExchangeError('Phản hồi token thiếu id_token');
  }
  return { ...tokens, idToken: tokens.idToken };
}

/** Đổi refresh token lấy access token mới (API xoay vòng refresh token nên phải lưu cái mới). */
export function refreshTokens(
  options: OidcOptions,
  refreshToken: string,
  fetchImplementation: typeof fetch = fetch,
): Promise<OidcTokens> {
  return postToTokenEndpoint(
    options,
    { grant_type: 'refresh_token', refresh_token: refreshToken },
    fetchImplementation,
  );
}

/** Đọc `name`, `email` từ id_token nhận trực tiếp từ token endpoint qua kết nối tin cậy (OIDC Core 3.1.3.7 cho phép bỏ qua kiểm chữ ký). */
export function readUserClaims(idToken: string): OidcUserClaims {
  const payloadPart = idToken.split('.')[1];
  if (payloadPart === undefined) {
    throw new OidcExchangeError('id_token sai định dạng');
  }
  try {
    const claims = JSON.parse(Buffer.from(payloadPart, 'base64url').toString('utf8')) as Record<
      string,
      unknown
    >;
    const email = readString(claims.email) ?? '';
    return {
      name: readString(claims.name) ?? email,
      email,
      isFirstSignIn: claims.first_sign_in === true,
    };
  } catch (error) {
    throw new OidcExchangeError('id_token sai định dạng', error);
  }
}
