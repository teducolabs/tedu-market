import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import HomePage, { metadata } from './page';

describe('HomePage', () => {
  it('renders course list heading and has metadata', () => {
    // Arrange / Act
    render(<HomePage />);

    // Assert
    expect(
      screen.getByRole('heading', { level: 1, name: 'Danh sách khóa học' }),
    ).toBeInTheDocument();
    expect(metadata.title).toEqual({ absolute: 'Danh sách khóa học | TEDU Market' });
    expect(metadata.description).toBeTruthy();
  });
});
