import { cn } from '@/shared/lib/utils';

export interface UnreadBadgeProps {
  count: number;
  className?: string;
}

/** Small dot/count badge — renders nothing at count 0, a dot up to 9, "9+" beyond. */
export function UnreadBadge({ count, className }: UnreadBadgeProps) {
  if (count <= 0) {
    return null;
  }
  return (
    <span
      className={cn(
        'bg-destructive flex min-w-4 items-center justify-center rounded-full px-1 text-[10px] leading-none text-white',
        className,
      )}
    >
      {count > 9 ? '9+' : count}
    </span>
  );
}
