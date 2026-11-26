import { client } from '@tedu-market/api-client';
import { describe, expect, it, vi } from 'vitest';
import { configureApiClient } from './api-client';

describe('configureApiClient', () => {
  it('sends the in-memory access token as Bearer header to the configured api', async () => {
    // Arrange
    const fetchMock = vi.fn().mockResolvedValue(new Response('{}', { status: 200 }));
    configureApiClient('http://api.test', () => Promise.resolve('abc'));
    client.setConfig({ fetch: fetchMock });

    // Act
    await client.get({ url: '/api/v1/ping' });

    // Assert
    const request = fetchMock.mock.calls[0]?.[0] as Request;
    expect(request.url).toBe('http://api.test/api/v1/ping');
    expect(request.headers.get('Authorization')).toBe('Bearer abc');
  });
});
