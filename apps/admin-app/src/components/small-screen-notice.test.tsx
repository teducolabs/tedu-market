import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { SmallScreenNotice } from './small-screen-notice';

describe('SmallScreenNotice', () => {
  it('shows the desktop-only message', () => {
    // Arrange / Act
    render(<SmallScreenNotice onSignOut={vi.fn()} />);

    // Assert
    expect(
      screen.getByText('Vui lòng dùng máy tính hoặc máy tính bảng để quản trị'),
    ).toBeInTheDocument();
  });

  it('calls onSignOut when Đăng xuất is clicked', async () => {
    // Arrange
    const user = userEvent.setup();
    const onSignOut = vi.fn();
    render(<SmallScreenNotice onSignOut={onSignOut} />);

    // Act
    await user.click(screen.getByRole('button', { name: 'Đăng xuất' }));

    // Assert
    expect(onSignOut).toHaveBeenCalledOnce();
  });
});
