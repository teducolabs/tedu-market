/** Trang cần đăng nhập: Giỏ hàng, Khóa học của tôi, Lịch sử đơn, Thanh toán (ux.md mục 2). Thẻ T44, T53, T54 chốt route thật; đổi ở đây thì đổi cả `matcher` trong `proxy.ts`. */
export const PROTECTED_PATH_PREFIXES = ['/cart', '/my-courses', '/orders', '/checkout'] as const;
