import { describe, expect, it } from 'vitest';
import { readReturnPath, sanitizeReturnPath } from './return-path';

describe('sanitizeReturnPath', () => {
  it('keeps an internal path with query', () => {
    // Arrange / Act
    const path = sanitizeReturnPath('/orders?page=2');

    // Assert
    expect(path).toBe('/orders?page=2');
  });

  it.each([
    ['https://evil.test'],
    ['//evil.test'],
    ['/\\evil.test'],
    ['/a\nb'],
    ['orders'],
    ['/sign-in'],
    ['/auth/callback?code=1'],
    [null],
    [42],
  ])('falls back to root for %j', (rawPath) => {
    // Arrange / Act
    const path = sanitizeReturnPath(rawPath);

    // Assert
    expect(path).toBe('/');
  });
});

describe('readReturnPath', () => {
  it('reads returnTo from the sign-in state', () => {
    // Arrange / Act
    const path = readReturnPath({ returnTo: '/coupons' });

    // Assert
    expect(path).toBe('/coupons');
  });

  it.each([[undefined], [null], ['x'], [{}], [{ returnTo: '//evil.test' }]])(
    'returns root for state %j',
    (state) => {
      // Arrange / Act
      const path = readReturnPath(state);

      // Assert
      expect(path).toBe('/');
    },
  );
});
