import { expect, test, type Page } from '@playwright/test';
import { FAKE_API_URL, signInAs } from './admin-app-oidc';

const CORS_HEADERS = {
  'access-control-allow-origin': '*',
  'access-control-allow-headers': '*',
  'access-control-allow-methods': '*',
};
const STUDENT_ID = '00000000-0000-0000-0000-0000000000a1';
const PROFILE_ID = '00000000-0000-0000-0000-0000000000b1';

/** Chặn API người dùng và Hồ sơ Instructor; duyệt xong thì danh sách trả thêm Hồ sơ Instructor mới. */
async function mockInstructorApi(page: Page): Promise<{ approvedUserIds: string[] }> {
  const approvedUserIds: string[] = [];
  await page.route(`${FAKE_API_URL}/api/v1/**`, async (route) => {
    const request = route.request();
    const { pathname } = new URL(request.url());
    if (request.method() === 'OPTIONS') {
      await route.fulfill({ status: 204, headers: CORS_HEADERS });
      return;
    }
    if (pathname === '/api/v1/users' && request.method() === 'GET') {
      const isApproved = approvedUserIds.includes(STUDENT_ID);
      await route.fulfill({
        headers: CORS_HEADERS,
        json: {
          items: [
            {
              userId: STUDENT_ID,
              email: 'hocvien@tedu.test',
              roles: isApproved ? ['Student', 'Instructor'] : ['Student'],
              instructorProfileId: isApproved ? PROFILE_ID : null,
            },
          ],
          pageNumber: 1,
          pageSize: 20,
          totalCount: 1,
        },
      });
      return;
    }
    if (pathname === `/api/v1/users/${STUDENT_ID}/instructor-approval`) {
      approvedUserIds.push(STUDENT_ID);
      await route.fulfill({
        headers: CORS_HEADERS,
        json: { userId: STUDENT_ID, instructorProfileId: PROFILE_ID },
      });
      return;
    }
    await route.fulfill({ status: 404, headers: CORS_HEADERS });
  });
  return { approvedUserIds };
}

test.describe('Luồng: Admin Duyệt Instructor', () => {
  test('shows new Hồ sơ Instructor after Admin approves a Học viên', async ({ page }) => {
    // Arrange
    await page.setViewportSize({ width: 1280, height: 800 });
    await signInAs(page, 'Admin');
    const api = await mockInstructorApi(page);
    await page.getByRole('link', { name: 'Instructor', exact: true }).click();
    await expect(page.getByText('hocvien@tedu.test')).toBeVisible();

    // Act
    await page.getByRole('button', { name: 'Duyệt Instructor' }).click();
    await page.getByRole('dialog').getByRole('button', { name: 'Duyệt Instructor' }).click();

    // Assert
    await expect(page.getByText('Đã duyệt Instructor')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Sửa hồ sơ hocvien@tedu.test' })).toBeVisible();
    expect(api.approvedUserIds).toEqual([STUDENT_ID]);
  });

  test('shows no permission for Instructor opening the Instructor screen', async ({ page }) => {
    // Arrange
    await page.setViewportSize({ width: 1280, height: 800 });
    await signInAs(page, 'Instructor');

    // Act
    await page.goto('/instructors');

    // Assert
    await expect(page.getByText('Bạn không có quyền vào mục này.')).toBeVisible();
  });
});
