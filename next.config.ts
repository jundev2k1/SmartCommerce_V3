import type { NextConfig } from 'next';
import createNextIntlPlugin from 'next-intl/plugin';

const withNextIntl = createNextIntlPlugin('./src/i18n/request.ts');

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      {
        // Khi client gọi đến /api/v1/users, Next.js sẽ proxy sang Backend
        source: '/api/:path*',
        destination: 'http://localhost:5000/api/:path*',
      },
      {
        // Cấu hình proxy riêng cho SignalR Hub nếu cần
        source: '/hubs/:path*',
        destination: 'http://localhost:5000/hubs/:path*',
      },
    ];
  },
};

export default withNextIntl(nextConfig);
