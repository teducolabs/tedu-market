import { describe, expect, it, vi } from 'vitest';

import { client, getApiV1SamplesNotes } from './index';

describe('api-client', () => {
  it('calls GET /api/v1/samples/notes on configured base url when using generated operation', async () => {
    // Arrange
    const fetchMock = vi.fn(() =>
      Promise.resolve(
        new Response(JSON.stringify([]), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      ),
    );
    client.setConfig({ baseUrl: 'https://api.test', fetch: fetchMock });

    // Act
    const result = await getApiV1SamplesNotes();

    // Assert
    const request = (fetchMock.mock.calls[0] as unknown as [Request])[0];
    expect(request.method).toBe('GET');
    expect(request.url).toBe('https://api.test/api/v1/samples/notes');
    expect(result.data).toEqual([]);
  });

  it('returns typed error without throwing when server answers 400', async () => {
    // Arrange
    const problem = { title: 'Dữ liệu không hợp lệ', status: 400 };
    client.setConfig({
      baseUrl: 'https://api.test',
      fetch: () =>
        Promise.resolve(
          new Response(JSON.stringify(problem), {
            status: 400,
            headers: { 'Content-Type': 'application/json' },
          }),
        ),
    });

    // Act
    const result = await getApiV1SamplesNotes();

    // Assert
    expect(result.error).toEqual(problem);
    expect(result.data).toBeUndefined();
  });
});
