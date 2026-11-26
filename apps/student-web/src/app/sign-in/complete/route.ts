import { NextResponse, type NextRequest } from 'next/server';
import {
  OidcExchangeError,
  exchangeCodeForTokens,
  readUserClaims,
} from '../../../lib/oidc/code-flow';
import { getOidcOptions, SIGN_IN_PATH, type OidcOptions } from '../../../lib/oidc/oidc-options';
import { withSignInFailed, withWelcome } from '../../../lib/oidc/return-path';
import { toSignInState, type SignInState } from '../../../lib/oidc/sign-in-state';
import { buildCookieAttributes } from '../../../lib/session/cookie-attributes';
import {
  SIGN_IN_STATE_COOKIE,
  unsealJson,
  type Session,
} from '../../../lib/session/session-cookie';
import { writeSessionCookies } from '../../../lib/session/write-session';

export const dynamic = 'force-dynamic';

function redirectTo(options: OidcOptions, path: string): NextResponse {
  const response = NextResponse.redirect(new URL(path, options.siteUrl));
  response.cookies.set(SIGN_IN_STATE_COOKIE, '', buildCookieAttributes(options, 0, SIGN_IN_PATH));
  return response;
}

function fail(options: OidcOptions, signInState: SignInState | null): NextResponse {
  return redirectTo(options, withSignInFailed(signInState?.returnPath ?? '/'));
}

/** Nhận `code` từ API: đối chiếu `state`, đổi token, lưu phiên vào cookie httpOnly mã hóa rồi về trang trước đó. Hủy hoặc lỗi thì về trang cũ kèm banner. */
export async function GET(request: NextRequest) {
  const options = getOidcOptions();
  const sealedState = request.cookies.get(SIGN_IN_STATE_COOKIE)?.value;
  const signInState =
    sealedState === undefined
      ? null
      : toSignInState(unsealJson(sealedState, options.sessionSecret));

  const parameters = request.nextUrl.searchParams;
  const code = parameters.get('code');
  const isStateValid = signInState !== null && parameters.get('state') === signInState.state;
  if (!isStateValid || parameters.has('error') || code === null || code === '') {
    return fail(options, isStateValid ? signInState : null);
  }

  try {
    const tokens = await exchangeCodeForTokens(options, {
      code,
      codeVerifier: signInState.codeVerifier,
    });
    const claims = readUserClaims(tokens.idToken);
    const session: Session = {
      accessToken: tokens.accessToken,
      idToken: tokens.idToken,
      refreshToken: tokens.refreshToken,
      expiresAt: Math.floor(Date.now() / 1000) + tokens.expiresInSeconds,
      name: claims.name,
      email: claims.email,
    };
    const response = redirectTo(
      options,
      claims.isFirstSignIn ? withWelcome(signInState.returnPath) : signInState.returnPath,
    );
    writeSessionCookies(response, options, session, tokens.expiresInSeconds);
    return response;
  } catch (error) {
    if (error instanceof OidcExchangeError) {
      return fail(options, signInState);
    }
    throw error;
  }
}
