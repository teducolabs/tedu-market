// @vitest-environment node
import { NextRequest } from 'next/server';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { PROTECTED_PATH_PREFIXES } from './lib/oidc/protected-paths';
import { buildSessionCookies, readSession, type Session } from './lib/session/session-cookie';
import { config, proxy } from './proxy';

const SESSION_SECRET = 's'.repeat(32);
const SITE = 'https://market.example.com';
const NOW_SECONDS = Math.floor(Date.now() / 1000);

function createSession(overrides: Partial<Session> = {}): Session {
  return {
    accessToken: 'access-1',
    idToken: 'id-1',
    refreshToken: 'refresh-1',
    expiresAt: NOW_SECONDS + 3600,
    name: 'A',
    email: 'a@example.com',
    ...overrides,
  };
}

function requestWith(path: string, session: Session | null): NextRequest {
  const cookie =
    session === null
      ? ''
      : buildSessionCookies(session, SESSION_SECRET)
          .map((item) => `${item.name}=${item.value}`)
          .join('; ');
  return new NextRequest(`${SITE}${path}`, { headers: { cookie } });
}

function readCookies(setCookies: string[]) {
  return (name: string) =>
    setCookies
      .map((line) => line.split(';')[0] ?? '')
      .find((pair) => pair.startsWith(`${name}=`))
      ?.slice(name.length + 1);
}

describe('proxy', () => {
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

  it('redirects guests to sign in and remembers the requested page', async () => {
    // Arrange
    const request = requestWith('/cart?from=header', null);

    // Act
    const response = await proxy(request);

    // Assert
    const location = new URL(response.headers.get('location') ?? '');
    expect(location.pathname).toBe('/sign-in');
    expect(location.searchParams.get('returnTo')).toBe('/cart?from=header');
  });

  it('lets guests through public pages without reading configuration or calling the API', async () => {
    // Arrange
    vi.unstubAllEnvs();
    const fetchMock = vi.fn();
    vi.stubGlobal('fetch', fetchMock);

    // Act
    const response = await proxy(requestWith('/courses', null));

    // Assert
    expect(response.headers.get('location')).toBeNull();
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it('lets signed in students through without calling the API', async () => {
    // Arrange
    const fetchMock = vi.fn();
    vi.stubGlobal('fetch', fetchMock);

    // Act
    const response = await proxy(requestWith('/cart', createSession()));

    // Assert
    expect(response.headers.get('location')).toBeNull();
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it('renews an expired access token with the refresh token and stores the new session', async () => {
    // Arrange
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(
          JSON.stringify({
            access_token: 'access-2',
            refresh_token: 'refresh-2',
            expires_in: 3600,
          }),
        ),
      ),
    );
    const request = requestWith('/cart', createSession({ expiresAt: NOW_SECONDS - 10 }));

    // Act
    const response = await proxy(request);

    // Assert
    expect(response.headers.get('location')).toBeNull();
    const stored = readSession(readCookies(response.headers.getSetCookie()), () => SESSION_SECRET);
    expect(stored).toMatchObject({
      accessToken: 'access-2',
      refreshToken: 'refresh-2',
      idToken: 'id-1',
    });
    expect(response.headers.get('x-middleware-request-cookie')).toContain('tm_session.0=');
    expect(response.headers.getSetCookie().join('\n')).toMatch(/HttpOnly/i);
  });

  it('renews the session on public pages too so the header keeps showing the student', async () => {
    // Arrange
    vi.stubGlobal(
      'fetch',
      vi
        .fn()
        .mockResolvedValue(
          new Response(JSON.stringify({ access_token: 'access-2', expires_in: 3600 })),
        ),
    );

    // Act
    const response = await proxy(
      requestWith('/courses', createSession({ expiresAt: NOW_SECONDS - 10 })),
    );

    // Assert
    const stored = readSession(readCookies(response.headers.getSetCookie()), () => SESSION_SECRET);
    expect(stored?.accessToken).toBe('access-2');
    expect(stored?.refreshToken).toBe('refresh-1');
  });

  it('clears the session and sends protected pages to sign in when refresh is rejected', async () => {
    // Arrange
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('{}', { status: 400 })));
    const request = requestWith('/cart', createSession({ expiresAt: NOW_SECONDS - 10 }));

    // Act
    const response = await proxy(request);

    // Assert
    expect(new URL(response.headers.get('location') ?? '').pathname).toBe('/sign-in');
    expect(response.headers.getSetCookie().some((line) => line.startsWith('tm_session.0=;'))).toBe(
      true,
    );
  });

  it('clears the session and shows public pages as guest when refresh is rejected', async () => {
    // Arrange
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('{}', { status: 400 })));
    const request = requestWith('/courses', createSession({ expiresAt: NOW_SECONDS - 10 }));

    // Act
    const response = await proxy(request);

    // Assert
    expect(response.headers.get('location')).toBeNull();
    expect(response.headers.getSetCookie().some((line) => line.startsWith('tm_session.0=;'))).toBe(
      true,
    );
  });

  it('treats an expired session without refresh token as guest', async () => {
    // Arrange
    const fetchMock = vi.fn();
    vi.stubGlobal('fetch', fetchMock);
    const request = requestWith(
      '/cart',
      createSession({ expiresAt: NOW_SECONDS - 10, refreshToken: null }),
    );

    // Act
    const response = await proxy(request);

    // Assert
    expect(new URL(response.headers.get('location') ?? '').pathname).toBe('/sign-in');
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it('keeps protected path prefixes in line with the matcher exclusions', () => {
    // Arrange / Act
    const isExcluded = (path: string) => !new RegExp(`^${config.matcher[0] ?? ''}$`).test(path);

    // Assert
    expect(PROTECTED_PATH_PREFIXES.every((prefix) => !isExcluded(`${prefix}/x`))).toBe(true);
    expect(
      ['/sign-in', '/sign-in/callback', '/sign-out', '/_next/static/a.js'].every(isExcluded),
    ).toBe(true);
  });
});
