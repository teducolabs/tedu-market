import { describe, expect, it } from 'vitest';
import {
  COOKIE_CHUNK_SIZE,
  allSessionCookieNames,
  buildSessionCookies,
  readSession,
  readStoredSession,
  sealJson,
  unsealJson,
  type Session,
} from './session-cookie';

const SECRET = 'a'.repeat(32);
const NOW_SECONDS = 1_800_000_000;

function createSession(overrides: Partial<Session> = {}): Session {
  return {
    accessToken: 'access-token',
    idToken: 'id-token',
    refreshToken: 'refresh-token',
    expiresAt: NOW_SECONDS + 3600,
    name: 'Học viên',
    email: 'hoc.vien@example.com',
    ...overrides,
  };
}

function toCookieReader(cookies: { name: string; value: string }[]) {
  return (name: string) => cookies.find((cookie) => cookie.name === name)?.value;
}

describe('sealJson / unsealJson', () => {
  it('returns original value when secret matches', () => {
    // Arrange
    const sealed = sealJson({ hello: 'xin chào' }, SECRET);

    // Act
    const value = unsealJson(sealed, SECRET);

    // Assert
    expect(value).toEqual({ hello: 'xin chào' });
  });

  it('does not expose plain text in sealed value', () => {
    // Arrange / Act
    const sealed = sealJson({ accessToken: 'very-secret-token' }, SECRET);

    // Assert
    expect(sealed).not.toContain('very-secret-token');
    expect(Buffer.from(sealed, 'base64url').toString('utf8')).not.toContain('very-secret-token');
  });

  it('returns null when secret differs', () => {
    // Arrange
    const sealed = sealJson({ a: 1 }, SECRET);

    // Act
    const value = unsealJson(sealed, 'b'.repeat(32));

    // Assert
    expect(value).toBeNull();
  });

  it('returns null when sealed value was tampered', () => {
    // Arrange
    const sealed = sealJson({ a: 1 }, SECRET);
    const tampered = `${sealed.slice(0, -2)}${sealed.endsWith('AA') ? 'BB' : 'AA'}`;

    // Act
    const value = unsealJson(tampered, SECRET);

    // Assert
    expect(value).toBeNull();
  });

  it('returns null when value is not a sealed string', () => {
    // Arrange / Act
    const value = unsealJson('khong-phai-chuoi-ma-hoa', SECRET);

    // Assert
    expect(value).toBeNull();
  });
});

describe('buildSessionCookies / readSession', () => {
  it('round-trips a session through cookies', () => {
    // Arrange
    const session = createSession();
    const cookies = buildSessionCookies(session, SECRET);

    // Act
    const result = readSession(toCookieReader(cookies), () => SECRET, NOW_SECONDS);

    // Assert
    expect(result).toEqual(session);
  });

  it('splits a large session into chunks under the cookie size limit', () => {
    // Arrange
    const session = createSession({ accessToken: 'x'.repeat(6000) });

    // Act
    const cookies = buildSessionCookies(session, SECRET);

    // Assert
    expect(cookies.length).toBeGreaterThan(1);
    expect(cookies.every((cookie) => cookie.value.length <= COOKIE_CHUNK_SIZE)).toBe(true);
    expect(readSession(toCookieReader(cookies), () => SECRET, NOW_SECONDS)).toEqual(session);
  });

  it('throws when session is too large for cookies', () => {
    // Arrange
    const session = createSession({ accessToken: 'x'.repeat(40_000) });

    // Act
    const act = () => buildSessionCookies(session, SECRET);

    // Assert
    expect(act).toThrow('quá lớn');
  });

  it('returns null without reading the secret when there is no cookie', () => {
    // Arrange
    const secret = () => {
      throw new Error('secret should not be read');
    };

    // Act
    const result = readSession(() => undefined, secret, NOW_SECONDS);

    // Assert
    expect(result).toBeNull();
  });

  it('returns null when session is expired', () => {
    // Arrange
    const cookies = buildSessionCookies(createSession({ expiresAt: NOW_SECONDS - 1 }), SECRET);

    // Act
    const result = readSession(toCookieReader(cookies), () => SECRET, NOW_SECONDS);

    // Assert
    expect(result).toBeNull();
  });

  it('returns null when a chunk is missing', () => {
    // Arrange
    const cookies = buildSessionCookies(createSession({ accessToken: 'x'.repeat(6000) }), SECRET);

    // Act
    const result = readSession(toCookieReader(cookies.slice(1)), () => SECRET, NOW_SECONDS);

    // Assert
    expect(result).toBeNull();
  });
});

describe('readStoredSession', () => {
  it('returns an expired session so it can be renewed', () => {
    // Arrange
    const session = createSession({ expiresAt: NOW_SECONDS - 100 });
    const cookies = buildSessionCookies(session, SECRET);

    // Act
    const result = readStoredSession(toCookieReader(cookies), () => SECRET);

    // Assert
    expect(result).toEqual(session);
  });

  it('treats a session without refresh token as having none', () => {
    // Arrange
    const sealed = sealJson(
      { accessToken: 'a', idToken: 'i', expiresAt: NOW_SECONDS, name: 'A', email: 'a@x.com' },
      SECRET,
    );

    // Act
    const result = readStoredSession(
      (name) => (name === 'tm_session.0' ? sealed : undefined),
      () => SECRET,
    );

    // Assert
    expect(result?.refreshToken).toBeNull();
  });
});

describe('allSessionCookieNames', () => {
  it('covers every chunk name that buildSessionCookies can write', () => {
    // Arrange
    const written = buildSessionCookies(createSession({ accessToken: 'x'.repeat(20_000) }), SECRET);

    // Act
    const names = allSessionCookieNames();

    // Assert
    expect(written.every((cookie) => names.includes(cookie.name))).toBe(true);
  });
});
