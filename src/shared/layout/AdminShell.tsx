import type { ReactNode } from 'react';
import { SidebarProvider, SidebarInset } from '@/components/ui/sidebar';
import { Sidebar } from './Sidebar';
import { Topbar } from './Topbar';

export interface AdminShellProps {
  children: ReactNode;
  onLogout?: () => void;
  notificationSlot?: ReactNode;
}

export function AdminShell({ children, onLogout, notificationSlot }: AdminShellProps) {
  return (
    <SidebarProvider>
      <Sidebar />
      <SidebarInset>
        <Topbar onLogout={onLogout} notificationSlot={notificationSlot} />
        <main className="flex-1 space-y-6 p-6">{children}</main>
      </SidebarInset>
    </SidebarProvider>
  );
}
