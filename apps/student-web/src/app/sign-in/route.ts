import { NextResponse, type NextRequest } from 'next/server';
import { buildAuthorizeUrl, createRandomToken } from '../../lib/oidc/code-flow';
import { getOidcOptions, SIGN_IN_PATH } from '../../lib/oidc/oidc-options';
import { sanitizeReturnPath } from '../../lib/oidc/return-path';
import { SIGN_IN_STATE_MAX_AGE_SECONDS } from '../../lib/oidc/sign-in-state';
import { buildCookieAttributes } from '../../lib/session/cookie-attributes';
import { SIGN_IN_STATE_COOKIE, sealJson } from '../../lib/session/session-cookie';

export const dynamic = 'force-dynamic';

/** Bắt đầu Đăng nhập bằng Google: Authorization Code + PKCE, nhớ trang cần quay lại (FR-03, NFR-06). */
export function GET(request: NextRequest) {
  const options = getOidcOptions();
  const state = createRandomToken();
  const codeVerifier = createRandomToken();
  const returnPath = sanitizeReturnPath(request.nextUrl.searchParams.get('returnTo'));

  const response = NextResponse.redirect(buildAuthorizeUrl(options, { state, codeVerifier }));
  response.cookies.set(
    SIGN_IN_STATE_COOKIE,
    sealJson({ state, codeVerifier, returnPath }, options.sessionSecret),
    buildCookieAttributes(options, SIGN_IN_STATE_MAX_AGE_SECONDS, SIGN_IN_PATH),
  );
  return response;
}
