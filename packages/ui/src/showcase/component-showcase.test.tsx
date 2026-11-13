import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { ComponentShowcase } from './component-showcase';

async function selectState(label: string) {
  await userEvent.click(screen.getByRole('button', { name: label }));
}

describe('ComponentShowcase', () => {
  it('shows data and success feedback in the Success state', () => {
    // Arrange / Act
    render(<ComponentShowcase />);

    // Assert
    expect(screen.getByRole('table', { name: 'Đơn' })).toBeInTheDocument();
    expect(screen.getByText('Đã lưu.')).toBeInTheDocument();
  });

  it('shows skeleton and busy submit button in the Loading state', async () => {
    // Arrange
    render(<ComponentShowcase />);

    // Act
    await selectState('Đang tải');

    // Assert
    expect(screen.getByRole('button', { name: 'Thêm vào giỏ' })).toBeDisabled();
    expect(screen.getByText('Đang tải…')).toBeInTheDocument();
  });

  it('shows the empty state in the Empty state', async () => {
    // Arrange
    render(<ComponentShowcase />);

    // Act
    await selectState('Trống');

    // Assert
    expect(screen.getByText('Chưa có Đơn nào.')).toBeInTheDocument();
  });

  it('shows field error and error banner in the Error state', async () => {
    // Arrange
    render(<ComponentShowcase />);

    // Act
    await selectState('Lỗi');

    // Assert
    expect(screen.getByLabelText(/Coupon/)).toHaveAccessibleDescription(
      'Coupon đã hết hạn. Bạn thử Coupon khác nhé.',
    );
    expect(screen.getByRole('alert')).toHaveTextContent('Có sự cố khi tải dữ liệu.');
  });

  it('lists a badge for every status', () => {
    // Arrange / Act
    render(<ComponentShowcase />);

    // Assert
    expect(screen.getByText('Khoản chia chờ duyệt')).toBeInTheDocument();
    expect(screen.getByText('Bị từ chối')).toBeInTheDocument();
  });
});
