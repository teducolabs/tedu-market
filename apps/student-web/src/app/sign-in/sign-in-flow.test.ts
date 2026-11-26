// @vitest-environment node
import { NextRequest } from 'next/server';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { POST as signOut } from '../sign-out/route';
import { GET as callback } from './complete/route';
import { GET as signIn } from './route';
import { buildSessionCookies, readSession } from '../../lib/session/session-cookie';

const SESSION_SECRET = 's'.repeat(32);
const SITE = 'https://market.example.com';

function toIdToken(claims: Record<string, unknown>): string {
  return `h.${Buffer.from(JSON.stringify(claims)).toString('base64url')}.s`;
}

function cookieHeader(setCookies: string[], names: string[]): string {
  return setCookies
    .map((line) => line.split(';')[0] ?? '')
    .filter((pair) => names.some((name) => pair.startsWith(name)))
    .join('; ');
}

/** Chạy /sign-in và trả state, cookie tạm, URL authorize như trình duyệt sẽ thấy. */
function startSignIn(returnTo: string) {
  const response = signIn(
    new NextRequest(`${SITE}/sign-in?returnTo=${encodeURIComponent(returnTo)}`),
  );
  const location = new URL(response.headers.get('location') ?? '');
  return {
    response,
    location,
    state: location.searchParams.get('state') ?? '',
    cookie: cookieHeader(response.headers.getSetCookie(), ['tm_sign_in']),
  };
}

function callbackRequest(search: string, cookie: string) {
  return new NextRequest(`${SITE}/sign-in/complete?${search}`, { headers: { cookie } });
}

