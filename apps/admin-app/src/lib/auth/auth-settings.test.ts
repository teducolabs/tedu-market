import { describe, expect, it } from 'vitest';
import { createUserManagerSettings } from './auth-settings';

describe('createUserManagerSettings', () => {
  it('uses Code + PKCE for the public admin-app client with callback on the app origin', () => {
    // Arrange / Act
    const settings = createUserManagerSettings('http://api.test', 'http://admin.test');

    // Assert
    expect(settings).toMatchObject({
      authority: 'http://api.test',
      client_id: 'admin-app',
      response_type: 'code',
      redirect_uri: 'http://admin.test/auth/callback',
      post_logout_redirect_uri: 'http://admin.test/',
    });
    expect(settings.scope).toContain('offline_access');
  });

  it('keeps tokens in memory instead of browser storage', async () => {
    // Arrange
    const settings = createUserManagerSettings('http://api.test', 'http://admin.test');

    // Act
    await settings.userStore?.set('user', 'secret-token');

    // Assert
    expect(window.localStorage.length).toBe(0);
    expect(window.sessionStorage.length).toBe(0);
  });
});
