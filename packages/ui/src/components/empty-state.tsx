import { FileQuestion, Inbox, Lock, type LucideIcon } from 'lucide-react';
import { Button } from './button';

export type EmptyStateKind = 'NoData' | 'NoResults' | 'NoPermission';

const ICONS: Record<EmptyStateKind, LucideIcon> = {
  NoData: Inbox,
  NoResults: FileQuestion,
  NoPermission: Lock,
};

export interface EmptyStateProps {
  kind: EmptyStateKind;
  /** 1 câu nói lý do. */
  message: string;
  /** Tối đa 1 nút hành động. */
  actionLabel?: string;
  onAction?: () => void;
}

/** Icon + 1 câu lý do + tối đa 1 nút (ux.md mục 5). */
export function EmptyState({ kind, message, actionLabel, onAction }: EmptyStateProps) {
  const Icon = ICONS[kind];
  return (
    <div className="flex flex-col items-center gap-3 px-4 py-12 text-center">
      <Icon aria-hidden className="size-10 text-text-muted" />
      <p className="text-base text-text">{message}</p>
      {actionLabel && onAction ? (
        <Button tone="secondary" onClick={onAction}>
          {actionLabel}
        </Button>
      ) : null}
    </div>
  );
}
