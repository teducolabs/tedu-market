import type { NextConfig } from 'next';

const nextConfig: NextConfig = {
  transpilePackages: ['@tedu-market/ui', '@tedu-market/api-client'],
};

export default nextConfig;
