import { expect, test, type Page } from '@playwright/test';

const FAKE_API_PATTERN = 'http://127.0.0.1:5999/**';

/** Chặn API giả: trình duyệt không rời khỏi test, chỉ ghi lại URL authorize. */
async function captureAuthorizeUrl(page: Page): Promise<() => URL> {
  let authorizeUrl: URL | null = null;
  await page.route(FAKE_API_PATTERN, async (route) => {
    authorizeUrl = new URL(route.request().url());
    await route.fulfill({ status: 200, contentType: 'text/html', body: '<p>Google</p>' });
  });
  return () => {
    if (authorizeUrl === null) {
      throw new Error('Chưa có request tới /connect/authorize');
    }
    return authorizeUrl;
  };
}

test.describe('Luồng: Đăng nhập bằng Google (Học viên)', () => {
  test('sends guests opening Giỏ hàng to Google sign in', async ({ page }) => {
    // Arrange
    const getAuthorizeUrl = await captureAuthorizeUrl(page);

    // Act
    await page.goto('/cart');

    // Assert
    const url = getAuthorizeUrl();
    expect(url.pathname).toBe('/connect/authorize');
    expect(url.searchParams.get('identity_provider')).toBe('google');
    expect(url.searchParams.get('code_challenge_method')).toBe('S256');
    expect(url.searchParams.get('client_id')).toBe('student-web');
  });

  test('starts sign in from the header button', async ({ page }) => {
    // Arrange
    const getAuthorizeUrl = await captureAuthorizeUrl(page);
    await page.goto('/');

    // Act
    await page.getByRole('button', { name: 'Đăng nhập bằng Google' }).click();

    // Assert
    await expect.poll(() => getAuthorizeUrl().pathname).toBe('/connect/authorize');
  });

  test('shows retry banner on the previous page when the student cancels at Google', async ({
    page,
  }) => {
    // Arrange
    const getAuthorizeUrl = await captureAuthorizeUrl(page);
    await page.goto('/sign-in?returnTo=%2F');
    const state = getAuthorizeUrl().searchParams.get('state') ?? '';

    // Act
    await page.goto(`/sign-in/callback?error=access_denied&state=${state}`);

    // Assert
    await expect(page).toHaveURL(/\/\?signIn=failed$/);
    await expect(page.getByRole('alert')).toHaveText(/Đăng nhập chưa hoàn tất. Bạn thử lại nhé/);
    await expect(page.getByRole('button', { name: 'Đăng nhập bằng Google' })).toHaveCount(2);
  });

  test('does not follow a return path pointing to another site', async ({ page }) => {
    // Arrange
    const getAuthorizeUrl = await captureAuthorizeUrl(page);
    await page.goto('/sign-in?returnTo=https%3A%2F%2Fevil.example%2F');
    const state = getAuthorizeUrl().searchParams.get('state') ?? '';

    // Act
    await page.goto(`/sign-in/callback?error=access_denied&state=${state}`);

    // Assert
    await expect(page).toHaveURL(/localhost:3100\/\?signIn=failed$/);
  });
});
