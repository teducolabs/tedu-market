import { createCipheriv, createDecipheriv, createHash, randomBytes } from 'node:crypto';

export const SESSION_COOKIE_PREFIX = 'tm_session';
export const SIGN_IN_STATE_COOKIE = 'tm_sign_in';
/** Cookie tối đa ~4096 byte gồm cả tên và thuộc tính; chia nhỏ phần giá trị để chứa token OpenIddict (JWE). */
export const COOKIE_CHUNK_SIZE = 3500;
const MAX_CHUNKS = 8;
const IV_LENGTH = 12;
const TAG_LENGTH = 16;

/** Thời gian giữ cookie phiên khi có refresh token (mặc định refresh token của OpenIddict là 14 ngày); hết hạn thật do API quyết định. */
export const SESSION_COOKIE_MAX_AGE_SECONDS = 14 * 24 * 60 * 60;

export interface Session {
  accessToken: string;
  idToken: string;
  /** Không có thì phiên kết thúc khi access token hết hạn. */
  refreshToken: string | null;
  /** Epoch giây. */
  expiresAt: number;
  name: string;
  email: string;
}

export interface SessionUser {
  name: string;
  email: string;
}

export interface CookieWrite {
  name: string;
  value: string;
}

function deriveKey(secret: string): Buffer {
  return createHash('sha256').update(`tedu-market:student-web:${secret}`).digest();
}

/** Mã hóa AES-256-GCM, kết quả base64url của `iv | tag | ciphertext`. */
export function sealJson(value: unknown, secret: string): string {
  const iv = randomBytes(IV_LENGTH);
  const cipher = createCipheriv('aes-256-gcm', deriveKey(secret), iv);
  const encrypted = Buffer.concat([cipher.update(JSON.stringify(value), 'utf8'), cipher.final()]);
  return Buffer.concat([iv, cipher.getAuthTag(), encrypted]).toString('base64url');
}

/** Giải mã; chuỗi sai, bị sửa hoặc khác secret đều trả `null`. */
export function unsealJson(sealed: string, secret: string): unknown {
  try {
    const bytes = Buffer.from(sealed, 'base64url');
    if (bytes.length <= IV_LENGTH + TAG_LENGTH) {
      return null;
    }
    const decipher = createDecipheriv(
      'aes-256-gcm',
      deriveKey(secret),
      bytes.subarray(0, IV_LENGTH),
    );
    decipher.setAuthTag(bytes.subarray(IV_LENGTH, IV_LENGTH + TAG_LENGTH));
    const decrypted = Buffer.concat([
      decipher.update(bytes.subarray(IV_LENGTH + TAG_LENGTH)),
      decipher.final(),
    ]);
    return JSON.parse(decrypted.toString('utf8')) as unknown;
  } catch {
    return null;
  }
}

function chunkName(index: number): string {
  return `${SESSION_COOKIE_PREFIX}.${String(index)}`;
}

/** Phiên đã mã hóa, chia thành các cookie `tm_session.0`, `tm_session.1`... */
export function buildSessionCookies(session: Session, secret: string): CookieWrite[] {
  const sealed = sealJson(session, secret);
  const chunks: CookieWrite[] = [];
  for (let start = 0; start < sealed.length; start += COOKIE_CHUNK_SIZE) {
    chunks.push({
      name: chunkName(chunks.length),
      value: sealed.slice(start, start + COOKIE_CHUNK_SIZE),
    });
  }
  if (chunks.length > MAX_CHUNKS) {
    throw new Error('Phiên đăng nhập quá lớn để lưu trong cookie');
  }
  return chunks;
}

/** Tên mọi cookie phiên có thể đang tồn tại, dùng để xóa. */
export function allSessionCookieNames(): string[] {
  return Array.from({ length: MAX_CHUNKS }, (_, index) => chunkName(index));
}

/** Đọc phiên đã lưu kể cả access token đã hết hạn (để gia hạn); thiếu hoặc hỏng thì `null`. */
export function readStoredSession(
  getCookie: (name: string) => string | undefined,
  secret: () => string,
): Session | null {
  const parts: string[] = [];
  for (let index = 0; index < MAX_CHUNKS; index += 1) {
    const part = getCookie(chunkName(index));
    if (part === undefined) {
      break;
    }
    parts.push(part);
  }
  if (parts.length === 0) {
    return null;
  }
  const session = unsealJson(parts.join(''), secret()) as Partial<Session> | null;
  if (
    session?.accessToken === undefined ||
    session.idToken === undefined ||
    typeof session.accessToken !== 'string' ||
    typeof session.idToken !== 'string' ||
    typeof session.expiresAt !== 'number'
  ) {
    return null;
  }
  return {
    accessToken: session.accessToken,
    idToken: session.idToken,
    refreshToken: typeof session.refreshToken === 'string' ? session.refreshToken : null,
    expiresAt: session.expiresAt,
    name: session.name ?? '',
    email: session.email ?? '',
  };
}

/** Đọc phiên còn hạn; thiếu, hỏng, sai secret hoặc hết hạn đều trả `null`. */
export function readSession(
  getCookie: (name: string) => string | undefined,
  secret: () => string,
  nowSeconds: number = Math.floor(Date.now() / 1000),
): Session | null {
  const session = readStoredSession(getCookie, secret);
  return session !== null && session.expiresAt > nowSeconds ? session : null;
}
