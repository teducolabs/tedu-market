import {
  Button,
  EmptyState,
  FormField,
  Input,
  showErrorToast,
  showSuccessToast,
} from '@tedu-market/ui';
import { useRef, useState, type SyntheticEvent } from 'react';
import { useAuth } from 'react-oidc-context';
import { useNavigate, useOutletContext } from 'react-router';
import { changePassword, type ChangePasswordFieldErrors } from '../lib/auth/change-password';
import type { AdminAppRole } from '../lib/sidebar-items';

/** Chờ toast hiện trước khi chuyển về đăng nhập. */
export const SIGN_OUT_AFTER_CHANGE_DELAY_MS = 1500;
const SUCCESS_MESSAGE = 'Đã đổi mật khẩu. Bạn đăng nhập lại nhé';
const REQUIRED_CURRENT_MESSAGE = 'Bạn cần nhập mật khẩu hiện tại';
const REQUIRED_NEW_MESSAGE = 'Bạn cần nhập mật khẩu mới';

function readText(form: FormData, name: string): string {
  const value = form.get(name);
  return typeof value === 'string' ? value : '';
}

/**
 * Đổi mật khẩu của Admin (FR-42). Đổi xong mọi token cũ đã bị thu hồi; App quản trị đăng xuất (xóa cả
 * phiên ở trang đăng nhập) để Admin đăng nhập lại bằng mật khẩu mới.
 */
export function ChangePasswordPage() {
  const auth = useAuth();
  const navigate = useNavigate();
  const { role } = useOutletContext<{ role: AdminAppRole }>();
  const [errors, setErrors] = useState<ChangePasswordFieldErrors>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const formRef = useRef<HTMLFormElement>(null);

  if (role !== 'Admin') {
    return (
      <EmptyState
        kind="NoPermission"
        message="Chỉ Admin được đổi mật khẩu."
        actionLabel="Quay lại"
        onAction={() => void navigate(-1)}
      />
    );
  }

  const focusFirstInvalid = () => {
    formRef.current?.querySelector<HTMLElement>('[aria-invalid="true"]')?.focus();
  };

  const handleSubmit = async (event: SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const currentPassword = readText(form, 'currentPassword');
    const newPassword = readText(form, 'newPassword');

    const clientErrors: ChangePasswordFieldErrors = {};
    if (currentPassword === '') {
      clientErrors.currentPassword = REQUIRED_CURRENT_MESSAGE;
    }
    if (newPassword === '') {
      clientErrors.newPassword = REQUIRED_NEW_MESSAGE;
    }
    if (Object.keys(clientErrors).length > 0) {
      setErrors(clientErrors);
      queueMicrotask(focusFirstInvalid);
      return;
    }

    setErrors({});
    setIsSubmitting(true);
    const result = await changePassword(currentPassword, newPassword);
    setIsSubmitting(false);

    if (result.isSuccess) {
      showSuccessToast(SUCCESS_MESSAGE);
      setTimeout(() => {
        auth.signoutRedirect().catch(() => {
          void navigate('/sign-in', { replace: true, state: { hasChangedPassword: true } });
        });
      }, SIGN_OUT_AFTER_CHANGE_DELAY_MS);
      return;
    }
    setErrors(result.fieldErrors);
    if (result.message !== null) {
      showErrorToast(result.message);
    }
    queueMicrotask(focusFirstInvalid);
  };

  return (
    <div className="flex max-w-md flex-col gap-6">
      <h1 className="text-2xl font-semibold text-text">Đổi mật khẩu</h1>
      <form
        ref={formRef}
        noValidate
        onSubmit={(event) => {
          void handleSubmit(event);
        }}
        className="flex flex-col gap-4"
      >
        <FormField label="Mật khẩu hiện tại" isRequired error={errors.currentPassword}>
          {(control) => (
            <Input
              {...control}
              name="currentPassword"
              type="password"
              autoComplete="current-password"
            />
          )}
        </FormField>
        <FormField label="Mật khẩu mới" isRequired error={errors.newPassword}>
          {(control) => (
            <Input {...control} name="newPassword" type="password" autoComplete="new-password" />
          )}
        </FormField>
        <div>
          <Button type="submit" isLoading={isSubmitting}>
            Lưu
          </Button>
        </div>
      </form>
    </div>
  );
}
