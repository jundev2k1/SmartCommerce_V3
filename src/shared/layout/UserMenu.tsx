'use client';

import { LogOut, Settings, User as UserIcon } from 'lucide-react';
import { useTranslations } from 'next-intl';
import {
  AppAvatar,
  AppDropdown,
  AppDropdownTrigger,
  AppDropdownContent,
  AppDropdownItem,
  AppDropdownLabel,
  AppDropdownSeparator,
} from '@/shared/ui';
import { useSessionStore } from '@/shared/stores/session.store';

export interface UserMenuProps {
  /**
   * Real behavior supplied by app/(admin)/layout.tsx (via features/auth's
   * useLogoutMutation) — shared/layout can't import features/, so this stays
   * a plain callback prop. See docs/decisions/0013-shared-shell-callback-props.md.
   */
  onLogout?: () => void;
}

export function UserMenu({ onLogout }: UserMenuProps) {
  const user = useSessionStore((s) => s.user);
  const t = useTranslations('common.userMenu');
  const displayName = user?.displayName ?? 'Guest';

  return (
    <AppDropdown>
      <AppDropdownTrigger asChild>
        <button
          type="button"
          className="focus-visible:ring-ring rounded-full outline-none focus-visible:ring-2"
        >
          <AppAvatar fallback={displayName.slice(0, 2).toUpperCase()} />
        </button>
      </AppDropdownTrigger>
      <AppDropdownContent align="end" className="w-56">
        <AppDropdownLabel>{displayName}</AppDropdownLabel>
        <AppDropdownSeparator />
        <AppDropdownItem>
          <UserIcon /> {t('profile')}
        </AppDropdownItem>
        <AppDropdownItem>
          <Settings /> {t('settings')}
        </AppDropdownItem>
        <AppDropdownSeparator />
        <AppDropdownItem variant="destructive" onSelect={onLogout} disabled={!onLogout}>
          <LogOut /> {t('logout')}
        </AppDropdownItem>
      </AppDropdownContent>
    </AppDropdown>
  );
}
