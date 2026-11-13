import { useEffect, useState, type HTMLAttributes, type ReactNode } from 'react';
import { cn } from '../lib/cn';

/** Nhấp nháy nhẹ, tắt khi người dùng bật giảm chuyển động. */
export function Skeleton({ className, ...rest }: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      aria-hidden
      className={cn('animate-pulse rounded-lg bg-border motion-reduce:animate-none', className)}
      {...rest}
    />
  );
}

export const SKELETON_DELAY_MS = 300;

/** Chỉ hiện Skeleton nếu tải lâu hơn 300 ms (ux.md mục 5). */
export function DelayedSkeleton({
  delayMs = SKELETON_DELAY_MS,
  label = 'Đang tải…',
  children,
}: {
  delayMs?: number;
  label?: string;
  children: ReactNode;
}) {
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => {
      setIsVisible(true);
    }, delayMs);
    return () => {
      clearTimeout(timer);
    };
  }, [delayMs]);

  return (
    <div role="status" aria-busy="true">
      <span className="sr-only">{label}</span>
      {isVisible ? children : null}
    </div>
  );
}
