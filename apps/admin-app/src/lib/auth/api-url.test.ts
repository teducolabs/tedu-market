import { describe, expect, it } from 'vitest';
import { resolveApiUrl } from './api-url';

describe('resolveApiUrl', () => {
  it('trims trailing slashes from the configured url and prefers it', () => {
    // Arrange / Act
    const url = resolveApiUrl('https://api.tedu.test//', 'https://admin.tedu.test');

    // Assert
    expect(url).toBe('https://api.tedu.test');
  });

  it.each([[undefined], [''], ['  ']])('ignores a blank configured value %j', (value) => {
    // Arrange / Act
    const url = resolveApiUrl(value, 'http://localhost:5173');

    // Assert
    expect(url).toBe('http://localhost:5000');
  });

  it.each([
    ['https://admin.tedu.test', 'https://api.tedu.test'],
    ['https://admin-stg.tedu.test', 'https://api-stg.tedu.test'],
    ['http://admin.tedu.test:8443', 'http://api.tedu.test:8443'],
  ])('derives the api origin from %s', (origin, expected) => {
    // Arrange / Act
    const url = resolveApiUrl(undefined, origin);

    // Assert
    expect(url).toBe(expected);
  });

  it('falls back to the dev api when the host does not follow the admin naming', () => {
    // Arrange / Act
    const url = resolveApiUrl(undefined, 'http://192.168.1.5:5173');

    // Assert
    expect(url).toBe('http://localhost:5000');
  });
});
