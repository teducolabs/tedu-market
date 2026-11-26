import { expect, test } from '@playwright/test';
import { signInAs } from './admin-app-oidc';

const ADMIN_ONLY_ITEMS = ['Duyệt khóa học', 'Đơn', 'Phiếu chi', 'Category', 'Coupon'] as const;

test.describe('Khung App quản trị', () => {
  test('shows admin sidebar items for Admin at 1280px', async ({ page }) => {
    // Arrange
    await page.setViewportSize({ width: 1280, height: 800 });

    // Act
    await signInAs(page, 'Admin');

    // Assert
    for (const label of ADMIN_ONLY_ITEMS) {
      await expect(page.getByRole('link', { name: label, exact: true })).toBeVisible();
    }
    await expect(page.getByRole('link', { name: 'Hồ sơ Instructor' })).toBeHidden();
    await expect(page.getByText('Vui lòng dùng máy tính', { exact: false })).toBeHidden();
  });

  test('shows only own sidebar items for Instructor', async ({ page }) => {
    // Arrange
    await page.setViewportSize({ width: 1280, height: 800 });

    // Act
    await signInAs(page, 'Instructor');

    // Assert
    await expect(page.getByRole('link', { name: 'Hồ sơ Instructor' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Doanh thu' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Duyệt khóa học' })).toBeHidden();
  });

  test('collapses sidebar to icons at 768px and expands on click', async ({ page }) => {
    // Arrange
    await page.setViewportSize({ width: 768, height: 800 });
    await signInAs(page, 'Admin');
    await expect(page.getByText('Duyệt khóa học', { exact: true })).toBeHidden();

    // Act
    await page.getByRole('button', { name: 'Mở rộng thanh bên' }).click();

    // Assert
    await expect(page.getByText('Duyệt khóa học', { exact: true })).toBeVisible();
  });

  test('shows desktop-only notice with Đăng xuất below 640px and hides the sidebar', async ({
    page,
  }) => {
    // Arrange
    await page.setViewportSize({ width: 639, height: 800 });

    // Act
    await signInAs(page, 'Admin');

    // Assert
    await expect(
      page.getByText('Vui lòng dùng máy tính hoặc máy tính bảng để quản trị'),
    ).toBeVisible();
    await expect(page.getByRole('button', { name: 'Đăng xuất' })).toBeVisible();
    await expect(page.getByRole('navigation', { name: 'Điều hướng quản trị' })).toBeHidden();
  });
});
