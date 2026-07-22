'use client';

import type { ReactNode } from 'react';
import { AdminShell } from '@/shared/layout';
import { AuthGuard, useLogoutMutation } from '@/features/auth';
import { NotificationBell } from '@/features/notifications';

function AdminShellWithLogout({ children }: { children: ReactNode }) {
  const logoutMutation = useLogoutMutation();
  return (
    <AdminShell onLogout={() => logoutMutation.mutate()} notificationSlot={<NotificationBell />}>
      {children}
    </AdminShell>
  );
}

export default function AdminLayout({ children }: { children: ReactNode }) {
  return (
    <AuthGuard>
      <AdminShellWithLogout>{children}</AdminShellWithLogout>
    </AuthGuard>
  );
}
