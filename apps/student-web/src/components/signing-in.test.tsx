import { render, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { SigningIn } from './signing-in';

describe('SigningIn', () => {
  const replaceMock = vi.fn();

  beforeEach(() => {
    replaceMock.mockClear();
    vi.stubGlobal('location', {
      search: '?code=abc&state=xyz',
      replace: replaceMock,
    });
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('shows the signing in message and forwards code and state once', () => {
    // Arrange / Act
    render(<SigningIn />);

    // Assert
    expect(screen.getByRole('status')).toHaveTextContent('Đang đăng nhập…');
    expect(replaceMock).toHaveBeenCalledTimes(1);
    expect(replaceMock).toHaveBeenCalledWith('/sign-in/complete?code=abc&state=xyz');
  });
});
