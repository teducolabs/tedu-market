import { describe, expect, it } from 'vitest';
import { getOidcOptions, getRedirectUri } from './oidc-options';

const VALID_ENVIRONMENT = {
  API_URL: 'https://api.example.com/',
  SITE_URL: 'https://market.example.com/',
  StudentWeb__OidcClientSecret: 'client-secret',
  StudentWeb__SessionSecret: 's'.repeat(32),
};

describe('getOidcOptions', () => {
  it('reads values and trims trailing slashes', () => {
    // Arrange / Act
    const options = getOidcOptions(VALID_ENVIRONMENT);

    // Assert
    expect(options).toMatchObject({
      apiUrl: 'https://api.example.com',
      siteUrl: 'https://market.example.com',
      clientId: 'student-web',
    });
    expect(getRedirectUri(options)).toBe('https://market.example.com/sign-in/callback');
  });

  it('falls back to localhost when SITE_URL is missing', () => {
    // Arrange / Act
    const options = getOidcOptions({ ...VALID_ENVIRONMENT, SITE_URL: undefined });

    // Assert
    expect(options.siteUrl).toBe('http://localhost:3000');
  });

  it.each(['API_URL', 'StudentWeb__OidcClientSecret', 'StudentWeb__SessionSecret'])(
    'throws when %s is missing',
    (name) => {
      // Arrange
      const environment = { ...VALID_ENVIRONMENT, [name]: undefined };

      // Act
      const act = () => getOidcOptions(environment);

      // Assert
      expect(act).toThrow(name);
    },
  );

  it('throws when session secret is too short', () => {
    // Arrange / Act
    const act = () => getOidcOptions({ ...VALID_ENVIRONMENT, StudentWeb__SessionSecret: 'ngan' });

    // Assert
    expect(act).toThrow('ít nhất 32 ký tự');
  });
});
