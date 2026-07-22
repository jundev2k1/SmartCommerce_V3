import type { ReactNode } from 'react';
import { GuestGuard } from '@/features/auth';
import { ThemeToggle } from '@/shared/layout';
import { APP_NAME } from '@/shared/lib/constants';

export default function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <GuestGuard>
      <div className="flex min-h-svh flex-col items-center justify-center gap-8 p-6">
        <div className="absolute top-4 right-4">
          <ThemeToggle />
        </div>
        <p className="text-lg font-semibold">{APP_NAME}</p>
        {children}
      </div>
    </GuestGuard>
  );
}
