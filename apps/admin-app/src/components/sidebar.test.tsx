import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { describe, expect, it, vi } from 'vitest';
import { Sidebar } from './sidebar';

const renderSidebar = (role: 'Admin' | 'Instructor') =>
  render(
    <MemoryRouter initialEntries={['/orders']}>
      <Sidebar role={role} userName="Lan" onSignOut={vi.fn()} />
    </MemoryRouter>,
  );

describe('Sidebar', () => {
  it('shows admin items and hides Hồ sơ Instructor for Admin', () => {
    // Arrange / Act
    renderSidebar('Admin');

    // Assert
    expect(screen.getByRole('link', { name: 'Duyệt khóa học' })).toHaveAttribute(
      'href',
      '/course-approvals',
    );
    expect(screen.getByRole('link', { name: 'Phiếu chi' })).toBeInTheDocument();
    expect(screen.queryByRole('link', { name: 'Hồ sơ Instructor' })).not.toBeInTheDocument();
  });

  it('shows only own items for Instructor', () => {
    // Arrange / Act
    renderSidebar('Instructor');

    // Assert
    expect(screen.getAllByRole('link').map((link) => link.getAttribute('aria-label'))).toEqual([
      'Doanh thu',
      'Khóa học',
      'Hồ sơ Instructor',
    ]);
  });

  it('marks the link of the current route as current page', () => {
    // Arrange / Act
    renderSidebar('Admin');

    // Assert
    expect(screen.getByRole('link', { name: 'Đơn' })).toHaveAttribute('aria-current', 'page');
  });

  it('toggles expanded state when the toggle button is clicked', async () => {
    // Arrange
    const user = userEvent.setup();
    renderSidebar('Admin');

    // Act
    await user.click(screen.getByRole('button', { name: 'Mở rộng thanh bên' }));

    // Assert
    expect(screen.getByRole('button', { name: 'Thu gọn thanh bên' })).toHaveAttribute(
      'aria-expanded',
      'true',
    );
  });
});
