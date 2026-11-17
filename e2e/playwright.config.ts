import { defineConfig } from '@playwright/test';

const STUDENT_WEB_PORT = 3100;
const ADMIN_APP_PORT = 3200;

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
