import {
  Banner,
  Button,
  DelayedSkeleton,
  EmptyState,
  FormField,
  Input,
  Skeleton,
  showErrorToast,
  showSuccessToast,
} from '@tedu-market/ui';
import { useEffect, useRef, useState, type SyntheticEvent } from 'react';
import { useNavigate, useOutletContext, useParams } from 'react-router';
import {
  getInstructorProfile,
  updateInstructorProfile,
  type InstructorProfileFieldErrors,
  type InstructorProfileItem,
} from '../lib/instructors/instructor-api';
import type { AdminAppRole } from '../lib/sidebar-items';

const LOAD_ERROR_MESSAGE = 'Có sự cố khi tải Hồ sơ Instructor. Bạn thử lại sau ít phút.';
const REQUIRED_DISPLAY_NAME_MESSAGE = 'Bạn cần nhập Tên hiển thị';

type LoadState =
  { kind: 'Loading' } | { kind: 'Error' } | { kind: 'Success'; profile: InstructorProfileItem };

function readText(form: FormData, name: string): string {
  const value = form.get(name);
  return typeof value === 'string' ? value : '';
}

/** Admin sửa Hồ sơ Instructor (FR-18): `/instructors/:instructorProfileId`, lỗi 400 gắn đúng ô. */
export function InstructorProfileEditPage() {
  const navigate = useNavigate();
  const { role } = useOutletContext<{ role: AdminAppRole }>();
  const { instructorProfileId = '' } = useParams();
  const [state, setState] = useState<LoadState>({ kind: 'Loading' });
  const [reloadCount, setReloadCount] = useState(0);
  const [errors, setErrors] = useState<InstructorProfileFieldErrors>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const formRef = useRef<HTMLFormElement>(null);

  useEffect(() => {
    if (role !== 'Admin') {
      return;
    }
    let isCancelled = false;
    setState({ kind: 'Loading' });
    void getInstructorProfile(instructorProfileId).then((result) => {
      if (!isCancelled) {
        setState(result.isSuccess ? { kind: 'Success', profile: result.value } : { kind: 'Error' });
      }
    });
    return () => {
      isCancelled = true;
    };
  }, [role, instructorProfileId, reloadCount]);

  if (role !== 'Admin') {
    return (
      <EmptyState
        kind="NoPermission"
        message="Bạn không có quyền vào mục này."
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
    const displayName = readText(form, 'displayName');
    if (displayName.trim() === '') {
      setErrors({ displayName: REQUIRED_DISPLAY_NAME_MESSAGE });
      queueMicrotask(focusFirstInvalid);
      return;
    }
    setErrors({});
    setIsSubmitting(true);
    const result = await updateInstructorProfile(instructorProfileId, {
      displayName,
      biography: readText(form, 'biography'),
      avatarUrl: readText(form, 'avatarUrl'),
    });
    setIsSubmitting(false);
    if (result.isSuccess) {
      showSuccessToast('Đã lưu');
      void navigate('/instructors');
      return;
    }
    setErrors(result.fieldErrors);
    if (result.message !== null) {
      showErrorToast(result.message);
    }
    queueMicrotask(focusFirstInvalid);
  };

  const renderBody = () => {
    if (state.kind === 'Loading') {
      return (
        <DelayedSkeleton>
          <div className="flex max-w-md flex-col gap-4">
            <Skeleton className="h-10" />
            <Skeleton className="h-24" />
            <Skeleton className="h-10" />
          </div>
        </DelayedSkeleton>
      );
    }
    if (state.kind === 'Error') {
      return (
        <div className="flex max-w-md flex-col gap-3">
          <Banner tone="error">{LOAD_ERROR_MESSAGE}</Banner>
          <div>
            <Button
              tone="secondary"
              onClick={() => {
                setReloadCount((count) => count + 1);
              }}
            >
              Thử lại
            </Button>
          </div>
        </div>
      );
    }
    const { profile } = state;
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
          <Button
            tone="secondary"
            disabled={isSubmitting}
            onClick={() => void navigate('/instructors')}
          >
            Quay lại
          </Button>
        </div>
      </form>
    );
  };

  return (
    <div className="flex flex-col gap-6">
      <h1 className="text-2xl font-semibold text-text">Sửa Hồ sơ Instructor</h1>
      {renderBody()}
    </div>
  );
}
