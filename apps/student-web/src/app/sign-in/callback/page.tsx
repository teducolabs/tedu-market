import type { Metadata } from 'next';
import { SigningIn } from '../../../components/signing-in';

export const metadata: Metadata = {
  title: 'Đang đăng nhập',
  robots: { index: false },
};

/** `redirect_uri` của client `student-web`: hiện "Đang đăng nhập…" trong lúc `/sign-in/complete` đổi code lấy token. */
export default function SignInCallbackPage() {
  return <SigningIn />;
}
