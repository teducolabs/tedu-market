import { AlertCircle } from 'lucide-react';
import { useId, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '../lib/cn';

interface FieldControlProps {
  id: string;
  'aria-invalid': boolean;
  'aria-describedby': string | undefined;
  'aria-required': boolean;
}

export interface FormFieldProps {
  label: string;
  /** Trường bắt buộc ghi "(bắt buộc)" bằng chữ. */
  isRequired?: boolean;
  /** Lỗi hiện dưới ô, màu `error` kèm icon. */
  error?: string | undefined;
  hint?: string;
  children: (control: FieldControlProps) => ReactNode;
}

/** Label phía trên input, lỗi dưới ô gắn bằng aria-describedby (ux.md mục 5 và 6). */
export function FormField({ label, isRequired = false, error, hint, children }: FormFieldProps) {
  const id = useId();
  const errorId = `${id}-error`;
  const hintId = `${id}-hint`;
  const hasError = error !== undefined && error !== '';
  const describedBy =
    [hint ? hintId : undefined, hasError ? errorId : undefined].filter(Boolean).join(' ') ||
    undefined;

  return (
    <div className="flex flex-col gap-1">
      <label htmlFor={id} className="text-sm font-medium text-text">
        {label}
        {isRequired ? <span className="font-normal text-text-muted"> (bắt buộc)</span> : null}
      </label>
      {children({
        id,
        'aria-invalid': hasError,
        'aria-describedby': describedBy,
        'aria-required': isRequired,
      })}
      {hint ? (
        <p id={hintId} className="text-xs text-text-muted">
          {hint}
        </p>
      ) : null}
      <p
        id={errorId}
        aria-live="polite"
        className={cn('flex items-center gap-1 text-sm text-error', !hasError && 'hidden')}
      >
        {hasError ? (
          <>
            <AlertCircle aria-hidden className="size-4 shrink-0" />
            {error}
          </>
        ) : null}
      </p>
    </div>
  );
}

export function Input({ className, ...rest }: InputHTMLAttributes<HTMLInputElement>) {
  return (
    <input
      className={cn(
        'min-h-11 w-full rounded-lg border border-border-input bg-card px-3 text-base text-text md:min-h-9 aria-[invalid=true]:border-error',
        className,
      )}
      {...rest}
    />
  );
}
