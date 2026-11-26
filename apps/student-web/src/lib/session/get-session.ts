import { cookies } from 'next/headers';
import { getOidcOptions } from '../oidc/oidc-options';
import { readSession, type Session, type SessionUser } from './session-cookie';

/** Phiên của request hiện tại trong Server Component; chỉ đọc cấu hình khi có cookie phiên. */
export async function getSession(): Promise<Session | null> {
  const cookieStore = await cookies();
  return readSession(
    (name) => cookieStore.get(name)?.value,
    () => getOidcOptions().sessionSecret,
  );
}

export async function getSessionUser(): Promise<SessionUser | null> {
  const session = await getSession();
  return session === null ? null : { name: session.name, email: session.email };
}
