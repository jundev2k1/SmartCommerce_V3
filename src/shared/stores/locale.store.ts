import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { DEFAULT_LOCALE } from '@/shared/lib/constants';

/**
 * Current locale. Minimal by design — next-intl owns message resolution;
 * this store just lets non-React code (e.g. the Axios client's Accept-Language
 * header) read the active locale without a hook. See docs/frontend/i18n.md.
 */
interface LocaleState {
  locale: string;
  setLocale: (locale: string) => void;
}

export const useLocaleStore = create<LocaleState>()(
  persist(
    (set) => ({
      locale: DEFAULT_LOCALE,
      setLocale: (locale) => set({ locale }),
    }),
    { name: 'simpleshopui-locale' },
  ),
);
