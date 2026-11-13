import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { ConfirmDialog } from './dialog';

function renderDialog(overrides: { isSubmitting?: boolean } = {}) {
  const onOpenChange = vi.fn();
  const onConfirm = vi.fn();
  render(
    <ConfirmDialog
      isOpen
      onOpenChange={onOpenChange}
      title="Hủy đơn này?"
      description="Đơn chuyển sang Đã hủy."
      confirmLabel="Hủy đơn"
      isDangerous
      onConfirm={onConfirm}
      {...overrides}
    />,
  );
  return { onOpenChange, onConfirm };
}

describe('ConfirmDialog', () => {
  it('names the buttons with a verb and "Quay lại"', () => {
    // Arrange / Act
    renderDialog();

    // Assert
    expect(screen.getByRole('dialog', { name: 'Hủy đơn này?' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Hủy đơn' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Quay lại' })).toBeInTheDocument();
  });

  it('closes on Esc when idle', async () => {
    // Arrange
    const { onOpenChange } = renderDialog();

    // Act
    await userEvent.keyboard('{Escape}');

    // Assert
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });

  it('does not close on Esc while submitting', async () => {
    // Arrange
    const { onOpenChange } = renderDialog({ isSubmitting: true });

    // Act
    await userEvent.keyboard('{Escape}');

    // Assert
    expect(onOpenChange).not.toHaveBeenCalled();
  });

  it('calls onConfirm when the action button is pressed', async () => {
    // Arrange
    const { onConfirm } = renderDialog();

    // Act
    await userEvent.click(screen.getByRole('button', { name: 'Hủy đơn' }));

    // Assert
    expect(onConfirm).toHaveBeenCalledOnce();
  });
});
