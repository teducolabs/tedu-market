import { NextResponse, type NextRequest } from 'next/server';
import { buildEndSessionUrl } from '../../lib/oidc/code-flow';
import { getOidcOptions } from '../../lib/oidc/oidc-options';
import { readStoredSession } from '../../lib/session/session-cookie';
import { clearSessionCookies } from '../../lib/session/write-session';

export const dynamic = 'force-dynamic';

/** Đăng xuất: xóa cookie phiên rồi gọi `/connect/logout` để API thu hồi token (FR-04). */
export function POST(request: NextRequest) {
  const options = getOidcOptions();
  // Phiên hết hạn vẫn cần id_token để API thu hồi token; chỉ cần đọc được, không cần còn hạn.
  const session = readStoredSession(
    (name) => request.cookies.get(name)?.value,
    () => options.sessionSecret,
  );
  const destination =
    session === null ? new URL('/', options.siteUrl) : buildEndSessionUrl(options, session.idToken);

  const response = NextResponse.redirect(destination, 303);
  clearSessionCookies(response, options);
  return response;
}
