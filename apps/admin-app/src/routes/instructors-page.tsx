import {
  Badge,
  Banner,
  Button,
  ConfirmDialog,
  DelayedSkeleton,
  EmptyState,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  showErrorToast,
  showSuccessToast,
  type BadgeTone,
} from '@tedu-market/ui';
import { useCallback, useEffect, useState } from 'react';
import { useNavigate, useOutletContext } from 'react-router';
import {
  USERS_PAGE_SIZE,
  approveInstructor,
  deleteInstructorProfile,
  listUsers,
  type UserItem,
  type UserPage,
  type UserRoleFilter,
} from '../lib/instructors/instructor-api';
import type { AdminAppRole } from '../lib/sidebar-items';

const LOAD_ERROR_MESSAGE = 'Có sự cố khi tải danh sách. Bạn thử lại sau ít phút.';
const APPROVE_ERROR_MESSAGE = 'Không thể duyệt Instructor. Bạn thử lại sau ít phút.';
const DELETE_ERROR_MESSAGE = 'Không thể xóa Hồ sơ Instructor. Bạn thử lại sau ít phút.';

const ROLE_FILTERS: readonly { value: UserRoleFilter | ''; label: string }[] = [
  { value: '', label: 'Tất cả' },
  { value: 'Student', label: 'Học viên' },
  { value: 'Instructor', label: 'Instructor' },
  { value: 'Admin', label: 'Admin' },
];

type ListState = { kind: 'Loading' } | { kind: 'Error' } | { kind: 'Success'; page: UserPage };

type PendingAction = { kind: 'Approve' | 'Delete'; user: UserItem } | null;

function describeRole(user: UserItem): { label: string; tone: BadgeTone } {
  if (user.roles.includes('Admin')) {
    return { label: 'Admin', tone: 'primary' };
  }
  if (user.roles.includes('Instructor')) {
    return { label: 'Instructor', tone: 'success' };
  }
  return { label: 'Học viên', tone: 'secondary' };
}

function canApprove(user: UserItem): boolean {
  return (
    user.roles.includes('Student') &&
    !user.roles.includes('Instructor') &&
    !user.roles.includes('Admin') &&
    user.instructorProfileId === null
  );
}

