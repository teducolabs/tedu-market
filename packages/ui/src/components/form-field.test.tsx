import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { FormField, Input } from './form-field';

describe('FormField', () => {
  it('links the label to the input and writes "(bắt buộc)" for required fields', () => {
    // Arrange / Act
    render(
      <FormField label="Coupon" isRequired>
        {(control) => <Input {...control} />}
      </FormField>,
    );

    // Assert
    expect(screen.getByLabelText(/Coupon/)).toBeRequired();
    expect(screen.getByText('(bắt buộc)')).toBeInTheDocument();
  });

  it('shows the error under the input and wires it with aria-describedby', () => {
    // Arrange / Act
    render(
      <FormField label="Coupon" error="Coupon đã hết hạn.">
        {(control) => <Input {...control} />}
      </FormField>,
    );

    // Assert
    const input = screen.getByLabelText('Coupon');
    expect(input).toHaveAttribute('aria-invalid', 'true');
    expect(input).toHaveAccessibleDescription('Coupon đã hết hạn.');
  });

  it('is valid without describedby when there is no error or hint', () => {
    // Arrange / Act
    render(<FormField label="Tên">{(control) => <Input {...control} />}</FormField>);

    // Assert
    const input = screen.getByLabelText('Tên');
    expect(input).toHaveAttribute('aria-invalid', 'false');
    expect(input).not.toHaveAttribute('aria-describedby');
  });
});
