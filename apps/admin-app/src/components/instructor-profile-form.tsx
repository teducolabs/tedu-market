import { Button, FormField, Input } from '@tedu-market/ui';
import { useRef, type ReactNode, type SyntheticEvent } from 'react';
import type {
  InstructorProfileFieldErrors,
  InstructorProfileInput,
  InstructorProfileItem,
} from '../lib/instructors/instructor-api';

export const REQUIRED_DISPLAY_NAME_MESSAGE = 'Bạn cần nhập Tên hiển thị';

export interface InstructorProfileFormProps {
  profile: InstructorProfileItem;
  errors: InstructorProfileFieldErrors;
  isSubmitting: boolean;
  /** Trang lưu hồ sơ và đặt `errors`; form chờ xong rồi focus ô lỗi đầu tiên. */
  onSubmit: (input: InstructorProfileInput) => Promise<void>;
  /** Báo lỗi kiểm tra phía trình duyệt (Tên hiển thị trống) để trang hiện dưới ô nhập. */
  onInvalid: (errors: InstructorProfileFieldErrors) => void;
  /** Nút phụ cạnh nút Lưu (ví dụ Quay lại). */
  secondaryAction?: ReactNode;
}

function readText(form: FormData, name: string): string {
  const value = form.get(name);
  return typeof value === 'string' ? value : '';
}

/** Form Tên hiển thị, Giới thiệu, Ảnh đại diện dùng chung cho Admin (FR-18) và Instructor (FR-43). */
export function InstructorProfileForm({
  profile,
  errors,
  isSubmitting,
  onSubmit,
  onInvalid,
  secondaryAction,
}: InstructorProfileFormProps) {
  const formRef = useRef<HTMLFormElement>(null);

  const focusFirstInvalid = () => {
    formRef.current?.querySelector<HTMLElement>('[aria-invalid="true"]')?.focus();
  };

  const handleSubmit = async (event: SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const displayName = readText(form, 'displayName');
    if (displayName.trim() === '') {
      onInvalid({ displayName: REQUIRED_DISPLAY_NAME_MESSAGE });
      queueMicrotask(focusFirstInvalid);
      return;
    }
    await onSubmit({
      displayName,
      biography: readText(form, 'biography'),
      avatarUrl: readText(form, 'avatarUrl'),
    });
    queueMicrotask(focusFirstInvalid);
  };

  return (
    <form
      ref={formRef}
      noValidate
      onSubmit={(event) => {
        void handleSubmit(event);
      }}
      className="flex max-w-md flex-col gap-4"
    >
      <FormField label="Tên hiển thị" isRequired error={errors.displayName}>
        {(control) => (
          <Input
            {...control}
            name="displayName"
            maxLength={100}
            defaultValue={profile.displayName}
          />
        )}
      </FormField>
      <FormField label="Giới thiệu" error={errors.biography}>
        {(control) => (
          <textarea
            {...control}
            name="biography"
            rows={5}
            maxLength={2000}
            defaultValue={profile.biography}
            className="w-full rounded-lg border border-border-input bg-card px-3 py-2 text-base text-text aria-[invalid=true]:border-error"
          />
        )}
      </FormField>
      <FormField
        label="Ảnh đại diện"
        error={errors.avatarUrl}
        hint="Đường dẫn http hoặc https tới ảnh."
      >
        {(control) => (
          <Input {...control} name="avatarUrl" type="url" defaultValue={profile.avatarUrl} />
        )}
      </FormField>
      <div className="flex gap-3">
        <Button type="submit" isLoading={isSubmitting}>
          Lưu
        </Button>
        {secondaryAction}
      </div>
    </form>
  );
}
