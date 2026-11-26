import { defineConfig } from '@playwright/test';

const STUDENT_WEB_PORT = 3100;
const ADMIN_APP_PORT = 3200;
/** API giả: test chặn mọi request tới đây, không cần chạy API thật. */
export const FAKE_API_URL = 'http://127.0.0.1:5999';

export default defineConfig({
  testDir: '.',
  testMatch: '**/*.spec.ts',
  forbidOnly: true,
  projects: [
    {
      name: 'student-web',
      testMatch: 'student-web-*.spec.ts',
      use: { baseURL: `http://localhost:${String(STUDENT_WEB_PORT)}` },
    },
    {
      name: 'admin-app',
      testMatch: 'admin-app-*.spec.ts',
      use: { baseURL: `http://localhost:${String(ADMIN_APP_PORT)}` },
    },
  ],
  webServer: [
    {
      command: `pnpm --filter @tedu-market/student-web build && pnpm --filter @tedu-market/student-web exec next start -p ${String(STUDENT_WEB_PORT)}`,
      url: `http://localhost:${String(STUDENT_WEB_PORT)}`,
      env: {
        API_URL: FAKE_API_URL,
        SITE_URL: `http://localhost:${String(STUDENT_WEB_PORT)}`,
        // Giá trị giả chỉ dùng cho e2e.
        StudentWeb__OidcClientSecret: 'e2e-client-secret',
        StudentWeb__SessionSecret: 'e2e-session-secret-e2e-session-secret',
      },
      reuseExistingServer: true,
      timeout: 180_000,
    },
    {
      // Mode e2e (không phải production) cho phép `?role=Instructor` đổi vai trò giả.
      command: `pnpm --filter @tedu-market/admin-app exec vite build --mode e2e && pnpm --filter @tedu-market/admin-app exec vite preview --port ${String(ADMIN_APP_PORT)} --strictPort`,
      url: `http://localhost:${String(ADMIN_APP_PORT)}`,
      reuseExistingServer: true,
      timeout: 180_000,
    },
  ],
});
