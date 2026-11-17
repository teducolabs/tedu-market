export const SITE_NAME = 'TEDU Market';
export const SITE_DESCRIPTION =
  'Học online cùng các Instructor của TEDU: khóa học lập trình và công nghệ.';
export const SITE_LOCALE = 'vi_VN';

/** URL gốc để sinh canonical, Open Graph, robots. Cấu hình bằng NEXT_PUBLIC_SITE_URL (không bí mật). */
export function getSiteUrl(rawUrl: string | undefined = process.env.NEXT_PUBLIC_SITE_URL): URL {
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
