import { Loader2 } from 'lucide-react';
import { cn } from '@/shared/lib/utils';

export interface AppLoadingProps {
  className?: string;
  label?: string;
}

export function AppLoading({ className, label }: AppLoadingProps) {
  return (
    <div
      className={cn('text-muted-foreground flex items-center justify-center gap-2 py-8', className)}
    >
      <Loader2 className="size-5 animate-spin" />
      {label ? <span className="text-sm">{label}</span> : null}
    </div>
  );
}
