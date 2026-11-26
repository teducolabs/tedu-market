import { render } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { WelcomeToast } from './welcome-toast';

const replaceMock = vi.fn();
const showSuccessToastMock = vi.fn();
let currentSearch = '';

vi.mock('next/navigation', () => ({
  useRouter: () => ({ replace: replaceMock }),
  usePathname: () => '/courses',
  useSearchParams: () => new URLSearchParams(currentSearch),
}));
vi.mock('@tedu-market/ui', () => ({
  showSuccessToast: (message: string) => {
    showSuccessToastMock(message);
  },
}));

describe('WelcomeToast', () => {
  beforeEach(() => {
    replaceMock.mockClear();
    showSuccessToastMock.mockClear();
    currentSearch = '';
  });

  it('shows nothing without the welcome flag', () => {
    // Arrange / Act
    render(<WelcomeToast />);

    // Assert
    expect(showSuccessToastMock).not.toHaveBeenCalled();
    expect(replaceMock).not.toHaveBeenCalled();
  });

  it('shows the toast once and removes the flag from the url', () => {
    // Arrange
    currentSearch = 'search=react&welcome=1';

    // Act
    render(<WelcomeToast />);

    // Assert
    expect(showSuccessToastMock).toHaveBeenCalledTimes(1);
    expect(showSuccessToastMock).toHaveBeenCalledWith('Chào mừng bạn đến TEDU Market');
    expect(replaceMock).toHaveBeenCalledWith('/courses?search=react');
  });
});
