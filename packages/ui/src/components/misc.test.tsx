import { act, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { Banner } from './banner';
import { Button } from './button';
import { EmptyState } from './empty-state';
import { DelayedSkeleton } from './skeleton';

describe('Banner', () => {
  it('announces errors with role alert', () => {
    // Arrange / Act
    render(<Banner tone="error">Có sự cố.</Banner>);

    // Assert
    expect(screen.getByRole('alert')).toHaveTextContent('Có sự cố.');
  });

  it('announces other tones politely with role status', () => {
    // Arrange / Act
    render(<Banner tone="warning">Đang chờ Admin.</Banner>);

    // Assert
    expect(screen.getByRole('status')).toHaveAttribute('aria-live', 'polite');
  });
});

describe('Button', () => {
  it('disables itself and marks busy while loading', () => {
    // Arrange / Act
    render(<Button isLoading>Gửi duyệt</Button>);

    // Assert
    expect(screen.getByRole('button', { name: 'Gửi duyệt' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Gửi duyệt' })).toHaveAttribute('aria-busy', 'true');
  });

  it('stays enabled when the form is merely invalid', () => {
    // Arrange / Act
    render(<Button>Gửi duyệt</Button>);

    // Assert
    expect(screen.getByRole('button', { name: 'Gửi duyệt' })).toBeEnabled();
  });
});

describe('EmptyState', () => {
  it('shows the reason and runs the single action', async () => {
    // Arrange
    const onAction = vi.fn();
    render(
      <EmptyState
        kind="NoResults"
        message="Không tìm thấy kết quả."
        actionLabel="Xóa bộ lọc"
        onAction={onAction}
      />,
    );

    // Act
    await userEvent.click(screen.getByRole('button', { name: 'Xóa bộ lọc' }));

    // Assert
    expect(screen.getByText('Không tìm thấy kết quả.')).toBeInTheDocument();
    expect(onAction).toHaveBeenCalledOnce();
  });

  it('renders no button without an action', () => {
    // Arrange / Act
    render(<EmptyState kind="NoPermission" message="Bạn không có quyền xem trang này." />);

    // Assert
    expect(screen.queryByRole('button')).toBeNull();
  });
});

describe('DelayedSkeleton', () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it('hides the skeleton before 300 ms and shows it after', () => {
    // Arrange
    vi.useFakeTimers();
    render(
      <DelayedSkeleton>
        <p>khung chờ</p>
      </DelayedSkeleton>,
    );

    // Act
    const before = screen.queryByText('khung chờ');
    act(() => {
      vi.advanceTimersByTime(300);
    });

    // Assert
    expect(before).toBeNull();
    expect(screen.getByText('khung chờ')).toBeInTheDocument();
  });
});
