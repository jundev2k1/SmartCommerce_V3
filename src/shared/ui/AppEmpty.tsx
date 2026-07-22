import type { ReactNode } from 'react';
import { Inbox } from 'lucide-react';
import { cn } from '@/shared/lib/utils';

export interface AppEmptyProps {
  title?: string;
  description?: string;
  icon?: ReactNode;
  action?: ReactNode;
  className?: string;
}

export function AppEmpty({ title, description, icon, action, className }: AppEmptyProps) {
  return (
    <div
      className={cn(
        'flex flex-col items-center justify-center gap-2 rounded-md border border-dashed py-12 text-center',
        className,
      )}
    >
      <div className="text-muted-foreground">{icon ?? <Inbox className="size-8" />}</div>
      {title ? <p className="text-sm font-medium">{title}</p> : null}
      {description ? <p className="text-muted-foreground text-sm">{description}</p> : null}
      {action}
    </div>
  );
}
