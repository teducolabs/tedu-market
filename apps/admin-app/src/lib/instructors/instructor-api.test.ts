import * as apiClient from '@tedu-market/api-client';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import {
  approveInstructor,
  deleteInstructorProfile,
  getInstructorProfile,
  getMyInstructorProfile,
  listUsers,
  updateInstructorProfile,
} from './instructor-api';

vi.mock('@tedu-market/api-client', () => ({
  getApiV1Users: vi.fn(),
  postApiV1UsersByUserIdInstructorApproval: vi.fn(),
  getApiV1InstructorProfilesByInstructorProfileId: vi.fn(),
  getApiV1InstructorProfilesMe: vi.fn(),
  putApiV1InstructorProfilesByInstructorProfileId: vi.fn(),
  deleteApiV1InstructorProfilesByInstructorProfileId: vi.fn(),
}));

const ok = { ok: true, status: 200 };
const asResult = (value: unknown) => value as never;

describe('instructor-api', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('returns parsed page and sends role filter when listing users', async () => {
    // Arrange
    vi.mocked(apiClient.getApiV1Users).mockResolvedValue(
      asResult({
        response: ok,
        data: {
          items: [
            {
              userId: 'u1',
              email: 'a@x.vn',
              roles: ['Student'],
              instructorProfileId: null,
              displayName: null,
            },
          ],
          pageNumber: '2',
          pageSize: 20,
          totalCount: 21,
        },
      }),
    );

    // Act
    const result = await listUsers('Student', 2);

    // Assert
    expect(apiClient.getApiV1Users).toHaveBeenCalledWith({
      query: { pageNumber: 2, pageSize: 20, role: 'Student' },
    });
    expect(result).toEqual({
      isSuccess: true,
      value: {
        items: [
          {
            userId: 'u1',
            email: 'a@x.vn',
            roles: ['Student'],
            instructorProfileId: null,
            displayName: null,
          },
        ],
        pageNumber: 2,
        pageSize: 20,
        totalCount: 21,
      },
    });
  });

  it('fails when list response is not ok or has no data', async () => {
    // Arrange
    vi.mocked(apiClient.getApiV1Users).mockResolvedValueOnce(
      asResult({ response: { ok: false, status: 500 } }),
    );
    vi.mocked(apiClient.getApiV1Users).mockResolvedValueOnce(asResult({ response: ok }));

    // Act
    const failed = await listUsers(null, 1);
    const missingData = await listUsers(null, 1);

    // Assert
    expect(failed).toEqual({ isSuccess: false });
    expect(missingData).toEqual({ isSuccess: false });
  });

  it('reports success only when approval response is ok', async () => {
    // Arrange
    vi.mocked(apiClient.postApiV1UsersByUserIdInstructorApproval).mockResolvedValueOnce(
      asResult({ response: ok }),
    );
    vi.mocked(apiClient.postApiV1UsersByUserIdInstructorApproval).mockResolvedValueOnce(
      asResult({ response: { ok: false, status: 409 } }),
    );

    // Act
    const approved = await approveInstructor('u1');
    const rejected = await approveInstructor('u1');

    // Assert
    expect(approved).toEqual({ isSuccess: true });
    expect(rejected).toEqual({ isSuccess: false });
  });

  it('turns null profile fields into empty strings when loading profile', async () => {
    // Arrange
    vi.mocked(apiClient.getApiV1InstructorProfilesByInstructorProfileId).mockResolvedValue(
      asResult({
        response: ok,
        data: {
          instructorProfileId: 'p1',
          userId: 'u1',
          displayName: null,
          biography: 'Hi',
          avatarUrl: null,
        },
      }),
    );

    // Act
    const result = await getInstructorProfile('p1');

    // Assert
    expect(result).toEqual({
      isSuccess: true,
      value: { instructorProfileId: 'p1', displayName: '', biography: 'Hi', avatarUrl: '' },
    });
  });

  it('maps 400 errors to fields when updating profile', async () => {
    // Arrange
    vi.mocked(apiClient.putApiV1InstructorProfilesByInstructorProfileId).mockResolvedValue(
      asResult({
        response: { ok: false, status: 400 },
        error: { errors: { displayName: ['Không được để trống'], avatarUrl: ['Sai Url'] } },
      }),
    );

    // Act
    const result = await updateInstructorProfile('p1', {
      displayName: '',
      biography: '',
      avatarUrl: 'x',
    });

    // Assert
    expect(result).toEqual({
      isSuccess: false,
      fieldErrors: { displayName: 'Không được để trống', avatarUrl: 'Sai Url' },
      message: null,
    });
  });

  it('returns generic message when update fails without field errors', async () => {
    // Arrange
    vi.mocked(apiClient.putApiV1InstructorProfilesByInstructorProfileId).mockResolvedValue(
      asResult({ response: { ok: false, status: 500 }, error: {} }),
    );

    // Act
    const result = await updateInstructorProfile('p1', {
      displayName: 'A',
      biography: '',
      avatarUrl: '',
    });

    // Assert
    expect(result).toEqual({
      isSuccess: false,
      fieldErrors: {},
      message: 'Có sự cố khi lưu hồ sơ. Bạn thử lại sau ít phút.',
    });
  });

  it('reports delete result from response status', async () => {
    // Arrange
    vi.mocked(apiClient.deleteApiV1InstructorProfilesByInstructorProfileId).mockResolvedValue(
      asResult({ response: { ok: true, status: 204 } }),
    );

    // Act
    const result = await deleteInstructorProfile('p1');

    // Assert
    expect(result).toEqual({ isSuccess: true });
  });

  it('maps own profile with empty fields when loading /me succeeds', async () => {
    // Arrange
    vi.mocked(apiClient.getApiV1InstructorProfilesMe).mockResolvedValue(
      asResult({
        response: ok,
        data: { instructorProfileId: 'p1', displayName: null, biography: null, avatarUrl: null },
      }),
    );

    // Act
    const result = await getMyInstructorProfile();

    // Assert
    expect(result).toEqual({
      isSuccess: true,
      value: { instructorProfileId: 'p1', displayName: '', biography: '', avatarUrl: '' },
    });
  });

  it('returns failure when loading /me is rejected', async () => {
    // Arrange
    vi.mocked(apiClient.getApiV1InstructorProfilesMe).mockResolvedValue(
      asResult({ response: { ok: false, status: 403 } }),
    );

    // Act
    const result = await getMyInstructorProfile();

    // Assert
    expect(result).toEqual({ isSuccess: false });
  });
});
