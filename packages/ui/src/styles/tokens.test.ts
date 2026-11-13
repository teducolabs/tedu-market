import { describe, expect, it } from 'vitest';
import { contrastRatio } from '../lib/contrast';
import css from './tokens.css?inline';

function token(name: string): string {
  const match = new RegExp(`--color-${name}:\\s*(#[0-9a-f]{6})`, 'i').exec(css);
  if (!match?.[1]) throw new Error(`Thiếu token ${name}`);
  return match[1];
}

const TONES = ['primary', 'secondary', 'success', 'warning', 'error'] as const;

describe('design tokens', () => {
  it.each(TONES)('keeps %s-strong text on %s-tint at least 4.5:1', (tone) => {
    // Arrange
    const text = token(`${tone}-strong`);
    const background = token(`${tone}-tint`);

    // Act
    const ratio = contrastRatio(text, background);

    // Assert
    expect(ratio).toBeGreaterThanOrEqual(4.5);
  });

  it.each(['primary', 'success', 'warning', 'error'] as const)(
    'keeps white button text on %s at least 4.5:1',
    (tone) => {
      // Arrange
      const background = token(tone);

      // Act
      const ratio = contrastRatio('#ffffff', background);

      // Assert
      expect(ratio).toBeGreaterThanOrEqual(4.5);
    },
  );

  it.each(['text', 'text-muted'])('keeps %s on page background at least 4.5:1', (name) => {
    // Arrange
    const text = token(name);

    // Act
    const ratio = contrastRatio(text, token('bg'));

    // Assert
    expect(ratio).toBeGreaterThanOrEqual(4.5);
  });

  it('keeps input border at least 3:1 on card background', () => {
    // Arrange
    const border = token('border-input');

    // Act
    const ratio = contrastRatio(border, token('card'));

    // Assert
    expect(ratio).toBeGreaterThanOrEqual(3);
  });

  it('draws focus ring 2px in primary', () => {
    // Arrange
    const rule = /:focus-visible\s*\{[^}]*outline:\s*2px solid var\(--color-primary\)/;

    // Act
    const hasRule = rule.test(css);

    // Assert
    expect(hasRule).toBe(true);
  });
});
