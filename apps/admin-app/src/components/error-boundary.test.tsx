import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { ErrorBoundary } from './error-boundary';

function Broken(): never {
  throw new Error('boom');
}

describe('ErrorBoundary', () => {
  it('shows generic message without exception details when a child throws', () => {
    // Arrange
    vi.spyOn(console, 'error').mockImplementation(() => undefined);

    // Act
    render(
      <ErrorBoundary>
        <Broken />
      </ErrorBoundary>,
    );

    // Assert
    expect(screen.getByRole('alert')).toHaveTextContent('Có lỗi xảy ra, vui lòng thử lại.');
    expect(screen.queryByText(/boom/)).not.toBeInTheDocument();
  });
});
