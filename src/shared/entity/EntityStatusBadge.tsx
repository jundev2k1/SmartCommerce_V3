import { AppBadge, type AppBadgeProps } from '@/shared/ui';

export interface EntityStatusBadgeProps {
  status: string | number | null | undefined;
  /** Optional label map for numeric/coded statuses — falls back to String(status). */
  labels?: Record<string, string>;
  variantMap?: Record<string, AppBadgeProps['variant']>;
}

/**
 * Generic status -> badge renderer. Handles Product's free-text status
 * ("Active"/"Inactive"/"Discontinued" — not a numeric enum, see
 * docs/backend/product/README.md) as well as any future numeric one, given a
 * label map.
 */
export function EntityStatusBadge({ status, labels, variantMap }: EntityStatusBadgeProps) {
  if (status === null || status === undefined) {
    return <AppBadge variant="outline">—</AppBadge>;
  }
  const key = String(status);
  const label = labels?.[key] ?? key;
  const variant = variantMap?.[key] ?? 'secondary';
  return <AppBadge variant={variant}>{label}</AppBadge>;
}
