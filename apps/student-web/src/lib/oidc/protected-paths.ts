/** Trang cần đăng nhập: Giỏ hàng, Khóa học của tôi, Lịch sử đơn, Thanh toán (ux.md mục 2). Thẻ T49, T60, T61 chốt route thật; đổi ở đây thì đổi cả `matcher` trong `proxy.ts`. */
export const PROTECTED_PATH_PREFIXES = ['/cart', '/my-courses', '/orders', '/checkout'] as const;
