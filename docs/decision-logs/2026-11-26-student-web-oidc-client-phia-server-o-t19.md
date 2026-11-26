---
date: 2026-11-26
status: Proposed
decided_on:
reconstructed: false
supersedes: []
superseded_by:
related: [FR-03, NFR-06, NFR-09]
cards: [T19, T70]
---
# StudentWeb làm OIDC client phía server, tự viết trên Node crypto

## Bối cảnh
T19 cần Next.js đăng nhập Code + PKCE với API, giữ token trong cookie session httpOnly mã hóa (NFR-06, không localStorage), quay lại đúng trang trước, chặn trang cần đăng nhập. Tech Stack chưa chốt thư viện session/OIDC cho StudentWeb.

## Các lựa chọn đã xem xét
- Auth.js / `openid-client` / `iron-session`: không chọn; thêm phụ thuộc ngoài Tech Stack (conventions 6.16) cho một luồng nhỏ (1 provider, 1 client confidential), Auth.js còn kéo theo mô hình session riêng.
- Tự viết bằng `node:crypto` + `fetch`: chọn. Luồng gồm `/sign-in` (PKCE S256, `state`, `identity_provider=google` theo [log T16](2026-11-26-dang-nhap-google-hoc-vien-o-t16.md)), `/sign-in/callback` (đối chiếu `state`, đổi code ở back-channel với `student-web` + client secret), `/sign-out` (xóa cookie, gọi `/connect/logout` kèm `id_token_hint` để thu hồi token).
- Phiên: AES-256-GCM, khóa dẫn từ `StudentWeb__SessionSecret` (≥ 32 ký tự); cookie `httpOnly`, `SameSite=Lax`, `Secure` khi HTTPS; chia thành `tm_session.0..7` vì token OpenIddict (JWE) lớn hơn 4 KB khi gộp.
- Cookie tạm `tm_sign_in` (10 phút, path `/sign-in`) giữ `state`, `codeVerifier`, trang quay lại; trang quay lại chỉ nhận đường dẫn nội bộ (chống open redirect).
- Chặn trang cần đăng nhập bằng `proxy.ts` (Next 16) cho `/cart`, `/my-courses`, `/orders`, `/checkout`; chỉ kiểm cookie phiên còn hạn.
- Gia hạn phiên (T70): xin `offline_access`, lưu refresh token trong cookie phiên; `proxy.ts` chạy trước mọi trang (trừ file tĩnh và `/sign-in*`, `/sign-out`), access token hết hạn (hoặc còn dưới 30 giây) thì đổi refresh token lấy token mới, ghi lại cookie và chuyển tiếp cho chính request đó. Refresh bị từ chối thì xóa cookie: trang cần đăng nhập chuyển sang Đăng nhập bằng Google, trang công khai hiện như Khách. Khách không có cookie thì proxy không đọc cấu hình, không gọi API. Cookie phiên sống 14 ngày (mặc định refresh token OpenIddict); hạn thật do API quyết định.
- Lần đăng nhập đầu (T70): API thêm claim `first_sign_in=true` vào id_token (chỉ id_token, không vào access token) ở lần đổi code ngay sau khi Học viên được tạo bằng Google; claim đi qua cookie đăng nhập của API và bị bỏ sau lần authorize đầu, nên lần authorize sau, đăng nhập lại, hay đổi refresh token đều không có claim. StudentWeb thêm `welcome=1` vào URL quay lại, toast hiện một lần rồi bỏ tham số khỏi URL.
- `redirect_uri` `/sign-in/callback` là trang hiện toàn trang "Đang đăng nhập…", rồi chuyển tiếp `code`/`state` (một lần duy nhất) sang route `/sign-in/complete` làm việc đổi token và ghi cookie.
- Tên và email lấy từ id_token nhận trực tiếp từ token endpoint qua kết nối tin cậy, không kiểm chữ ký lại (OIDC Core 3.1.3.7).
- Cấu hình đọc lúc chạy: `API_URL` (URL công khai của API, dùng cả cho redirect trình duyệt và back-channel), `SITE_URL`, `StudentWeb__OidcClientSecret`, `StudentWeb__SessionSecret`.

## Quyết định
Như mục "chọn" ở trên.

## Lý do
NFR-06 (Code + PKCE, session phía server, không localStorage), FR-03 (quay lại đúng trang), conventions 6.16 (không thêm thư viện ngoài Tech Stack).

## Đánh đổi / hệ quả
- Tự giữ mã bảo mật: có test cho mã hóa, giả mạo cookie, `state` sai, open redirect; nên cho người có kinh nghiệm bảo mật review.
- Refresh token xoay vòng: hai request song song cùng lúc hết hạn có thể cùng đổi một refresh token; OpenIddict cho phép dùng lại trong khoảng ngắn (mặc định 30 giây), quá thì request thua bị coi như Khách và đăng nhập lại.
- `API_URL` phải truy cập được từ cả trình duyệt và container `web`; nếu sau này tách địa chỉ nội bộ thì thêm biến riêng.
- Proxy chỉ kiểm cookie, không hỏi API; token bị thu hồi từ phía khác vẫn bị API từ chối ở lần gọi sau.

## Bằng chứng
[docs/ux.md](../ux.md) luồng "Đăng nhập bằng Google"; test `sign-in-flow.test.ts`, `session-cookie.test.ts`, `proxy.test.ts`, e2e `student-web-sign-in.spec.ts`; backend `GoogleLoginTests` (claim `first_sign_in`).
