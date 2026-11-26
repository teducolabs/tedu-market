import type { OidcOptions } from '../oidc/oidc-options';

/** httpOnly, SameSite=Lax (architecture mục 4); Secure khi site chạy HTTPS. */
export function buildCookieAttributes(options: OidcOptions, maxAgeSeconds: number, path = '/') {
  return {
    httpOnly: true,
    sameSite: 'lax' as const,
    secure: options.siteUrl.startsWith('https://'),
    path,
    maxAge: maxAgeSeconds,
  };
}
