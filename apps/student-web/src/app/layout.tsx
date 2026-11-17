import { Toaster } from '@tedu-market/ui';
import type { Metadata, Viewport } from 'next';
import { Inter } from 'next/font/google';
import type { ReactNode } from 'react';
import { SiteHeader } from '../components/site-header';
import { getSiteUrl, SITE_DESCRIPTION, SITE_LOCALE, SITE_NAME } from '../lib/site';
import './globals.css';

const inter = Inter({
  subsets: ['latin', 'vietnamese'],
  variable: '--font-inter',
  display: 'swap',
});

export const metadata: Metadata = {
  metadataBase: getSiteUrl(),
  title: { default: SITE_NAME, template: `%s | ${SITE_NAME}` },
  description: SITE_DESCRIPTION,
  openGraph: { siteName: SITE_NAME, locale: SITE_LOCALE, type: 'website' },
};

export const viewport: Viewport = { width: 'device-width', initialScale: 1 };

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="vi" className={inter.variable}>
      <body className="min-h-screen antialiased">
        <a
          href="#main-content"
          className="sr-only rounded-lg bg-card px-4 py-2 focus:not-sr-only focus:absolute focus:left-4 focus:top-2 focus:z-50"
        >
          Bỏ qua điều hướng
        </a>
        <SiteHeader />
        <main id="main-content" className="mx-auto max-w-7xl px-4 py-6 md:py-12">
          {children}
        </main>
        <Toaster />
      </body>
    </html>
  );
}
