import { vi } from 'vitest';

export interface FakeAuthOptions {
  isLoading?: boolean;
  isAuthenticated?: boolean;
  roles?: string[];
  name?: string;
  error?: Error;
  state?: unknown;
}

/** Giá trị `useAuth()` giả cho test component; chỉ có các trường App quản trị dùng. */
export function createFakeAuth(options: FakeAuthOptions = {}) {
  const isAuthenticated = options.isAuthenticated ?? true;
  return {
    isLoading: options.isLoading ?? false,
    isAuthenticated,
    error: options.error,
    user: isAuthenticated
      ? {
          profile: { name: options.name ?? 'admin@tedu.test', role: options.roles ?? ['Admin'] },
          state: options.state,
          access_token: 'access-token',
        }
      : null,
    signinRedirect: vi.fn().mockResolvedValue(undefined),
    signoutRedirect: vi.fn().mockResolvedValue(undefined),
  };
}
