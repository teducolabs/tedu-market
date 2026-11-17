import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { App } from './app';

describe('App', () => {
  it('redirects to Doanh thu and shows the admin sidebar by default', () => {
    // Arrange / Act
    render(<App />);

    // Assert
    expect(screen.getByRole('heading', { name: 'Doanh thu' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Coupon' })).toBeInTheDocument();
  });
});
