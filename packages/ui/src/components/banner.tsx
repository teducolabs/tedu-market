import { cva, type VariantProps } from 'class-variance-authority';
import { AlertCircle, CheckCircle2, Info, TriangleAlert, type LucideIcon } from 'lucide-react';
import type { ReactNode } from 'react';

const bannerVariants = cva('flex items-start gap-3 rounded-lg border px-4 py-3 text-sm', {
  variants: {
    tone: {
      info: 'border-primary bg-primary-tint text-primary-strong',
      success: 'border-success bg-success-tint text-success-strong',
      warning: 'border-warning bg-warning-tint text-warning-strong',
      error: 'border-error bg-error-tint text-error-strong',
    },
  },
  defaultVariants: { tone: 'info' },
});

type BannerTone = NonNullable<VariantProps<typeof bannerVariants>['tone']>;

const ICONS: Record<BannerTone, LucideIcon> = {
  info: Info,
  success: CheckCircle2,
  warning: TriangleAlert,
  error: AlertCircle,
};

/** Khối ngang đầu vùng nội dung, có icon, không tự tắt (ux.md mục 5). Lỗi dùng aria-live assertive. */
export function Banner({ tone = 'info', children }: { tone?: BannerTone; children: ReactNode }) {
  const Icon = ICONS[tone];
  return (
    <div
      role={tone === 'error' ? 'alert' : 'status'}
      aria-live={tone === 'error' ? 'assertive' : 'polite'}
      className={bannerVariants({ tone })}
    >
      <Icon aria-hidden className="mt-0.5 size-5 shrink-0" />
      <div>{children}</div>
    </div>
  );
}