/** Màn hình Instructor của Admin (FR-40, FR-18): danh sách người dùng, Duyệt Instructor, sửa/xóa Hồ sơ Instructor. */
export function InstructorsPage() {
  const navigate = useNavigate();
  const { role } = useOutletContext<{ role: AdminAppRole }>();
  const [roleFilter, setRoleFilter] = useState<UserRoleFilter | ''>('');
  const [pageNumber, setPageNumber] = useState(1);
  const [reloadCount, setReloadCount] = useState(0);
  const [state, setState] = useState<ListState>({ kind: 'Loading' });
  const [pending, setPending] = useState<PendingAction>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (role !== 'Admin') {
      return;
    }
    let isCancelled = false;
    setState({ kind: 'Loading' });
    void listUsers(roleFilter === '' ? null : roleFilter, pageNumber).then((result) => {
      if (!isCancelled) {
        setState(result.isSuccess ? { kind: 'Success', page: result.value } : { kind: 'Error' });
      }
    });
    return () => {
      isCancelled = true;
    };
  }, [role, roleFilter, pageNumber, reloadCount]);

  const reload = useCallback(() => {
    setReloadCount((count) => count + 1);
  }, []);

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

  const confirm = async () => {
    if (pending === null) {
      return;
    }
    setIsSubmitting(true);
    const isApprove = pending.kind === 'Approve';
    const result = isApprove
      ? await approveInstructor(pending.user.userId)
      : pending.user.instructorProfileId === null
        ? { isSuccess: false }
        : await deleteInstructorProfile(pending.user.instructorProfileId);
    setIsSubmitting(false);
    if (result.isSuccess) {
      showSuccessToast(isApprove ? 'Đã duyệt Instructor' : 'Đã xóa Hồ sơ Instructor');
      setPending(null);
      reload();
      return;
    }
    showErrorToast(isApprove ? APPROVE_ERROR_MESSAGE : DELETE_ERROR_MESSAGE);
    setPending(null);
  };

  const changeFilter = (value: UserRoleFilter | '') => {
    setRoleFilter(value);
    setPageNumber(1);
  };

  const renderBody = () => {
    if (state.kind === 'Loading') {
      return (
        <DelayedSkeleton>
          <div className="flex flex-col gap-2">
            <Skeleton className="h-10" />
            <Skeleton className="h-10" />
            <Skeleton className="h-10" />
          </div>
        </DelayedSkeleton>
      );
    }
    if (state.kind === 'Error') {
      return (
        <div className="flex flex-col gap-3">
          <Banner tone="error">{LOAD_ERROR_MESSAGE}</Banner>
          <div>
            <Button tone="secondary" onClick={reload}>
              Thử lại
            </Button>
          </div>
        </div>
      );
    }
    const { page } = state;
    if (page.items.length === 0) {
      return roleFilter === '' ? (
        <EmptyState kind="NoData" message="Chưa có người dùng nào." />
      ) : (
        <EmptyState
          kind="NoResults"
          message="Không có người dùng nào khớp bộ lọc."
          actionLabel="Xóa bộ lọc"
          onAction={() => {
            changeFilter('');
          }}
        />
      );
    }
    const pageCount = Math.max(1, Math.ceil(page.totalCount / USERS_PAGE_SIZE));
    return (
      <div className="flex flex-col gap-4">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Email</TableHead>
              <TableHead>Tên hiển thị</TableHead>
              <TableHead>Vai trò</TableHead>
              <TableHead>Hồ sơ Instructor</TableHead>
              <TableHead>
                <span className="sr-only">Thao tác</span>
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {page.items.map((user) => {
              const userRole = describeRole(user);
              const email = user.email ?? '(không có email)';
              return (
                <TableRow key={user.userId}>
                  <TableCell>{email}</TableCell>
                  <TableCell>{user.displayName ?? '—'}</TableCell>
                  <TableCell>
                    <Badge tone={userRole.tone}>{userRole.label}</Badge>
                  </TableCell>
                  <TableCell>{user.instructorProfileId === null ? 'Chưa có' : 'Đã có'}</TableCell>
                  <TableCell>
                    <div className="flex justify-end gap-2">
                      {canApprove(user) ? (
                        <Button
                          tone="secondary"
                          onClick={() => {
                            setPending({ kind: 'Approve', user });
                          }}
                        >
                          Duyệt Instructor
                        </Button>
                      ) : null}
                      {user.instructorProfileId !== null ? (
                        <>
                          <Button
                            tone="secondary"
                            aria-label={`Sửa hồ sơ ${email}`}
                            onClick={() =>
                              void navigate(`/instructors/${user.instructorProfileId ?? ''}`)
                            }
                          >
                            Sửa hồ sơ
                          </Button>
                          <Button
                            tone="danger"
                            aria-label={`Xóa hồ sơ ${email}`}
                            onClick={() => {
                              setPending({ kind: 'Delete', user });
                            }}
                          >
                            Xóa hồ sơ
                          </Button>
                        </>
                      ) : null}
                    </div>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
        <div className="flex items-center justify-between text-sm text-text-muted">
          <span>
            Trang {page.pageNumber}/{pageCount}
          </span>
          <div className="flex gap-2">
            <Button
              tone="secondary"
              disabled={page.pageNumber <= 1}
              onClick={() => {
                setPageNumber(page.pageNumber - 1);
              }}
            >
              Trang trước
            </Button>
            <Button
              tone="secondary"
              disabled={page.pageNumber >= pageCount}
              onClick={() => {
                setPageNumber(page.pageNumber + 1);
              }}
            >
              Trang sau
            </Button>
          </div>
        </div>
      </div>
    );
  };

  const pendingEmail = pending?.user.email ?? 'người dùng này';
  return (
    <div className="flex flex-col gap-6">
      <h1 className="text-2xl font-semibold text-text">Instructor</h1>
      <div className="flex items-center gap-2">
        <label htmlFor="role-filter" className="text-sm font-medium text-text">
          Vai trò
        </label>
        <select
          id="role-filter"
          value={roleFilter}
          onChange={(event) => {
            changeFilter(event.target.value as UserRoleFilter | '');
          }}
          className="min-h-9 rounded-lg border border-border-input bg-card px-3 text-sm text-text"
        >
          {ROLE_FILTERS.map((filter) => (
            <option key={filter.value} value={filter.value}>
              {filter.label}
            </option>
          ))}
        </select>
      </div>
      {renderBody()}
      <ConfirmDialog
        isOpen={pending !== null}
        onOpenChange={(isOpen) => {
          if (!isOpen) {
            setPending(null);
          }
        }}
        title={pending?.kind === 'Delete' ? 'Xóa Hồ sơ Instructor này?' : 'Duyệt Instructor này?'}
        description={
          pending?.kind === 'Delete'
            ? `${pendingEmail} sẽ mất Hồ sơ Instructor và quyền Instructor, phải đăng nhập lại.`
            : `${pendingEmail} sẽ trở thành Instructor, có Hồ sơ Instructor và phải đăng nhập lại để vào App quản trị.`
        }
        confirmLabel={pending?.kind === 'Delete' ? 'Xóa hồ sơ' : 'Duyệt Instructor'}
        isDangerous={pending?.kind === 'Delete'}
        isSubmitting={isSubmitting}
        onConfirm={() => {
          void confirm();
        }}
      />
    </div>
  );
}
