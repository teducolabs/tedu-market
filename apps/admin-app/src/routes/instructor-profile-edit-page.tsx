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
import { useNavigate, useOutletContext, useParams } from 'react-router';
import {
  getInstructorProfile,
  updateInstructorProfile,
  type InstructorProfileFieldErrors,
  type InstructorProfileInput,
  type InstructorProfileItem,
} from '../lib/instructors/instructor-api';
import { InstructorProfileForm } from '../components/instructor-profile-form';
import type { AdminAppRole } from '../lib/sidebar-items';

const LOAD_ERROR_MESSAGE = 'Có sự cố khi tải Hồ sơ Instructor. Bạn thử lại sau ít phút.';

type LoadState =
  { kind: 'Loading' } | { kind: 'Error' } | { kind: 'Success'; profile: InstructorProfileItem };

/** Admin sửa Hồ sơ Instructor (FR-18): `/instructors/:instructorProfileId`, lỗi 400 gắn đúng ô. */
export function InstructorProfileEditPage() {
  const navigate = useNavigate();
  const { role } = useOutletContext<{ role: AdminAppRole }>();
  const { instructorProfileId = '' } = useParams();
  const [state, setState] = useState<LoadState>({ kind: 'Loading' });
  const [reloadCount, setReloadCount] = useState(0);
  const [errors, setErrors] = useState<InstructorProfileFieldErrors>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

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

  const handleSubmit = async (input: InstructorProfileInput) => {
    setErrors({});
    setIsSubmitting(true);
    const result = await updateInstructorProfile(instructorProfileId, input);
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
        secondaryAction={
          <Button
            tone="secondary"
            disabled={isSubmitting}
            onClick={() => void navigate('/instructors')}
          >
            Quay lại
          </Button>
        }
      />
    );
  };

  return (
    <div className="flex flex-col gap-6">
      <h1 className="text-2xl font-semibold text-text">Sửa Hồ sơ Instructor</h1>
      {renderBody()}
    </div>
  );
}
