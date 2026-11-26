import { describe, expect, it, vi } from 'vitest';
import {
  OidcExchangeError,
  buildAuthorizeUrl,
  buildEndSessionUrl,
  createCodeChallenge,
  exchangeCodeForTokens,
  readUserClaims,
  refreshTokens,
} from './code-flow';
import type { OidcOptions } from './oidc-options';

const OPTIONS: OidcOptions = {
  apiUrl: 'https://api.example.com',
  siteUrl: 'https://market.example.com',
  clientId: 'student-web',
  clientSecret: 'client-secret',
  sessionSecret: 's'.repeat(32),
};

function toIdToken(claims: Record<string, unknown>): string {
  return `header.${Buffer.from(JSON.stringify(claims)).toString('base64url')}.signature`;
}

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), { status });
}

describe('createCodeChallenge', () => {
  it('matches the RFC 7636 S256 example', () => {
    // Arrange
    const codeVerifier = 'dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk';

    // Act
    const codeChallenge = createCodeChallenge(codeVerifier);

    // Assert
    expect(codeChallenge).toBe('E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM');
  });
});

describe('buildAuthorizeUrl', () => {
  it('requests code flow with PKCE and Google as identity provider', () => {
    // Arrange / Act
    const url = buildAuthorizeUrl(OPTIONS, { state: 'state-1', codeVerifier: 'verifier-1' });

    // Assert
    expect(url.origin + url.pathname).toBe('https://api.example.com/connect/authorize');
    expect(Object.fromEntries(url.searchParams)).toEqual({
      response_type: 'code',
      client_id: 'student-web',
      redirect_uri: 'https://market.example.com/sign-in/callback',
      scope: 'openid profile email offline_access',
      state: 'state-1',
      code_challenge: createCodeChallenge('verifier-1'),
      code_challenge_method: 'S256',
      identity_provider: 'google',
    });
  });
});

describe('buildEndSessionUrl', () => {
  it('sends id token hint and returns to home', () => {
    // Arrange / Act
    const url = buildEndSessionUrl(OPTIONS, 'id-token');

    // Assert
    expect(url.origin + url.pathname).toBe('https://api.example.com/connect/logout');
    expect(url.searchParams.get('id_token_hint')).toBe('id-token');
    expect(url.searchParams.get('post_logout_redirect_uri')).toBe('https://market.example.com/');
  });
});

describe('exchangeCodeForTokens', () => {
  it('posts code, verifier and client secret to the token endpoint', async () => {
    // Arrange
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(
      jsonResponse({
        access_token: 'access',
        id_token: 'id',
        refresh_token: 'refresh',
        expires_in: 3600,
      }),
    );

    // Act
    const tokens = await exchangeCodeForTokens(
      OPTIONS,
      { code: 'code-1', codeVerifier: 'verifier-1' },
      fetchMock,
    );

    // Assert
    expect(tokens).toEqual({
      accessToken: 'access',
      idToken: 'id',
      refreshToken: 'refresh',
      expiresInSeconds: 3600,
    });
    const [url, init] = fetchMock.mock.calls[0] ?? [];
    expect(url).toBe('https://api.example.com/connect/token');
    const body = init?.body as URLSearchParams;
    expect(Object.fromEntries(body)).toMatchObject({
      grant_type: 'authorization_code',
      code: 'code-1',
      code_verifier: 'verifier-1',
      client_secret: 'client-secret',
      redirect_uri: 'https://market.example.com/sign-in/callback',
    });
  });

  it('throws OidcExchangeError when token endpoint rejects the code', async () => {
    // Arrange
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(jsonResponse({}, 400));

    // Act
    const act = exchangeCodeForTokens(OPTIONS, { code: 'c', codeVerifier: 'v' }, fetchMock);

    // Assert
    await expect(act).rejects.toBeInstanceOf(OidcExchangeError);
  });

  it('throws OidcExchangeError when the network fails', async () => {
    // Arrange
    const fetchMock = vi.fn<typeof fetch>().mockRejectedValue(new TypeError('network'));

    // Act
    const act = exchangeCodeForTokens(OPTIONS, { code: 'c', codeVerifier: 'v' }, fetchMock);

    // Assert
    await expect(act).rejects.toBeInstanceOf(OidcExchangeError);
  });

  it('throws OidcExchangeError when response has no id token', async () => {
    // Arrange
    const fetchMock = vi
      .fn<typeof fetch>()
      .mockResolvedValue(jsonResponse({ access_token: 'access', expires_in: 3600 }));

    // Act
    const act = exchangeCodeForTokens(OPTIONS, { code: 'c', codeVerifier: 'v' }, fetchMock);

    // Assert
    await expect(act).rejects.toBeInstanceOf(OidcExchangeError);
  });
});

describe('refreshTokens', () => {
  it('posts the refresh token grant and returns the rotated refresh token', async () => {
    // Arrange
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(
      jsonResponse({
        access_token: 'new-access',
        refresh_token: 'new-refresh',
        expires_in: 3600,
      }),
    );

    // Act
    const tokens = await refreshTokens(OPTIONS, 'old-refresh', fetchMock);

    // Assert
    expect(tokens).toEqual({
      accessToken: 'new-access',
      idToken: null,
      refreshToken: 'new-refresh',
      expiresInSeconds: 3600,
    });
    const body = fetchMock.mock.calls[0]?.[1]?.body as URLSearchParams;
    expect(Object.fromEntries(body)).toMatchObject({
      grant_type: 'refresh_token',
      refresh_token: 'old-refresh',
      client_secret: 'client-secret',
    });
  });

  it('throws OidcExchangeError when the refresh token is rejected', async () => {
    // Arrange
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(jsonResponse({}, 400));

    // Act
    const act = refreshTokens(OPTIONS, 'old-refresh', fetchMock);

    // Assert
    await expect(act).rejects.toBeInstanceOf(OidcExchangeError);
  });
});

describe('readUserClaims', () => {
  it('reads name and email from the id token', () => {
    // Arrange
    const idToken = toIdToken({ name: 'Nguyễn Văn A', email: 'a@example.com' });

    // Act
    const claims = readUserClaims(idToken);

    // Assert
    expect(claims).toEqual({
      name: 'Nguyễn Văn A',
      email: 'a@example.com',
      isFirstSignIn: false,
    });
  });

  it('uses email as name when name claim is missing', () => {
    // Arrange / Act
    const claims = readUserClaims(toIdToken({ email: 'a@example.com' }));

    // Assert
    expect(claims).toMatchObject({ name: 'a@example.com', email: 'a@example.com' });
  });

  it('reports the first sign in when the claim is true', () => {
    // Arrange / Act
    const claims = readUserClaims(toIdToken({ email: 'a@example.com', first_sign_in: true }));

    // Assert
    expect(claims.isFirstSignIn).toBe(true);
  });

  it('throws OidcExchangeError when token is malformed', () => {
    // Arrange / Act
    const act = () => readUserClaims('khong-co-dau-cham');

    // Assert
    expect(act).toThrow(OidcExchangeError);
  });
});
