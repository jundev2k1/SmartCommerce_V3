import type { ComponentProps } from 'react';
import { Badge } from '@/components/ui/badge';

export type AppBadgeProps = ComponentProps<typeof Badge>;

export function AppBadge(props: AppBadgeProps) {
  return <Badge {...props} />;
}
