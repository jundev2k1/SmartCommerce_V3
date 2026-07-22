'use client';

import { useEffect, useState } from 'react';
import { Search, X } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { cn } from '@/shared/lib/utils';

export interface AppSearchBoxProps {
  /** Only used as the initial value — this is an uncontrolled-input pattern.
   *  To force-reset externally, remount with a different `key`. */
  defaultValue?: string;
  onValueChange: (value: string) => void;
  placeholder?: string;
  debounceMs?: number;
  className?: string;
  /** Defaults to English — `shared/ui` has no i18n dependency of its own, see docs/frontend/i18n.md. */
  clearLabel?: string;
}

export function AppSearchBox({
  defaultValue = '',
  onValueChange,
  placeholder,
  debounceMs = 300,
  className,
  clearLabel = 'Clear search',
}: AppSearchBoxProps) {
  const [draft, setDraft] = useState(defaultValue);

  useEffect(() => {
    const timer = setTimeout(() => onValueChange(draft), debounceMs);
    return () => clearTimeout(timer);
  }, [draft, debounceMs, onValueChange]);

  return (
    <div className={cn('relative', className)}>
      <Search className="text-muted-foreground absolute top-1/2 left-2.5 size-4 -translate-y-1/2" />
      <Input
        value={draft}
        onChange={(e) => setDraft(e.target.value)}
        placeholder={placeholder}
        className="px-8"
      />
      {draft ? (
        <button
          type="button"
          aria-label={clearLabel}
          className="text-muted-foreground hover:text-foreground absolute top-1/2 right-2.5 -translate-y-1/2"
          onClick={() => setDraft('')}
        >
          <X className="size-4" />
        </button>
      ) : null}
    </div>
  );
}
