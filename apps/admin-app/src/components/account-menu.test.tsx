import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { describe, expect, it, vi } from 'vitest';
import { AccountMenu } from './account-menu';

const renderMenu = (role: 'Admin' | 'Instructor', onSignOut = vi.fn()) => {
  render(
    <MemoryRouter>
      <AccountMenu role={role} userName="Lan" labelClass="inline" onSignOut={onSignOut} />
    </MemoryRouter>,
  );
  return onSignOut;
};

describe('AccountMenu', () => {
  it('shows Đổi mật khẩu and Đăng xuất for Admin', async () => {
    // Arrange
    renderMenu('Admin');

    // Act
    await userEvent.setup().click(screen.getByRole('button', { name: 'Menu tài khoản' }));

    // Assert
    expect(screen.getByRole('menuitem', { name: 'Đổi mật khẩu' })).toHaveAttribute(
      'href',
      '/change-password',
    );
    expect(screen.getByRole('menuitem', { name: 'Đăng xuất' })).toBeInTheDocument();
  });

  it('hides Đổi mật khẩu for Instructor', async () => {
    // Arrange
    renderMenu('Instructor');

    // Act
    await userEvent.setup().click(screen.getByRole('button', { name: 'Menu tài khoản' }));

    // Assert
    expect(screen.queryByRole('menuitem', { name: 'Đổi mật khẩu' })).not.toBeInTheDocument();
    expect(screen.getByRole('menuitem', { name: 'Đăng xuất' })).toBeInTheDocument();
  });

  it('calls onSignOut and closes the menu when Đăng xuất is chosen', async () => {
    // Arrange
    const onSignOut = renderMenu('Admin');
    const user = userEvent.setup();
    await user.click(screen.getByRole('button', { name: 'Menu tài khoản' }));

    // Act
    await user.click(screen.getByRole('menuitem', { name: 'Đăng xuất' }));

    // Assert
    expect(onSignOut).toHaveBeenCalledOnce();
    expect(screen.queryByRole('menu')).not.toBeInTheDocument();
  });
});
