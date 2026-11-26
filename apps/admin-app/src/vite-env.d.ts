/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Địa chỉ API tùy chỉnh lúc build; bỏ trống thì suy ra từ origin (xem `resolveApiUrl`). */
  readonly VITE_API_URL?: string;
}
