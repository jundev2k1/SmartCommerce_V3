'use client';

import { useTheme } from 'next-themes';
import { Sun, Moon, Monitor } from 'lucide-react';
import { useTranslations } from 'next-intl';
import {
  AppDropdown,
  AppDropdownTrigger,
  AppDropdownContent,
  AppDropdownItem,
  IconButton,
} from '@/shared/ui';

export function ThemeToggle() {
  const { theme, setTheme } = useTheme();
  const t = useTranslations('common.theme');

  return (
    <AppDropdown>
      <AppDropdownTrigger asChild>
        <IconButton aria-label={t('light')}>
          <Sun className="scale-100 rotate-0 transition-all dark:scale-0 dark:-rotate-90" />
          <Moon className="absolute scale-0 rotate-90 transition-all dark:scale-100 dark:rotate-0" />
        </IconButton>
      </AppDropdownTrigger>
      <AppDropdownContent align="end">
        <AppDropdownItem onSelect={() => setTheme('light')} data-active={theme === 'light'}>
          <Sun /> {t('light')}
        </AppDropdownItem>
        <AppDropdownItem onSelect={() => setTheme('dark')} data-active={theme === 'dark'}>
          <Moon /> {t('dark')}
        </AppDropdownItem>
        <AppDropdownItem onSelect={() => setTheme('system')} data-active={theme === 'system'}>
          <Monitor /> {t('system')}
        </AppDropdownItem>
      </AppDropdownContent>
    </AppDropdown>
  );
}
