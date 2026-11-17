import { defineConfig } from '@playwright/test';

const STUDENT_WEB_PORT = 3100;

export default defineConfig({
  testDir: '.',
  testMatch: '**/*.spec.ts',
  forbidOnly: true,
  use: { baseURL: `http://localhost:${String(STUDENT_WEB_PORT)}` },
  webServer: {
    command: `pnpm --filter @tedu-market/student-web build && pnpm --filter @tedu-market/student-web exec next start -p ${String(STUDENT_WEB_PORT)}`,
    url: `http://localhost:${String(STUDENT_WEB_PORT)}`,
    reuseExistingServer: true,
    timeout: 180_000,
  },
});
