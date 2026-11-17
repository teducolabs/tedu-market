import { expect, test } from '@playwright/test';

/** Breakpoint theo ux.md mục 4 và 8: dưới sm là mobile, sau đó sm 640, md 768, lg 1024, xl 1280. */
const FULL_HEADER_VIEWPORTS = [
  { name: 'sm', width: 640, isLabelVisible: false },
  { name: 'md', width: 768, isLabelVisible: false },
  { name: 'lg', width: 1024, isLabelVisible: true },
  { name: 'xl', width: 1280, isLabelVisible: true },
] as const;

test.describe('Header Web học viên', () => {
  for (const viewport of FULL_HEADER_VIEWPORTS) {
    test(`shows full header on one row at ${viewport.name} (${String(viewport.width)}px)`, async ({
      page,
    }) => {
      // Arrange
      await page.setViewportSize({ width: viewport.width, height: 800 });

      // Act
      await page.goto('/');

      // Assert
      await expect(page.getByRole('link', { name: 'TEDU Market' })).toBeVisible();
      await expect(page.getByRole('searchbox', { name: 'Tìm khóa học' })).toBeVisible();
      await expect(page.getByRole('link', { name: 'Giỏ hàng' })).toBeVisible();
      await expect(page.getByRole('link', { name: 'Khóa học của tôi' })).toBeVisible();
      await expect(page.getByRole('button', { name: 'Đăng nhập bằng Google' })).toBeVisible();
      await expect(page.getByRole('button', { name: 'Mở menu' })).toBeHidden();
      await expect(page.getByText('Khóa học của tôi', { exact: true })).toBeVisible({
        visible: viewport.isLabelVisible,
      });
      const hasHorizontalScroll = await page.evaluate(
        () => document.documentElement.scrollWidth > document.documentElement.clientWidth,
      );
      expect(hasHorizontalScroll).toBe(false);
    });
  }

  test('collapses into a menu below sm (375px) and opens it on click', async ({ page }) => {
    // Arrange
    await page.setViewportSize({ width: 375, height: 800 });
    await page.goto('/');
    await expect(page.getByRole('searchbox', { name: 'Tìm khóa học' })).toBeHidden();

    // Act
    await page.getByRole('button', { name: 'Mở menu' }).click();

    // Assert
    await expect(page.getByRole('searchbox', { name: 'Tìm khóa học' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Khóa học của tôi' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Đăng nhập bằng Google' })).toBeVisible();
  });

  test('renders sample page on the server with metadata', async ({ request }) => {
    // Arrange / Act
    const response = await request.get('/');
    const html = await response.text();

    // Assert
    expect(response.ok()).toBe(true);
    expect(html).toContain('<title>Danh sách khóa học | TEDU Market</title>');
    expect(html).toContain('name="description"');
    expect(html).toContain('<h1');
    expect(html).toContain('lang="vi"');
  });
});
