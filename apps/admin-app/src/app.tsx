import { BrowserRouter, Navigate, Route, Routes } from 'react-router';
import { AdminLayout } from './components/admin-layout';
import { ErrorBoundary } from './components/error-boundary';
import { resolveFakeRole } from './lib/fake-session';
import { SIDEBAR_ITEMS } from './lib/sidebar-items';
import { PlaceholderPage } from './routes/placeholder-page';

export function App() {
  const role = resolveFakeRole(window.location.search, import.meta.env.MODE);

  return (
    <ErrorBoundary>
      <BrowserRouter>
        <Routes>
          {/* Đăng xuất chưa nối OIDC: TODO(T20). */}
          <Route element={<AdminLayout role={role} onSignOut={() => undefined} />}>
            <Route index element={<Navigate to="/revenue" replace />} />
            {SIDEBAR_ITEMS.map((item) => (
              <Route key={item.path} path={item.path} element={<PlaceholderPage />} />
            ))}
          </Route>
        </Routes>
      </BrowserRouter>
    </ErrorBoundary>
  );
}
