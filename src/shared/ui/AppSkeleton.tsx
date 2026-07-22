import type { ComponentProps } from 'react';
import { Skeleton } from '@/components/ui/skeleton';

export type AppSkeletonProps = ComponentProps<typeof Skeleton>;

export function AppSkeleton(props: AppSkeletonProps) {
  return <Skeleton {...props} />;
}
