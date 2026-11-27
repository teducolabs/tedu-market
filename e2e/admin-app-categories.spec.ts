import { expect, test, type Page } from '@playwright/test';
import { FAKE_API_URL, signInAs } from './admin-app-oidc';

const CORS_HEADERS = {
  'access-control-allow-origin': '*',
  'access-control-allow-headers': '*',
  'access-control-allow-methods': '*',
};

interface FakeCategory {
  categoryId: string;
  name: string;
}

/** Chặn API Category: giữ danh sách trong bộ nhớ, tạo/sửa/xóa xong thì danh sách đổi ngay. */
async function mockCategoryApi(page: Page): Promise<FakeCategory[]> {
  const categories: FakeCategory[] = [{ categoryId: 'c-1', name: 'Lập trình' }];
  await page.route(`${FAKE_API_URL}/api/v1/categories**`, async (route) => {
    const request = route.request();
    const { pathname } = new URL(request.url());
    const categoryId = pathname.split('/')[4];
    if (request.method() === 'OPTIONS') {
      await route.fulfill({ status: 204, headers: CORS_HEADERS });
      return;
    }
    if (request.method() === 'GET') {
      await route.fulfill({ headers: CORS_HEADERS, json: categories });
      return;
    }
    if (request.method() === 'POST') {
      const body = request.postDataJSON() as { name: string };
      const created = { categoryId: `c-${String(categories.length + 1)}`, name: body.name };
      categories.push(created);
      await route.fulfill({ status: 201, headers: CORS_HEADERS, json: created });
      return;
    }
    const index = categories.findIndex((category) => category.categoryId === categoryId);
    if (request.method() === 'PUT') {
      const body = request.postDataJSON() as { name: string };
      categories[index] = { categoryId: categoryId ?? '', name: body.name };
      await route.fulfill({ headers: CORS_HEADERS, json: categories[index] });
      return;
    }
    categories.splice(index, 1);
    await route.fulfill({ status: 204, headers: CORS_HEADERS });
  });
  return categories;
}

test.describe('Luồng: Admin quản lý Category', () => {
  test('shows created, renamed and deleted Category right away in the table', async ({ page }) => {
    // Arrange
    await page.setViewportSize({ width: 1280, height: 800 });
    await signInAs(page, 'Admin');
    await mockCategoryApi(page);
    await page.getByRole('link', { name: 'Category', exact: true }).click();
    await expect(page.getByText('Lập trình', { exact: true })).toBeVisible();

    // Act
    await page.getByRole('button', { name: 'Tạo Category' }).click();
    await page.getByLabel(/Tên/).fill('Thiết kế');
    await page.getByRole('button', { name: 'Lưu' }).click();
    await expect(page.getByText('Thiết kế', { exact: true })).toBeVisible();
    await page.getByRole('button', { name: 'Sửa Category Thiết kế' }).click();
    await page.getByLabel(/Tên/).fill('Thiết kế đồ họa');
    await page.getByRole('button', { name: 'Lưu' }).click();
    await expect(page.getByText('Thiết kế đồ họa', { exact: true })).toBeVisible();
    await page.getByRole('button', { name: 'Xóa Category Lập trình' }).click();
    await page.getByRole('dialog').getByRole('button', { name: 'Xóa Category' }).click();

    // Assert
    await expect(page.getByText('Đã xóa Category')).toBeVisible();
    await expect(page.getByText('Lập trình', { exact: true })).toBeHidden();
    await expect(page.getByText('Thiết kế đồ họa', { exact: true })).toBeVisible();
  });

  test('shows no permission for Instructor opening the Category screen', async ({ page }) => {
    // Arrange
    await page.setViewportSize({ width: 1280, height: 800 });
    await signInAs(page, 'Instructor');

    // Act: đổi URL trong app (không tải lại trang) vì token chỉ giữ trong bộ nhớ
    await page.evaluate(() => {
      window.history.pushState({}, '', '/categories');
      window.dispatchEvent(new PopStateEvent('popstate'));
    });

    // Assert
    await expect(page.getByText('Bạn không có quyền vào mục này.')).toBeVisible();
  });
});
