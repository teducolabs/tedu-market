import {
  Banner,
  Button,
  ConfirmDialog,
  DelayedSkeleton,
  EmptyState,
  FormField,
  Input,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  showErrorToast,
  showSuccessToast,
} from '@tedu-market/ui';
import { useCallback, useEffect, useRef, useState, type SyntheticEvent } from 'react';
import { useNavigate, useOutletContext } from 'react-router';
import {
  createCategory,
  deleteCategory,
  listCategories,
  updateCategory,
  type CategoryItem,
} from '../lib/categories/category-api';
import type { AdminAppRole } from '../lib/sidebar-items';

const LOAD_ERROR_MESSAGE = 'Có sự cố khi tải danh sách. Bạn thử lại sau ít phút.';
const DELETE_ERROR_MESSAGE = 'Không thể xóa Category. Bạn thử lại sau ít phút.';
export const REQUIRED_NAME_MESSAGE = 'Bạn cần nhập tên Category';

type ListState =
  { kind: 'Loading' } | { kind: 'Error' } | { kind: 'Success'; items: CategoryItem[] };

/** `null` là đóng form; `category` là `null` khi tạo mới, có giá trị khi sửa. */
type FormState = { category: CategoryItem | null } | null;

/** Màn hình Category của Admin (FR-17): bảng, form tạo/sửa, modal xác nhận xóa. */
export function CategoriesPage() {
  const navigate = useNavigate();
  const { role } = useOutletContext<{ role: AdminAppRole }>();
  const formRef = useRef<HTMLFormElement>(null);
  const [reloadCount, setReloadCount] = useState(0);
  const [state, setState] = useState<ListState>({ kind: 'Loading' });
  const [form, setForm] = useState<FormState>(null);
  const [nameError, setNameError] = useState<string | undefined>(undefined);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [pendingDelete, setPendingDelete] = useState<CategoryItem | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    if (role !== 'Admin') {
      return;
    }
    let isCancelled = false;
    setState({ kind: 'Loading' });
    void listCategories().then((result) => {
      if (!isCancelled) {
        setState(result.isSuccess ? { kind: 'Success', items: result.value } : { kind: 'Error' });
      }
    });
    return () => {
      isCancelled = true;
    };
  }, [role, reloadCount]);

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

  const openForm = (category: CategoryItem | null) => {
    setNameError(undefined);
    setForm({ category });
  };

  const focusInvalidName = () => {
    formRef.current?.querySelector<HTMLElement>('[aria-invalid="true"]')?.focus();
  };

  const handleSubmit = async (event: SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (form === null) {
      return;
    }
    const value = new FormData(event.currentTarget).get('name');
    const name = typeof value === 'string' ? value : '';
    if (name.trim() === '') {
      setNameError(REQUIRED_NAME_MESSAGE);
      queueMicrotask(focusInvalidName);
      return;
    }
    setNameError(undefined);
    setIsSubmitting(true);
    const result =
      form.category === null
        ? await createCategory(name)
        : await updateCategory(form.category.categoryId, name);
    setIsSubmitting(false);
    if (result.isSuccess) {
      showSuccessToast('Đã lưu');
      setForm(null);
      reload();
      return;
    }
    setNameError(result.fieldErrors.name);
    if (result.message !== null) {
      showErrorToast(result.message);
    }
    queueMicrotask(focusInvalidName);
  };

  const confirmDelete = async () => {
    if (pendingDelete === null) {
      return;
    }
    setIsDeleting(true);
    const result = await deleteCategory(pendingDelete.categoryId);
    setIsDeleting(false);
    if (result.isSuccess) {
      showSuccessToast('Đã xóa Category');
      reload();
    } else {
      showErrorToast(DELETE_ERROR_MESSAGE);
    }
    setPendingDelete(null);
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
    if (state.items.length === 0) {
      return (
        <EmptyState
          kind="NoData"
          message="Chưa có Category nào."
          actionLabel="Tạo Category"
          onAction={() => {
            openForm(null);
          }}
        />
      );
    }
    return (
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Tên</TableHead>
            <TableHead>
              <span className="sr-only">Thao tác</span>
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {state.items.map((category) => (
            <TableRow key={category.categoryId}>
              <TableCell>{category.name}</TableCell>
              <TableCell>
                <div className="flex justify-end gap-2">
                  <Button
                    tone="secondary"
                    aria-label={`Sửa Category ${category.name}`}
                    onClick={() => {
                      openForm(category);
                    }}
                  >
                    Sửa
                  </Button>
                  <Button
                    tone="danger"
                    aria-label={`Xóa Category ${category.name}`}
                    onClick={() => {
                      setPendingDelete(category);
                    }}
                  >
                    Xóa
                  </Button>
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    );
  };

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-text">Category</h1>
        {form === null ? (
          <Button
            onClick={() => {
              openForm(null);
            }}
          >
            Tạo Category
          </Button>
        ) : null}
      </div>
      {form === null ? null : (
        <form
          key={form.category?.categoryId ?? 'new'}
          ref={formRef}
          noValidate
          aria-label={form.category === null ? 'Tạo Category' : 'Sửa Category'}
          onSubmit={(event) => {
            void handleSubmit(event);
          }}
          className="flex max-w-md flex-col gap-4 rounded-lg border border-border bg-card p-4"
        >
          <FormField label="Tên" isRequired error={nameError}>
            {(control) => (
              <Input
                {...control}
                name="name"
                maxLength={100}
                defaultValue={form.category?.name ?? ''}
              />
            )}
          </FormField>
          <div className="flex gap-3">
            <Button type="submit" isLoading={isSubmitting}>
              Lưu
            </Button>
            <Button
              tone="secondary"
              disabled={isSubmitting}
              onClick={() => {
                setForm(null);
              }}
            >
              Quay lại
            </Button>
          </div>
        </form>
      )}
      {renderBody()}
      <ConfirmDialog
        isOpen={pendingDelete !== null}
        onOpenChange={(isOpen) => {
          if (!isOpen) {
            setPendingDelete(null);
          }
        }}
        title="Xóa Category này?"
        description={`Category "${pendingDelete?.name ?? ''}" sẽ biến mất khỏi danh sách và bộ lọc Khóa học.`}
        confirmLabel="Xóa Category"
        isDangerous
        isSubmitting={isDeleting}
        onConfirm={() => {
          void confirmDelete();
        }}
      />
    </div>
  );
}
