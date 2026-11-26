import { describe, expect, it } from 'vitest';
import { resolveAdminAppRole } from './user-role';

describe('resolveAdminAppRole', () => {
  it.each([
    ['Admin', 'Admin'],
    [['Admin'], 'Admin'],
    [['Instructor', 'Student'], 'Instructor'],
    [['Student', 'Admin', 'Instructor'], 'Admin'],
  ])('returns the app role for claim %j', (claim, expected) => {
    // Arrange / Act
    const role = resolveAdminAppRole(claim);

    // Assert
    expect(role).toBe(expected);
  });

  it.each([[undefined], [['Student']], ['Student'], [[]], [42]])(
    'returns null when claim %j has no admin app role',
    (claim) => {
      // Arrange / Act
      const role = resolveAdminAppRole(claim);

      // Assert
      expect(role).toBeNull();
    },
  );
});
