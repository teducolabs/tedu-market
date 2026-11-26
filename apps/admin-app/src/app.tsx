import { Toaster } from '@tedu-market/ui';
import type { UserManager } from 'oidc-client-ts';
import { AuthProvider } from 'react-oidc-context';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router';
import { AuthenticatedLayout } from './components/authenticated-layout';
import { ErrorBoundary } from './components/error-boundary';
import { AUTH_CALLBACK_PATH, SIGN_IN_PATH } from './lib/auth/auth-settings';
import { SIDEBAR_ITEMS } from './lib/sidebar-items';
import { AuthCallbackPage } from './routes/auth-callback-page';
import { ChangePasswordPage } from './routes/change-password-page';
import { InstructorProfileEditPage } from './routes/instructor-profile-edit-page';
import { InstructorsPage } from './routes/instructors-page';
import { PlaceholderPage } from './routes/placeholder-page';
import { SignInPage } from './routes/sign-in-page';

export interface AppProps {
  userManager: UserManager;
}

/** Điều hướng sau đăng nhập do `AuthCallbackPage` xử lý nên không cần làm gì ở đây. */
function keepRouterInControl(): void {
  // Cố ý để trống: thư viện vẫn giữ nguyên URL callback cho tới khi AuthCallbackPage điều hướng.
}

export function App({ userManager }: AppProps) {
  return (
    <ErrorBoundary>
      <AuthProvider userManager={userManager} onSigninCallback={keepRouterInControl}>
        <BrowserRouter>
          <Routes>
            <Route path={SIGN_IN_PATH} element={<SignInPage />} />
            <Route path={AUTH_CALLBACK_PATH} element={<AuthCallbackPage />} />
            <Route element={<AuthenticatedLayout />}>
              <Route index element={<Navigate to="/revenue" replace />} />
              <Route path="/change-password" element={<ChangePasswordPage />} />
              <Route path="/instructors" element={<InstructorsPage />} />
              <Route
                path="/instructors/:instructorProfileId"
                element={<InstructorProfileEditPage />}
              />
              {SIDEBAR_ITEMS.filter((item) => item.path !== '/instructors').map((item) => (
                <Route key={item.path} path={item.path} element={<PlaceholderPage />} />
              ))}
            </Route>
          </Routes>
        </BrowserRouter>
        <Toaster />
      </AuthProvider>
    </ErrorBoundary>
  );
}
