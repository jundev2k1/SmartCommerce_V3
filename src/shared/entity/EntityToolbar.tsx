import type { ReactNode } from 'react';
import { AppSearchBox } from '@/shared/ui';

export interface EntityToolbarProps {
  onSearchChange?: (value: string) => void;
  searchPlaceholder?: string;
  filters?: ReactNode;
  /** Rendered by the caller only while a selection is active — see SelectionPanel. */
  selectionBar?: ReactNode;
  actions?: ReactNode;
}

/** Search + filters + actions row for list pages — composes AppSearchBox, not a Product-specific implementation. */
export function EntityToolbar({
  onSearchChange,
  searchPlaceholder,
  filters,
  selectionBar,
  actions,
}: EntityToolbarProps) {
  return (
    <div className="space-y-3">
      <div className="flex flex-wrap items-center gap-2">
        {onSearchChange ? (
          <AppSearchBox
            onValueChange={onSearchChange}
            placeholder={searchPlaceholder}
            className="w-full sm:w-64"
          />
        ) : null}
        {filters}
        <div className="ml-auto flex items-center gap-2">{actions}</div>
      </div>
      {selectionBar}
    </div>
  );
}
