import { client } from '@tedu-market/api-client';

export { client };

export type AccessTokenGetter = () => Promise<string | null>;

/** Gắn `baseUrl` và header Bearer (token lấy từ bộ nhớ qua `getAccessToken`) cho mọi lời gọi API. */
export function configureApiClient(apiUrl: string, getAccessToken: AccessTokenGetter): void {
  client.setConfig({ baseUrl: apiUrl });
  client.interceptors.request.use(async (request) => {
    const token = await getAccessToken();
    if (token !== null) {
      request.headers.set('Authorization', `Bearer ${token}`);
    }
    return request;
  });
}
