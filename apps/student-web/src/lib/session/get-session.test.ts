// @vitest-environment node
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { getSession, getSessionUser } from './get-session';
import { buildSessionCookies } from './session-cookie';

const SESSION_SECRET = 's'.repeat(32);
let cookieJar = new Map<string, string>();

vi.mock('next/headers', () => ({
  cookies: () =>
    Promise.resolve({
      get: (name: string) => {
        const value = cookieJar.get(name);
        return value === undefined ? undefined : { name, value };
      },
    }),
}));

describe('getSession', () => {
  beforeEach(() => {
    cookieJar = new Map();
    vi.stubEnv('API_URL', 'https://api.example.com');
    vi.stubEnv('StudentWeb__OidcClientSecret', 'client-secret');
    vi.stubEnv('StudentWeb__SessionSecret', SESSION_SECRET);
  });

  afterEach(() => {
    vi.unstubAllEnvs();
  });

  it('returns null for guests without needing configuration', async () => {
    // Arrange
    vi.unstubAllEnvs();

    // Act
    const session = await getSession();

    // Assert
    expect(session).toBeNull();
  });

  it('returns name and email only for the signed in student', async () => {
    // Arrange
    for (const cookie of buildSessionCookies(
      {
        accessToken: 'secret-access',
        idToken: 'i',
        refreshToken: null,
        expiresAt: Math.floor(Date.now() / 1000) + 3600,
        name: 'Học viên A',
        email: 'a@example.com',
      },
      SESSION_SECRET,
    )) {
      cookieJar.set(cookie.name, cookie.value);
    }

    // Act
    const user = await getSessionUser();

    // Assert
    expect(user).toEqual({ name: 'Học viên A', email: 'a@example.com' });
  });
});
