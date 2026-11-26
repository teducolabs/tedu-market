import { describe, expect, it } from 'vitest';
import {
  sanitizeReturnPath,
  withSignInFailed,
  withWelcome,
  withoutSignInFailed,
  withoutWelcome,
} from './return-path';

describe('sanitizeReturnPath', () => {
  it.each([
    ['/cart', '/cart'],
    ['/courses?search=react#top', '/courses?search=react#top'],
  ])('keeps internal path %s', (rawPath, expected) => {
    // Arrange / Act
    const result = sanitizeReturnPath(rawPath);

    // Assert
    expect(result).toBe(expected);
  });

  it.each([
    ['https://evil.example/cart'],
    ['//evil.example'],
    ['/\\evil.example'],
    ['cart'],
    ['/a\nb'],
    ['/sign-in?returnTo=/cart'],
    ['/sign-out'],
    [''],
  ])('falls back to home for %j', (rawPath) => {
    // Arrange / Act
    const result = sanitizeReturnPath(rawPath);

    // Assert
    expect(result).toBe('/');
  });

  it('falls back to home when value is missing', () => {
    // Arrange / Act / Assert
    expect(sanitizeReturnPath(null)).toBe('/');
    expect(sanitizeReturnPath(undefined)).toBe('/');
  });
});

describe('withSignInFailed / withoutSignInFailed', () => {
  it('adds the failed flag and keeps existing query and hash', () => {
    // Arrange / Act
    const result = withSignInFailed('/courses?search=react#top');

    // Assert
    expect(result).toBe('/courses?search=react&signIn=failed#top');
  });

  it('removes only the failed flag', () => {
    // Arrange / Act
    const result = withoutSignInFailed('/courses?search=react&signIn=failed');

    // Assert
    expect(result).toBe('/courses?search=react');
  });

  it('returns bare path when failed flag was the only parameter', () => {
    // Arrange / Act
    const result = withoutSignInFailed('/cart?signIn=failed');

    // Assert
    expect(result).toBe('/cart');
  });
});

describe('withWelcome / withoutWelcome', () => {
  it('adds the welcome flag and keeps the existing query', () => {
    // Arrange / Act
    const result = withWelcome('/courses?search=react');

    // Assert
    expect(result).toBe('/courses?search=react&welcome=1');
  });

  it('removes only the welcome flag', () => {
    // Arrange / Act
    const result = withoutWelcome('/courses?search=react&welcome=1');

    // Assert
    expect(result).toBe('/courses?search=react');
  });
});
