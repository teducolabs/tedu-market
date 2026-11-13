import { cva, type VariantProps } from 'class-variance-authority';
import { Loader2 } from 'lucide-react';
import type { ButtonHTMLAttributes } from 'react';
import { cn } from '../lib/cn';

const buttonVariants = cva(
  'inline-flex min-h-11 items-center justify-center gap-2 rounded-lg px-4 text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-60 md:min-h-9',
  {
    variants: {
      tone: {
        primary: 'bg-primary text-white hover:bg-primary-strong',
        secondary:
          'border border-border-input bg-card text-secondary-strong hover:bg-secondary-tint',
        danger: 'bg-error text-white hover:bg-error-strong',
      },
    },
    defaultVariants: { tone: 'primary' },
  },
);

export interface ButtonProps
  extends ButtonHTMLAttributes<HTMLButtonElement>, VariantProps<typeof buttonVariants> {
  /** Đang gửi: hiện spinner và khóa nút (ux.md mục 5, Form). */
  isLoading?: boolean;
}

export function Button({
  tone,
  isLoading = false,
  disabled,
  className,
  children,
  type = 'button',
  ...rest
}: ButtonProps) {
  return (
    <button
      type={type}
      disabled={disabled === true || isLoading}
      aria-busy={isLoading}
      className={cn(buttonVariants({ tone }), className)}
      {...rest}
    >
      {isLoading ? <Loader2 aria-hidden className="size-4 animate-spin" /> : null}
      {children}
    </button>
  );
}
