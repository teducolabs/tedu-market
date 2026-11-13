import { Badge, type BadgeTone } from './badge';

/** Khóa là tên Code trong glossary; nhãn là cột Hiển thị. Màu theo ux.md mục 5 (Badge trạng thái). */
const STATUS_BADGES = {
  New: { label: 'Mới', tone: 'primary' },
  OnHold: { label: 'Đang chờ', tone: 'warning' },
  Paid: { label: 'Đã thanh toán', tone: 'success' },
  Cancelled: { label: 'Đã hủy', tone: 'secondary' },
  Expired: { label: 'Hết hạn', tone: 'error' },
  Draft: { label: 'Chưa đăng', tone: 'secondary' },
  PendingApproval: { label: 'Chờ duyệt', tone: 'warning' },
  Published: { label: 'Đã đăng', tone: 'success' },
  Rejected: { label: 'Bị từ chối', tone: 'error' },
  VideoProcessing: { label: 'Đang xử lý', tone: 'warning', isBusy: true },
  VideoReady: { label: 'Sẵn sàng', tone: 'success' },
  VideoFailed: { label: 'Lỗi', tone: 'error' },
  PendingShare: { label: 'Khoản chia chờ duyệt', tone: 'warning' },
} as const satisfies Record<string, { label: string; tone: BadgeTone; isBusy?: boolean }>;

export type StatusBadgeStatus = keyof typeof STATUS_BADGES;

export const STATUS_BADGE_STATUSES = Object.keys(STATUS_BADGES) as StatusBadgeStatus[];

export function getStatusBadgeLabel(status: StatusBadgeStatus): string {
  return STATUS_BADGES[status].label;
}

export function StatusBadge({ status }: { status: StatusBadgeStatus }) {
  const definition: { label: string; tone: BadgeTone; isBusy?: boolean } = STATUS_BADGES[status];
  return (
    <Badge tone={definition.tone} isBusy={definition.isBusy === true}>
      {definition.label}
    </Badge>
  );
}
