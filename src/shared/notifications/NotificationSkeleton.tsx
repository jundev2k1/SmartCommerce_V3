import { AppSkeleton } from '@/shared/ui';

export interface NotificationSkeletonProps {
  count?: number;
}

/** Loading placeholder rows for the notification list/dropdown. */
export function NotificationSkeleton({ count = 3 }: NotificationSkeletonProps) {
  return (
    <div className="space-y-3 p-2">
      {Array.from({ length: count }).map((_, index) => (
        <div key={index} className="flex items-center gap-3">
          <AppSkeleton className="size-8 shrink-0 rounded-full" />
          <div className="flex-1 space-y-1.5">
            <AppSkeleton className="h-3.5 w-3/4" />
            <AppSkeleton className="h-3 w-1/2" />
          </div>
        </div>
      ))}
    </div>
  );
}
