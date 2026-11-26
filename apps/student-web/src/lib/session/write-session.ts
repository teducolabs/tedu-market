import type { NextResponse } from 'next/server';
import type { OidcOptions } from '../oidc/oidc-options';
import { buildCookieAttributes } from './cookie-attributes';
import {
  SESSION_COOKIE_MAX_AGE_SECONDS,
  allSessionCookieNames,
  buildSessionCookies,
  type CookieWrite,
  type Session,
} from './session-cookie';

/** Ghi phiên vào response; xóa các cookie chunk thừa của phiên cũ. Trả cookie đã ghi để proxy chuyển tiếp cho request hiện tại. */
export function writeSessionCookies(
  response: NextResponse,
  options: OidcOptions,
  session: Session,
  accessTokenLifetimeSeconds: number,
): CookieWrite[] {
  const cookies = buildSessionCookies(session, options.sessionSecret);
  const maxAge =
    session.refreshToken === null ? accessTokenLifetimeSeconds : SESSION_COOKIE_MAX_AGE_SECONDS;
  for (const cookie of cookies) {
    response.cookies.set(cookie.name, cookie.value, buildCookieAttributes(options, maxAge));
  }
  const written = new Set(cookies.map((cookie) => cookie.name));
  for (const name of allSessionCookieNames().filter((candidate) => !written.has(candidate))) {
    response.cookies.set(name, '', buildCookieAttributes(options, 0));
  }
  return cookies;
}

export function clearSessionCookies(response: NextResponse, options: OidcOptions): void {
  for (const name of allSessionCookieNames()) {
    response.cookies.set(name, '', buildCookieAttributes(options, 0));
  }
}
