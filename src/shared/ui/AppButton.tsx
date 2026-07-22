'use client';

import * as React from 'react';
import { Loader2 } from 'lucide-react';
import { Button, buttonVariants } from '@/components/ui/button';
import type { VariantProps } from 'class-variance-authority';

export interface AppButtonProps
  extends React.ComponentProps<typeof Button>, VariantProps<typeof buttonVariants> {
  loading?: boolean;
}

/**
 * Base of the button hierarchy (see docs/frontend/ui-components.md#button-hierarchy).
 * Business pages should use a semantic preset (PrimaryButton, DeleteButton, ...)
 * instead of styling AppButton directly.
 */
export function AppButton({
  loading = false,
  disabled,
  asChild,
  children,
  ...props
}: AppButtonProps) {
  if (asChild) {
    // Slot requires exactly one child element — skip the loading icon injection
    // below, which doesn't apply when AppButton is rendering as its child anyway.
    return (
      <Button asChild disabled={disabled || loading} {...props}>
        {children}
      </Button>
    );
  }

  return (
    <Button disabled={disabled || loading} {...props}>
      {loading && <Loader2 className="animate-spin" />}
      {children}
    </Button>
  );
}
