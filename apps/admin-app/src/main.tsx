import { UserManager } from 'oidc-client-ts';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './app';
import { resolveApiUrl } from './lib/auth/api-url';
import { createUserManagerSettings } from './lib/auth/auth-settings';
import { configureApiClient } from './lib/api-client';
import './styles.css';

const rootElement = document.getElementById('root');
if (rootElement === null) {
  throw new Error('Không tìm thấy phần tử #root');
}

const apiUrl = resolveApiUrl(import.meta.env.VITE_API_URL, window.location.origin);
const userManager = new UserManager(createUserManagerSettings(apiUrl, window.location.origin));
configureApiClient(apiUrl, async () => (await userManager.getUser())?.access_token ?? null);

createRoot(rootElement).render(
  <StrictMode>
    <App userManager={userManager} />
  </StrictMode>,
);
