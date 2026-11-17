export const SITE_NAME = 'TEDU Market';
export const SITE_DESCRIPTION =
  'Học online cùng các Instructor của TEDU: khóa học lập trình và công nghệ.';
export const SITE_LOCALE = 'vi_VN';

/** URL gốc để sinh canonical, Open Graph, robots. Đọc biến SITE_URL lúc chạy (không bí mật, không dùng NEXT_PUBLIC_ vì bị nhúng lúc build). */
export function getSiteUrl(rawUrl: string | undefined = process.env.SITE_URL): URL {
  const fallbackUrl = 'http://localhost:3000';
  try {
    return new URL(rawUrl !== undefined && rawUrl !== '' ? rawUrl : fallbackUrl);
  } catch {
    return new URL(fallbackUrl);
  }
}

/** Đường dẫn trang tìm kiếm của Danh sách khóa học (FR-44). */
export function buildSearchUrl(keyword: string): string {
  const trimmedKeyword = keyword.trim();
  return trimmedKeyword === ''
    ? '/courses'
    : `/courses?search=${encodeURIComponent(trimmedKeyword)}`;
}
