import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { STATUS_BADGE_STATUSES, StatusBadge, getStatusBadgeLabel } from './status-badge';

const GLOSSARY_LABELS = [
  'Mới',
  'Đang chờ',
  'Đã thanh toán',
  'Đã hủy',
  'Hết hạn',
  'Chưa đăng',
  'Chờ duyệt',
  'Đã đăng',
  'Bị từ chối',
  'Đang xử lý',
  'Sẵn sàng',
  'Lỗi',
  'Khoản chia chờ duyệt',
];
const FORBIDDEN_LABELS = ['Pending', 'Completed', 'Paid', 'Canceled', 'Hoàn thành', 'huỷ'];

describe('StatusBadge', () => {
  it('uses exactly the glossary labels for every status', () => {
    // Arrange
    const labels = STATUS_BADGE_STATUSES.map(getStatusBadgeLabel);

    // Act
    const sorted = [...labels].sort();

    // Assert
    expect(sorted).toEqual([...GLOSSARY_LABELS].sort());
  });

  it('never uses a forbidden alias from the glossary', () => {
    // Arrange
    const labels = STATUS_BADGE_STATUSES.map(getStatusBadgeLabel);

    // Act
    const hasForbidden = labels.some((label) => FORBIDDEN_LABELS.includes(label));

    // Assert
    expect(hasForbidden).toBe(false);
  });

  it('renders the label as text, not only as colour', () => {
    // Arrange / Act
    render(<StatusBadge status="Paid" />);

    // Assert
    expect(screen.getByText('Đã thanh toán')).toBeInTheDocument();
  });

  it('shows a spinner for the video processing status', () => {
    // Arrange / Act
    const { container } = render(<StatusBadge status="VideoProcessing" />);

    // Assert
    expect(container.querySelector('svg.animate-spin')).not.toBeNull();
  });
});
