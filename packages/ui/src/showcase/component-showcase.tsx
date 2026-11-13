import { useState } from 'react';
import { Banner } from '../components/banner';
import { Button } from '../components/button';
import { ConfirmDialog } from '../components/dialog';
import { EmptyState } from '../components/empty-state';
import { FormField, Input } from '../components/form-field';
import { DelayedSkeleton, Skeleton } from '../components/skeleton';
import { STATUS_BADGE_STATUSES, StatusBadge } from '../components/status-badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../components/table';
import { showErrorToast, showSuccessToast } from '../components/toast';

export type ShowcaseState = 'Loading' | 'Empty' | 'Error' | 'Success';

const STATE_LABELS: Record<ShowcaseState, string> = {
  Loading: 'Đang tải',
  Empty: 'Trống',
  Error: 'Lỗi',
  Success: 'Thành công',
};

const ROWS = [
  { orderCode: 'DH-0001', status: 'New' },
  { orderCode: 'DH-0002', status: 'Paid' },
  { orderCode: 'DH-0003', status: 'Expired' },
] as const;

function OrderTable({ state }: { state: ShowcaseState }) {
  if (state === 'Loading') {
    return (
      <DelayedSkeleton delayMs={0}>
        <div className="flex flex-col gap-2">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      </DelayedSkeleton>
    );
  }
  if (state === 'Error') {
    return <Banner tone="error">Có sự cố khi tải dữ liệu. Bạn thử lại sau ít phút.</Banner>;
  }
  if (state === 'Empty') {
    return <EmptyState kind="NoData" message="Chưa có Đơn nào." />;
  }
  return (
    <Table aria-label="Đơn">
      <TableHeader>
        <TableRow>
          <TableHead>Mã đơn</TableHead>
          <TableHead>Trạng thái</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {ROWS.map((row) => (
          <TableRow key={row.orderCode}>
            <TableCell>{row.orderCode}</TableCell>
            <TableCell>
              <StatusBadge status={row.status} />
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

function CouponForm({ state }: { state: ShowcaseState }) {
  return (
    <form
      className="flex max-w-sm flex-col gap-4"
      onSubmit={(event) => {
        event.preventDefault();
      }}
    >
      <FormField
        label="Coupon"
        isRequired
        error={state === 'Error' ? 'Coupon đã hết hạn. Bạn thử Coupon khác nhé.' : undefined}
      >
        {(control) => <Input {...control} defaultValue="TEDU10" />}
      </FormField>
      <Button type="submit" isLoading={state === 'Loading'}>
        Thêm vào giỏ
      </Button>
      {state === 'Success' ? <Banner tone="success">Đã lưu.</Banner> : null}
    </form>
  );
}

/** Trang mẫu: mọi component ở bốn trạng thái Loading/Empty/Error/Success (T07). */
export function ComponentShowcase() {
  const [state, setState] = useState<ShowcaseState>('Success');
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  return (
    <main className="mx-auto flex max-w-7xl flex-col gap-8 p-4 md:p-6">
      <h1 className="text-2xl font-semibold">Component mẫu</h1>

      <div role="group" aria-label="Trạng thái" className="flex flex-wrap gap-2">
        {(Object.keys(STATE_LABELS) as ShowcaseState[]).map((option) => (
          <Button
            key={option}
            tone={option === state ? 'primary' : 'secondary'}
            aria-pressed={option === state}
            onClick={() => {
              setState(option);
            }}
          >
            {STATE_LABELS[option]}
          </Button>
        ))}
      </div>

      <section className="flex flex-col gap-3" aria-labelledby="showcase-form">
        <h2 id="showcase-form" className="text-xl font-semibold">
          Form
        </h2>
        <CouponForm state={state} />
      </section>

      <section className="flex flex-col gap-3" aria-labelledby="showcase-table">
        <h2 id="showcase-table" className="text-xl font-semibold">
          Table
        </h2>
        <OrderTable state={state} />
      </section>

      <section className="flex flex-col gap-3" aria-labelledby="showcase-badge">
        <h2 id="showcase-badge" className="text-xl font-semibold">
          Badge trạng thái
        </h2>
        <div className="flex flex-wrap gap-2">
          {STATUS_BADGE_STATUSES.map((status) => (
            <StatusBadge key={status} status={status} />
          ))}
        </div>
      </section>

      <section className="flex flex-col gap-3" aria-labelledby="showcase-feedback">
        <h2 id="showcase-feedback" className="text-xl font-semibold">
          Modal và Toast
        </h2>
        <div className="flex flex-wrap gap-2">
          <Button
            tone="danger"
            onClick={() => {
              setIsDialogOpen(true);
            }}
          >
            Hủy đơn
          </Button>
          <Button
            tone="secondary"
            onClick={() => {
              if (state === 'Error') showErrorToast('Có sự cố khi lưu. Bạn thử lại sau ít phút.');
              else showSuccessToast('Đã lưu');
            }}
          >
            Hiện thông báo
          </Button>
        </div>
        <ConfirmDialog
          isOpen={isDialogOpen}
          onOpenChange={setIsDialogOpen}
          title="Hủy đơn này?"
          description="Đơn chuyển sang Đã hủy và Enrollment (nếu có) bị thu hồi."
          confirmLabel="Hủy đơn"
          isDangerous
          isSubmitting={state === 'Loading'}
          onConfirm={() => {
            setIsDialogOpen(false);
          }}
        />
      </section>
    </main>
  );
}
