import * as DialogPrimitive from '@radix-ui/react-dialog';
import { Button } from './button';

export interface ConfirmDialogProps {
  isOpen: boolean;
  onOpenChange: (isOpen: boolean) => void;
  /** Câu hỏi hành động, ví dụ "Hủy đơn này?". */
  title: string;
  /** Nêu hệ quả cụ thể. */
  description: string;
  /** Động từ đặt tên nút, ví dụ "Hủy đơn". */
  confirmLabel: string;
  cancelLabel?: string;
  isDangerous?: boolean;
  isSubmitting?: boolean;
  onConfirm: () => void;
}

/** Modal xác nhận (ux.md mục 5): Esc và bấm nền đóng, trừ khi đang gửi; Radix giữ focus và trả focus. */
export function ConfirmDialog({
  isOpen,
  onOpenChange,
  title,
  description,
  confirmLabel,
  cancelLabel = 'Quay lại',
  isDangerous = false,
  isSubmitting = false,
  onConfirm,
}: ConfirmDialogProps) {
  const guardClose = (event: Event) => {
    if (isSubmitting) event.preventDefault();
  };

  return (
    <DialogPrimitive.Root
      open={isOpen}
      onOpenChange={(next) => {
        if (!isSubmitting) onOpenChange(next);
      }}
    >
      <DialogPrimitive.Portal>
        <DialogPrimitive.Overlay className="fixed inset-0 bg-text/50" />
        <DialogPrimitive.Content
          onEscapeKeyDown={guardClose}
          onPointerDownOutside={guardClose}
          className="fixed top-1/2 left-1/2 w-[calc(100%-2rem)] max-w-md -translate-x-1/2 -translate-y-1/2 rounded-xl bg-card p-6 shadow-lg"
        >
          <DialogPrimitive.Title className="text-lg font-semibold text-text">
            {title}
          </DialogPrimitive.Title>
          <DialogPrimitive.Description className="mt-2 text-sm text-text-muted">
            {description}
          </DialogPrimitive.Description>
          <div className="mt-6 flex justify-end gap-3">
            <Button
              tone="secondary"
              disabled={isSubmitting}
              onClick={() => {
                onOpenChange(false);
              }}
            >
              {cancelLabel}
            </Button>
            <Button
              tone={isDangerous ? 'danger' : 'primary'}
              isLoading={isSubmitting}
              onClick={onConfirm}
            >
              {confirmLabel}
            </Button>
          </div>
        </DialogPrimitive.Content>
      </DialogPrimitive.Portal>
    </DialogPrimitive.Root>
  );
}
