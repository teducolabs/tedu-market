const DEV_API_URL = 'http://localhost:5000';
const ADMIN_HOST_PATTERN = /^admin(?=[.-])/;

/**
 * Địa chỉ API (Authorization Server chạy chung). Ưu tiên biến build `VITE_API_URL` (e2e, local tùy chỉnh);
 * không có thì suy ra từ origin đang chạy: `localhost` → `http://localhost:5000`, còn lại đổi nhãn
 * đầu `admin`/`admin-stg` thành `api`/`api-stg` theo quy ước subdomain ở deploy.md mục 1. Một image dùng
 * cho cả staging và production nên không nhúng URL lúc build.
 */
export function resolveApiUrl(configuredUrl: string | undefined, origin: string): string {
  const configured = configuredUrl?.trim();
  if (configured !== undefined && configured !== '') {
    return configured.replace(/\/+$/, '');
  }

  const { protocol, hostname, port } = new URL(origin);
  if (hostname === 'localhost' || !ADMIN_HOST_PATTERN.test(hostname)) {
    return DEV_API_URL;
  }
  const apiHost = hostname.replace(ADMIN_HOST_PATTERN, 'api');
  return `${protocol}//${apiHost}${port === '' ? '' : `:${port}`}`;
}
