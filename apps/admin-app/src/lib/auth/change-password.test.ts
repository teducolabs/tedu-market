import { postApiV1AccountChangePassword } from '@tedu-market/api-client';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { changePassword } from './change-password';

vi.mock('@tedu-market/api-client', () => ({ postApiV1AccountChangePassword: vi.fn() }));

const mockedPost = vi.mocked(postApiV1AccountChangePassword);

const respond = (status: number, error?: unknown) =>
  mockedPost.mockResolvedValue({
    response: new Response(null, { status }),
    error,
  } as never);

describe('changePassword', () => {
  beforeEach(() => {
    mockedPost.mockReset();
  });

  it('sends both passwords and returns success', async () => {
    // Arrange
    respond(200);

    // Act
    const result = await changePassword('old-pass', 'new-pass');

    // Assert
    expect(result).toEqual({ isSuccess: true });
    expect(mockedPost).toHaveBeenCalledWith({
      body: { currentPassword: 'old-pass', newPassword: 'new-pass' },
    });
  });

  it('maps server 400 errors to the matching fields', async () => {
    // Arrange
    respond(400, { errors: { currentPassword: ['Mật khẩu hiện tại chưa đúng'] } });

    // Act
    const result = await changePassword('wrong', 'new-pass');

    // Assert
    expect(result).toEqual({
      isSuccess: false,
      fieldErrors: { currentPassword: 'Mật khẩu hiện tại chưa đúng' },
      message: null,
    });
  });

  it('maps new password errors to the new password field', async () => {
    // Arrange
    respond(400, { errors: { newPassword: ['Mật khẩu cần có ít nhất 1 chữ số'] } });

    // Act
    const result = await changePassword('old-pass', 'abc');

    // Assert
    expect(result).toEqual({
      isSuccess: false,
      fieldErrors: { newPassword: 'Mật khẩu cần có ít nhất 1 chữ số' },
      message: null,
    });
  });

  it.each([[500], [403]])('returns a generic message for status %i', async (status) => {
    // Arrange
    respond(status, { errorCode: 'X' });

    // Act
    const result = await changePassword('old-pass', 'new-pass');

    // Assert
    expect(result).toEqual({
      isSuccess: false,
      fieldErrors: {},
      message: 'Có sự cố khi đổi mật khẩu. Bạn thử lại sau ít phút.',
    });
  });

  it('returns a generic message when the network fails', async () => {
    // Arrange
    mockedPost.mockResolvedValue({ response: undefined, error: new Error('down') } as never);

    // Act
    const result = await changePassword('old-pass', 'new-pass');

    // Assert
    expect(result).toMatchObject({ isSuccess: false, fieldErrors: {} });
  });
});
