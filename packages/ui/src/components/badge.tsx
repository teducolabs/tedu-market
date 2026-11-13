import { cva, type VariantProps } from 'class-variance-authority';
import { Loader2 } from 'lucide-react';
import type { HTMLAttributes } from 'react';
import { cn } from '../lib/cn';

/** Chữ trên nền tint dùng màu đậm của chính token để đạt contrast ≥ 4.5:1 (ux.md mục 4). */
const badgeVariants = cva(
  'inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium',
  {
    variants: {
      tone: {
        primary: 'bg-primary-tint text-primary-strong',
        secondary: 'bg-secondary-tint text-secondary-strong',
        success: 'bg-success-tint text-success-strong',
        warning: 'bg-warning-tint text-warning-strong',
        error: 'bg-error-tint text-error-strong',
      },
    },
    defaultVariants: { tone: 'secondary' },
  },
);

export type BadgeTone = NonNullable<VariantProps<typeof badgeVariants>['tone']>;

export interface BadgeProps
  extends HTMLAttributes<HTMLSpanElement>, VariantProps<typeof badgeVariants> {
  /** Spinner nhỏ cho trạng thái đang xử lý. */
  isBusy?: boolean;
}

export function Badge({ tone, isBusy = false, className, children, ...rest }: BadgeProps) {
  return (
    <span className={cn(badgeVariants({ tone }), className)} {...rest}>
      {isBusy ? <Loader2 aria-hidden className="size-3 animate-spin" /> : null}
      {children}
    </span>
  );
}
