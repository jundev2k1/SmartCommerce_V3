import type { ReactNode } from 'react';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';

export interface AppTooltipProps {
  content: ReactNode;
  children: ReactNode;
}

/**
 * Requires a TooltipProvider ancestor — mounted once in shared/layout's
 * AdminShell/Providers, not per-usage.
 */
export function AppTooltip({ content, children }: AppTooltipProps) {
  return (
    <Tooltip>
      <TooltipTrigger asChild>{children}</TooltipTrigger>
      <TooltipContent>{content}</TooltipContent>
    </Tooltip>
  );
}
