import {
  Banner,
  Button,
  DelayedSkeleton,
  EmptyState,
  Skeleton,
  showErrorToast,
  showSuccessToast,
} from '@tedu-market/ui';
import { useEffect, useState } from 'react';
import { useNavigate, useOutletContext } from 'react-router';
import { InstructorProfileForm } from '../components/instructor-profile-form';
import {
  getMyInstructorProfile,
  updateInstructorProfile,
  type InstructorProfileFieldErrors,
  type InstructorProfileInput,
  type InstructorProfileItem,
} from '../lib/instructors/instructor-api';
import type { AdminAppRole } from '../lib/sidebar-items';

const LOAD_ERROR_MESSAGE = 'Có sự cố khi tải Hồ sơ Instructor. Bạn thử lại sau ít phút.';

type LoadState =
  { kind: 'Loading' } | { kind: 'Error' } | { kind: 'Success'; profile: InstructorProfileItem };

/** Instructor sửa Hồ sơ Instructor của mình (FR-43): `/instructor-profile`, lỗi 400 gắn đúng ô. */
export function InstructorProfilePage() {
  const navigate = useNavigate();
  const { role } = useOutletContext<{ role: AdminAppRole }>();
  const [state, setState] = useState<LoadState>({ kind: 'Loading' });
  const [reloadCount, setReloadCount] = useState(0);
  const [errors, setErrors] = useState<InstructorProfileFieldErrors>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (role !== 'Instructor') {
      return;
    }
    let isCancelled = false;
    setState({ kind: 'Loading' });
    void getMyInstructorProfile().then((result) => {
      if (!isCancelled) {
        setState(result.isSuccess ? { kind: 'Success', profile: result.value } : { kind: 'Error' });
      }
    });
    return () => {
      isCancelled = true;
    };
  }, [role, reloadCount]);

  if (role !== 'Instructor') {
    return (
      <EmptyState
        kind="NoPermission"
        message="Bạn không có quyền vào mục này."
        actionLabel="Quay lại"
        onAction={() => void navigate(-1)}
      />
    );
  }

  const handleSubmit = async (input: InstructorProfileInput) => {
    if (state.kind !== 'Success') {
      return;
    }
    setErrors({});
    setIsSubmitting(true);
    const result = await updateInstructorProfile(state.profile.instructorProfileId, input);
    setIsSubmitting(false);
    if (result.isSuccess) {
      showSuccessToast('Đã lưu');
      return;
    }
    setErrors(result.fieldErrors);
    if (result.message !== null) {
      showErrorToast(result.message);
    }
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
    return (
      <InstructorProfileForm
        profile={state.profile}
        errors={errors}
        isSubmitting={isSubmitting}
        onSubmit={handleSubmit}
        onInvalid={setErrors}
      />
    );
  };

  return (
    <div className="flex flex-col gap-6">
      <h1 className="text-2xl font-semibold text-text">Hồ sơ Instructor</h1>
      {renderBody()}
    </div>
  );
}
