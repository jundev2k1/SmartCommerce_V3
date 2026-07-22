import type { ReactNode } from 'react';
import { AppBreadcrumb, type AppBreadcrumbItem } from '@/shared/ui';
import { EntityHeader } from './EntityHeader';

export interface EntityDetailHeaderProps {
  breadcrumbItems: AppBreadcrumbItem[];
  title: string;
  description?: string;
  actions?: ReactNode;
}

/**
 * Breadcrumb + EntityHeader, always paired identically atop every detail
 * page (Product, Warehouse, Inventory record, Order) — extracted once the
 * exact same two-component composition showed up in four modules. See
 * docs/decisions/0014-shared-entity-component-layer.md.
 */
export function EntityDetailHeader({
  breadcrumbItems,
  title,
  description,
  actions,
}: EntityDetailHeaderProps) {
  return (
    <>
      <AppBreadcrumb items={breadcrumbItems} />
      <EntityHeader title={title} description={description} actions={actions} />
    </>
  );
}
