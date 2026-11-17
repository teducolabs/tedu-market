import { describe, expect, it } from 'vitest';
import { resolveFakeRole } from './fake-session';

describe('resolveFakeRole', () => {
  it('returns Admin when no role is requested', () => {
    // Arrange / Act
    const role = resolveFakeRole('', 'development');

    // Assert
    expect(role).toBe('Admin');
  });

  it('returns Instructor when requested outside production', () => {
    // Arrange / Act
    const role = resolveFakeRole('?role=Instructor', 'e2e');

    // Assert
    expect(role).toBe('Instructor');
  });

  it('falls back to Admin when role is unknown', () => {
    // Arrange / Act
    const role = resolveFakeRole('?role=Student', 'development');

    // Assert
    expect(role).toBe('Admin');
  });

  it('ignores requested role in production', () => {
    // Arrange / Act
    const role = resolveFakeRole('?role=Instructor', 'production');

    // Assert
    expect(role).toBe('Admin');
  });
});
