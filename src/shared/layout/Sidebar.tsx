'use client';

import {
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  Sidebar as SidebarPrimitive,
} from '@/components/ui/sidebar';
import { hasRequiredRole, navGroups } from '@/shared/config/nav';
import { APP_NAME } from '@/shared/lib/constants';
import { useSessionStore } from '@/shared/stores/session.store';
import { AppBadge } from '@/shared/ui';
import { LayoutDashboard } from 'lucide-react';
import { useTranslations } from 'next-intl';
import Link from 'next/link';
import { usePathname } from 'next/navigation';

export function Sidebar() {
  const pathname = usePathname();
  const tNav = useTranslations('nav');
  const tCommon = useTranslations('common');

  const EMPTY_ROLES: string[] = [];
  const userRoles = useSessionStore((s) => s.user?.roles ?? EMPTY_ROLES);

  const visibleGroups = navGroups
    .filter((group) => hasRequiredRole(userRoles, group.roles))
    .map((group) => ({
      ...group,
      items: group.items.filter((item) => hasRequiredRole(userRoles, item.roles ?? group.roles)),
    }))
    .filter((group) => group.items.length > 0);

  return (
    <SidebarPrimitive collapsible="icon">
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton size="lg" asChild>
              <Link href="/">
                <LayoutDashboard />
                <span className="font-semibold">{APP_NAME}</span>
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>
      <SidebarContent>
        {visibleGroups.map((group) => (
          <SidebarGroup key={group.key}>
            <SidebarGroupLabel>{tNav(group.labelKey)}</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {group.items.map((item) => {
                  const Icon = item.icon;
                  const label = tNav(item.labelKey);

                  if (!item.implemented || !item.href) {
                    return (
                      <SidebarMenuItem key={item.key}>
                        <SidebarMenuButton
                          disabled
                          tooltip={label}
                          className="cursor-not-allowed opacity-60"
                        >
                          <Icon />
                          <span>{label}</span>
                        </SidebarMenuButton>
                        <AppBadge
                          variant="outline"
                          className="absolute top-1.5 right-2 text-[10px] group-data-[collapsible=icon]:hidden"
                        >
                          {tCommon('comingSoon')}
                        </AppBadge>
                      </SidebarMenuItem>
                    );
                  }

                  return (
                    <SidebarMenuItem key={item.key}>
                      <SidebarMenuButton asChild isActive={pathname === item.href} tooltip={label}>
                        <Link href={item.href}>
                          <Icon />
                          <span>{label}</span>
                        </Link>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  );
                })}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        ))}
      </SidebarContent>
    </SidebarPrimitive>
  );
}
