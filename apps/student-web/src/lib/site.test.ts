import { describe, expect, it } from 'vitest';
import { buildSearchUrl, getSiteUrl } from './site';

describe('getSiteUrl', () => {
  it('returns configured origin when value is a valid url', () => {
    // Arrange
    const rawUrl = 'https://market.tedu.vn';

    // Act
    const siteUrl = getSiteUrl(rawUrl);

    // Assert
    expect(siteUrl.origin).toBe('https://market.tedu.vn');
  });

  it('falls back to localhost when value is missing', () => {
    // Arrange / Act
    const siteUrl = getSiteUrl(undefined);

    // Assert
    expect(siteUrl.origin).toBe('http://localhost:3000');
  });

  it('falls back to localhost when value is not a url', () => {
    // Arrange / Act
    const siteUrl = getSiteUrl('khong phai url');

    // Assert
    expect(siteUrl.origin).toBe('http://localhost:3000');
  });
});

describe('buildSearchUrl', () => {
  it('encodes keyword with diacritics and spaces', () => {
    // Arrange / Act
    const url = buildSearchUrl('  lập trình  ');

    // Assert
    expect(url).toBe('/courses?search=l%E1%BA%ADp%20tr%C3%ACnh');
  });

  it('returns course list path when keyword is blank', () => {
    // Arrange / Act
    const url = buildSearchUrl('   ');

    // Assert
    expect(url).toBe('/courses');
  });
});