describe('sign-in flow', () => {
  beforeEach(() => {
    vi.stubEnv('API_URL', 'https://api.example.com');
    vi.stubEnv('SITE_URL', SITE);
    vi.stubEnv('StudentWeb__OidcClientSecret', 'client-secret');
    vi.stubEnv('StudentWeb__SessionSecret', SESSION_SECRET);
  });

  afterEach(() => {
    vi.unstubAllEnvs();
    vi.unstubAllGlobals();
  });

  describe('GET /sign-in', () => {
    it('redirects to the API authorize endpoint with Google and a httpOnly state cookie', () => {
      // Arrange / Act
      const { response, location } = startSignIn('/cart');

      // Assert
      expect(response.status).toBe(307);
      expect(location.origin + location.pathname).toBe('https://api.example.com/connect/authorize');
      expect(location.searchParams.get('identity_provider')).toBe('google');
      expect(location.searchParams.get('code_challenge_method')).toBe('S256');
      const setCookie = response.headers.getSetCookie().join('\n');
      expect(setCookie).toMatch(/tm_sign_in=/);
      expect(setCookie).toMatch(/HttpOnly/i);
      expect(setCookie).toMatch(/SameSite=lax/i);
      expect(setCookie).toMatch(/Secure/i);
    });

    it('does not put the return path or verifier in clear text in the cookie', () => {
      // Arrange / Act
      const { cookie } = startSignIn('/cart');

      // Assert
      expect(cookie).not.toContain('cart');
    });
  });

  describe('GET /sign-in/complete', () => {
    it('stores an encrypted session and returns to the original page when sign in succeeds', async () => {
      // Arrange
      const { state, cookie } = startSignIn('/cart');
      const idToken = toIdToken({ name: 'Học viên A', email: 'a@example.com' });
      vi.stubGlobal(
        'fetch',
        vi.fn().mockResolvedValue(
          new Response(
            JSON.stringify({
              access_token: 'access-1',
              id_token: idToken,
              refresh_token: 'refresh-1',
              expires_in: 3600,
            }),
          ),
        ),
      );

      // Act
      const response = await callback(callbackRequest(`code=abc&state=${state}`, cookie));

      // Assert
      expect(response.headers.get('location')).toBe(`${SITE}/cart`);
      const setCookies = response.headers.getSetCookie();
      const sessionCookies = cookieHeader(setCookies, ['tm_session']);
      expect(sessionCookies).not.toContain('access-1');
      expect(setCookies.filter((line) => line.startsWith('tm_session.'))).toSatisfy(
        (lines: string[]) => lines.every((line) => /HttpOnly/i.test(line)),
      );
      const session = readSession(
        (name) =>
          sessionCookies
            .split('; ')
            .find((pair) => pair.startsWith(`${name}=`))
            ?.slice(name.length + 1),
        () => SESSION_SECRET,
      );
      expect(session).toMatchObject({
        accessToken: 'access-1',
        refreshToken: 'refresh-1',
        name: 'Học viên A',
      });
    });

    it('adds the welcome flag to the return path on the first sign in', async () => {
      // Arrange
      const { state, cookie } = startSignIn('/courses?search=react');
      const idToken = toIdToken({ name: 'A', email: 'a@example.com', first_sign_in: true });
      vi.stubGlobal(
        'fetch',
        vi
          .fn()
          .mockResolvedValue(
            new Response(
              JSON.stringify({ access_token: 'a', id_token: idToken, expires_in: 3600 }),
            ),
          ),
      );

      // Act
      const response = await callback(callbackRequest(`code=abc&state=${state}`, cookie));

      // Assert
      expect(response.headers.get('location')).toBe(`${SITE}/courses?search=react&welcome=1`);
    });

    it('returns to the original page with failed flag when user cancels at Google', async () => {
      // Arrange
      const { state, cookie } = startSignIn('/cart');

      // Act
      const response = await callback(
        callbackRequest(`error=access_denied&state=${state}`, cookie),
      );

      // Assert
      expect(response.headers.get('location')).toBe(`${SITE}/cart?signIn=failed`);
      expect(cookieHeader(response.headers.getSetCookie(), ['tm_session'])).toBe('');
    });

    it('returns home with failed flag when state does not match', async () => {
      // Arrange
      const { cookie } = startSignIn('/cart');

      // Act
      const response = await callback(callbackRequest('code=abc&state=sai-state', cookie));

      // Assert
      expect(response.headers.get('location')).toBe(`${SITE}/?signIn=failed`);
    });

    it('returns home with failed flag when state cookie is missing', async () => {
      // Arrange / Act
      const response = await callback(callbackRequest('code=abc&state=x', ''));

      // Assert
      expect(response.headers.get('location')).toBe(`${SITE}/?signIn=failed`);
    });

    it('returns to the original page with failed flag when token exchange fails', async () => {
      // Arrange
      const { state, cookie } = startSignIn('/my-courses');
      vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('{}', { status: 400 })));

      // Act
      const response = await callback(callbackRequest(`code=abc&state=${state}`, cookie));

      // Assert
      expect(response.headers.get('location')).toBe(`${SITE}/my-courses?signIn=failed`);
      expect(cookieHeader(response.headers.getSetCookie(), ['tm_session'])).toBe('');
    });
  });

  describe('POST /sign-out', () => {
    it('clears session cookies and sends the id token to the API logout endpoint', () => {
      // Arrange
      const cookies = buildSessionCookies(
        {
          accessToken: 'access-1',
          idToken: 'id-1',
          refreshToken: null,
          expiresAt: Math.floor(Date.now() / 1000) + 3600,
          name: 'A',
          email: 'a@example.com',
        },
        SESSION_SECRET,
      );
      const cookie = cookies.map((item) => `${item.name}=${item.value}`).join('; ');

      // Act
      const response = signOut(
        new NextRequest(`${SITE}/sign-out`, { method: 'POST', headers: { cookie } }),
      );

      // Assert
      const location = new URL(response.headers.get('location') ?? '');
      expect(response.status).toBe(303);
      expect(location.origin + location.pathname).toBe('https://api.example.com/connect/logout');
      expect(location.searchParams.get('id_token_hint')).toBe('id-1');
      expect(
        response.headers.getSetCookie().some((line) => line.startsWith('tm_session.0=;')),
      ).toBe(true);
    });

    it('goes home when there is no session', () => {
      // Arrange / Act
      const response = signOut(new NextRequest(`${SITE}/sign-out`, { method: 'POST' }));

      // Assert
      expect(response.headers.get('location')).toBe(`${SITE}/`);
    });
  });
});
