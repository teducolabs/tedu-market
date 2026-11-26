import { NextResponse, type NextRequest } from 'next/server';
import { OidcExchangeError, refreshTokens } from './lib/oidc/code-flow';
import { getOidcOptions, SIGN_IN_PATH } from './lib/oidc/oidc-options';
import { PROTECTED_PATH_PREFIXES } from './lib/oidc/protected-paths';
import {
  allSessionCookieNames,
  readStoredSession,
  type Session,
} from './lib/session/session-cookie';
import { clearSessionCookies, writeSessionCookies } from './lib/session/write-session';

/** Gia hạn sớm vài giây để token không hết hạn giữa chừng khi request tới API. */
const EXPIRY_SKEW_SECONDS = 30;

function isProtectedPath(pathname: string): boolean {
  return PROTECTED_PATH_PREFIXES.some(
    (prefix) => pathname === prefix || pathname.startsWith(`${prefix}/`),
  );
}

function redirectToSignIn(request: NextRequest, siteUrl: string): NextResponse {
  const signInUrl = new URL(SIGN_IN_PATH, siteUrl);
  signInUrl.searchParams.set('returnTo', `${request.nextUrl.pathname}${request.nextUrl.search}`);
  return NextResponse.redirect(signInUrl);
}

/**
 * Chạy trước mọi trang: giữ phiên đăng nhập (gia hạn bằng refresh token khi access token hết hạn, T83) và chuyển Khách
 * vào trang cần đăng nhập sang Đăng nhập bằng Google rồi quay lại đúng trang (FR-03). Khách không có cookie thì không làm gì.
 */
export async function proxy(request: NextRequest) {
  const getCookie = (name: string) => request.cookies.get(name)?.value;
  const hasSessionCookie = allSessionCookieNames().some((name) => getCookie(name) !== undefined);
  if (!hasSessionCookie && !isProtectedPath(request.nextUrl.pathname)) {
    return NextResponse.next();
  }

  const options = getOidcOptions();
  const stored = readStoredSession(getCookie, () => options.sessionSecret);
  const nowSeconds = Math.floor(Date.now() / 1000);
  if (stored !== null && stored.expiresAt - EXPIRY_SKEW_SECONDS > nowSeconds) {
    return NextResponse.next();
  }

  if (stored !== null && stored.refreshToken !== null) {
    try {
      const tokens = await refreshTokens(options, stored.refreshToken);
      const renewed: Session = {
        ...stored,
        accessToken: tokens.accessToken,
        idToken: tokens.idToken ?? stored.idToken,
        // API xoay vòng refresh token; không trả cái mới thì giữ cái cũ.
        refreshToken: tokens.refreshToken ?? stored.refreshToken,
        expiresAt: nowSeconds + tokens.expiresInSeconds,
      };
      // Cookie mới cho cả request hiện tại (Server Component đọc được) lẫn trình duyệt.
      const response = NextResponse.next();
      for (const cookie of writeSessionCookies(
        response,
        options,
        renewed,
        tokens.expiresInSeconds,
      )) {
        request.cookies.set(cookie.name, cookie.value);
      }
      return NextResponse.next({ request, headers: response.headers });
    } catch (error) {
      if (!(error instanceof OidcExchangeError)) {
        throw error;
      }
    }
  }

  // Không có phiên, hoặc refresh bị từ chối: xóa cookie phiên.
  const response = isProtectedPath(request.nextUrl.pathname)
    ? redirectToSignIn(request, options.siteUrl)
    : NextResponse.next();
  clearSessionCookies(response, options);
  return response;
}

/** Mọi trang trừ file tĩnh và các route đăng nhập/đăng xuất (đã tự xử lý phiên). */
export const config = {
  matcher: ['/((?!_next/|favicon.ico|robots.txt|sign-in|sign-out).*)'],
};
