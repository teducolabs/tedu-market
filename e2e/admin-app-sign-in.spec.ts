import { expect, test } from '@playwright/test';
import { mockAuthorizationServer, signInAs } from './admin-app-oidc';

test.describe('Luồng: Admin đăng nhập và đổi mật khẩu', () => {
  test('sends guests to sign in and returns them to the page they opened', async ({ page }) => {
    // Arrange
    await mockAuthorizationServer(page, { role: 'Admin' });

    // Act
    await page.goto('/orders');
    await page.getByRole('button', { name: 'Đăng nhập bằng email' }).click();

    // Assert
    await expect(page).toHaveURL(/\/orders$/);
    await expect(page.getByRole('heading', { name: 'Đơn' })).toBeVisible();
  });

  test('asks the authorization server for Code + PKCE without the Google provider for email sign in', async ({
    page,
  }) => {
    // Arrange
    const server = await mockAuthorizationServer(page, { role: 'Admin' });
    await page.goto('/');

    // Act
    await page.getByRole('button', { name: 'Đăng nhập bằng email' }).click();
    await page.waitForURL(/\/revenue$/);

    // Assert
    const authorize = server.requests.find((url) => url.pathname === '/connect/authorize');
    expect(authorize?.searchParams.get('client_id')).toBe('admin-app');
    expect(authorize?.searchParams.get('code_challenge_method')).toBe('S256');
    expect(authorize?.searchParams.get('identity_provider')).toBeNull();
  });

  test('lets Instructor sign in with Google', async ({ page }) => {
    // Arrange
    const server = await mockAuthorizationServer(page, { role: 'Instructor' });
    await page.goto('/');

    // Act
    await page.getByRole('button', { name: 'Đăng nhập bằng Google' }).click();
    await page.waitForURL(/\/revenue$/);

    // Assert
    const authorize = server.requests.find((url) => url.pathname === '/connect/authorize');
    expect(authorize?.searchParams.get('identity_provider')).toBe('google');
    await expect(page.getByRole('link', { name: 'Hồ sơ Instructor' })).toBeVisible();
  });

  test('shows the retry banner when sign in is cancelled', async ({ page }) => {
    // Arrange
    await mockAuthorizationServer(page, { role: 'Instructor', isCancelled: true });
    await page.goto('/');

    // Act
    await page.getByRole('button', { name: 'Đăng nhập bằng Google' }).click();

    // Assert
    await expect(page.getByRole('alert')).toHaveText(/Đăng nhập chưa hoàn tất. Bạn thử lại nhé/);
    await expect(page.getByRole('button', { name: 'Đăng nhập bằng Google' })).toBeVisible();
  });

  test('shows no permission for a Student account', async ({ page }) => {
    // Arrange / Act
    await mockAuthorizationServer(page, { role: 'Student' });
    await page.goto('/');
    await page.getByRole('button', { name: 'Đăng nhập bằng Google' }).click();

    // Assert
    await expect(page.getByText('Tài khoản này không có quyền vào App quản trị.')).toBeVisible();
  });

  test('offers Đổi mật khẩu to Admin only', async ({ page }) => {
    // Arrange
    await signInAs(page, 'Admin');

    // Act
    await page.getByRole('button', { name: 'Menu tài khoản' }).click();

    // Assert
    await expect(page.getByRole('menuitem', { name: 'Đổi mật khẩu' })).toBeVisible();
  });

  test('hides Đổi mật khẩu from Instructor', async ({ page }) => {
    // Arrange
    await signInAs(page, 'Instructor');

    // Act
    await page.getByRole('button', { name: 'Menu tài khoản' }).click();

    // Assert
    await expect(page.getByRole('menuitem', { name: 'Đăng xuất' })).toBeVisible();
    await expect(page.getByRole('menuitem', { name: 'Đổi mật khẩu' })).toHaveCount(0);
  });

  test('signs out through the end session endpoint and returns to sign in', async ({ page }) => {
    // Arrange
    const server = await signInAs(page, 'Admin');
    await page.getByRole('button', { name: 'Menu tài khoản' }).click();

    // Act
    await page.getByRole('menuitem', { name: 'Đăng xuất' }).click();

    // Assert
    await expect(page.getByRole('button', { name: 'Đăng nhập bằng email' })).toBeVisible();
    const logout = server.requests.find((url) => url.pathname === '/connect/logout');
    expect(logout?.searchParams.get('id_token_hint')).not.toBeNull();
  });

  test('shows the server error under the field when the current password is wrong', async ({
    page,
  }) => {
    // Arrange
    const server = await signInAs(page, 'Admin');
    server.setChangePasswordResponse(400, {
      errorCode: 'VALIDATION_FAILED',
      errors: { currentPassword: ['Mật khẩu hiện tại chưa đúng'] },
    });
    await page.getByRole('button', { name: 'Menu tài khoản' }).click();
    await page.getByRole('menuitem', { name: 'Đổi mật khẩu' }).click();
    await page.getByLabel(/Mật khẩu hiện tại/).fill('sai-mat-khau');
    await page.getByLabel(/Mật khẩu mới/).fill('Mat-khau-moi-2@');

    // Act
    await page.getByRole('button', { name: 'Lưu' }).click();

    // Assert
    await expect(page.getByText('Mật khẩu hiện tại chưa đúng')).toBeVisible();
  });

  test('signs the Admin out after the password is changed', async ({ page }) => {
    // Arrange
    const server = await signInAs(page, 'Admin');
    server.setChangePasswordResponse(204);
    await page.getByRole('button', { name: 'Menu tài khoản' }).click();
    await page.getByRole('menuitem', { name: 'Đổi mật khẩu' }).click();
    await page.getByLabel(/Mật khẩu hiện tại/).fill('Mat-khau-cu-1!');
    await page.getByLabel(/Mật khẩu mới/).fill('Mat-khau-moi-2@');

    // Act
    await page.getByRole('button', { name: 'Lưu' }).click();

    // Assert
    await expect(page.getByText('Đã đổi mật khẩu. Bạn đăng nhập lại nhé')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Đăng nhập bằng email' })).toBeVisible();
    expect(server.requests.some((url) => url.pathname === '/connect/logout')).toBe(true);
  });
});
