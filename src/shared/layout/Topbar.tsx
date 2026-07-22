'use client';

import type { ReactNode } from 'react';
import { usePathname } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { SidebarTrigger } from '@/components/ui/sidebar';
import { Separator } from '@/components/ui/separator';
import { AppBreadcrumb, type AppBreadcrumbItem } from '@/shared/ui';
import { navGroups } from '@/shared/config/nav';
import { ThemeToggle } from './ThemeToggle';
import { UserMenu } from './UserMenu';

const allNavItems = navGroups.flatMap((group) => group.items);

function useBreadcrumbItems(): AppBreadcrumbItem[] {
  const pathname = usePathname();
  const tNav = useTranslations('nav');

  if (pathname === '/') {
    return [{ label: tNav('dashboard') }];
  }

  const activeItem = allNavItems.find((item) => item.href === pathname);
  return [
    { label: tNav('dashboard'), href: '/' },
    { label: activeItem ? tNav(activeItem.labelKey) : pathname.replace('/', '') },
  ];
}

export interface TopbarProps {
  onLogout?: () => void;
  /**
   * Real content supplied by app/(admin)/layout.tsx from features/notifications
   * — shared/layout can't import features/, so this stays a slot prop, same
   * shape as onLogout. See docs/decisions/0013-shared-shell-callback-props.md
   * and docs/decisions/0017-shared-notifications-component-layer.md.
   */
  notificationSlot?: ReactNode;
}

export function Topbar({ onLogout, notificationSlot }: TopbarProps) {
  const items = useBreadcrumbItems();

  return (
    <header className="flex h-14 shrink-0 items-center gap-2 border-b px-4">
      <SidebarTrigger />
      <Separator orientation="vertical" className="h-6" />
      <AppBreadcrumb items={items} />
      <div className="ml-auto flex items-center gap-1">
        {notificationSlot}
        <ThemeToggle />
        <UserMenu onLogout={onLogout} />
      </div>
    </header>
  );
}
