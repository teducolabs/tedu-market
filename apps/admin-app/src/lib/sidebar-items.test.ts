import { describe, expect, it } from 'vitest';
import { getSidebarItems } from './sidebar-items';

const labelsOf = (role: 'Admin' | 'Instructor') => getSidebarItems(role).map((item) => item.label);

describe('getSidebarItems', () => {
  it('returns all management items for Admin and no Hồ sơ Instructor', () => {
    // Arrange / Act
    const labels = labelsOf('Admin');

    // Assert
    expect(labels).toEqual([
      'Doanh thu',
      'Khóa học',
      'Duyệt khóa học',
      'Đơn',
      'Instructor',
      'Khoản chia chờ duyệt',
      'Phiếu chi',
      'Category',
      'Coupon',
      'Báo lỗi video',
    ]);
  });

  it('returns only Doanh thu, Khóa học and Hồ sơ Instructor for Instructor', () => {
    // Arrange / Act
    const labels = labelsOf('Instructor');

    // Assert
    expect(labels).toEqual(['Doanh thu', 'Khóa học', 'Hồ sơ Instructor']);
  });

  it('gives every item a unique path', () => {
    // Arrange / Act
    const paths = getSidebarItems('Admin')
      .concat(getSidebarItems('Instructor'))
      .map((i) => i.path);

    // Assert
    expect(new Set(paths).size).toBe(11);
  });
});
