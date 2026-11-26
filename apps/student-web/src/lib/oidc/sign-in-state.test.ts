import { describe, expect, it } from 'vitest';
import { toSignInState } from './sign-in-state';

describe('toSignInState', () => {
  it('keeps valid state and sanitizes the return path', () => {
    // Arrange / Act
    const result = toSignInState({ state: 's', codeVerifier: 'v', returnPath: 'https://evil.com' });

    // Assert
    expect(result).toEqual({ state: 's', codeVerifier: 'v', returnPath: '/' });
  });

  it.each([[null], [{}], [{ state: 's' }], [{ state: '', codeVerifier: 'v' }]])(
    'returns null for %j',
    (value) => {
      // Arrange / Act
      const result = toSignInState(value);

      // Assert
      expect(result).toBeNull();
    },
  );
});
