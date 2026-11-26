import { sanitizeReturnPath } from './return-path';

export interface SignInState {
  state: string;
  codeVerifier: string;
  returnPath: string;
}

export const SIGN_IN_STATE_MAX_AGE_SECONDS = 600;

/** Kiểm tra dữ liệu giải mã từ cookie tạm trước khi tin; trả `null` nếu thiếu trường. */
export function toSignInState(unsealed: unknown): SignInState | null {
  const value = unsealed as Partial<SignInState> | null;
  if (
    value === null ||
    typeof value.state !== 'string' ||
    typeof value.codeVerifier !== 'string' ||
    value.state === '' ||
    value.codeVerifier === ''
  ) {
    return null;
  }
  return {
    state: value.state,
    codeVerifier: value.codeVerifier,
    returnPath: sanitizeReturnPath(value.returnPath),
  };
}
