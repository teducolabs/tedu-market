import { expect, test, type Page } from '@playwright/test';
import { FAKE_API_URL, signInAs } from './admin-app-oidc';

const CORS_HEADERS = {
  'access-control-allow-origin': '*',
  'access-control-allow-headers': '*',
  'access-control-allow-methods': '*',
};
const PROFILE_ID = '00000000-0000-0000-0000-0000000000b1';

/** Chặn API Hồ sơ Instructor của mình; PUT ghi lại nội dung gửi lên, avatarUrl sai thì trả 400. */
async function mockProfileApi(page: Page): Promise<{ saved: unknown[] }> {
  const saved: unknown[] = [];
  await page.route(`${FAKE_API_URL}/api/v1/instructor-profiles/**`, async (route) => {
    const request = route.request();
    const { pathname } = new URL(request.url());
    if (request.method() === 'OPTIONS') {
      await route.fulfill({ status: 204, headers: CORS_HEADERS });
      return;
    }
    if (pathname === '/api/v1/instructor-profiles/me') {
      await route.fulfill({
        headers: CORS_HEADERS,
        json: {
          instructorProfileId: PROFILE_ID,
          displayName: null,
          biography: null,
          avatarUrl: null,
        },
      });
      return;
    }
    if (pathname === `/api/v1/instructor-profiles/${PROFILE_ID}` && request.method() === 'PUT') {
      const body = request.postDataJSON() as { avatarUrl?: string };
      if (body.avatarUrl === 'khong-hop-le') {
        await route.fulfill({
          status: 400,
          headers: CORS_HEADERS,
          json: { errors: { avatarUrl: ['Phải là đường dẫn http hoặc https hợp lệ'] } },
        });
        return;
      }
      saved.push(body);
      await route.fulfill({
        headers: CORS_HEADERS,
        json: { instructorProfileId: PROFILE_ID, ...body },
      });
      return;
    }
    await route.fulfill({ status: 404, headers: CORS_HEADERS });
  });
  return { saved };
}

test.describe('Luồng: Instructor sửa Hồ sơ Instructor của mình', () => {
  test('shows saved toast after Instructor saves own profile', async ({ page }) => {
    // Arrange
    await page.setViewportSize({ width: 1280, height: 800 });
    await signInAs(page, 'Instructor');
    const api = await mockProfileApi(page);
    await page.getByRole('link', { name: 'Hồ sơ Instructor' }).click();

    // Act
    await page.getByLabel(/Tên hiển thị/).fill('Cô Hoa');
    await page.getByRole('button', { name: 'Lưu' }).click();

    // Assert
    await expect(page.getByText('Đã lưu')).toBeVisible();
    expect(api.saved).toEqual([{ displayName: 'Cô Hoa', biography: '', avatarUrl: '' }]);
  });

  test('shows field error under the input when server rejects Ảnh đại diện', async ({ page }) => {
    // Arrange
    await page.setViewportSize({ width: 1280, height: 800 });
    await signInAs(page, 'Instructor');
    await mockProfileApi(page);
    await page.getByRole('link', { name: 'Hồ sơ Instructor' }).click();
    await page.getByLabel(/Tên hiển thị/).fill('Cô Hoa');

    // Act
    await page.getByLabel('Ảnh đại diện').fill('khong-hop-le');
    await page.getByRole('button', { name: 'Lưu' }).click();

    // Assert
    await expect(page.getByText('Phải là đường dẫn http hoặc https hợp lệ')).toBeVisible();
  });
});
