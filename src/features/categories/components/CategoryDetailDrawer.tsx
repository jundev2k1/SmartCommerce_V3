'use client';

import { useTranslations } from 'next-intl';
import {
  AppDrawer,
  AppDrawerContent,
  AppDrawerHeader,
  AppDrawerTitle,
  AppDrawerDescription,
} from '@/shared/ui';
import { AuditTrailButton, EntityStatusBadge } from '@/shared/entity';
import type { CategoryTreeNode } from '../categories.utils';

export interface CategoryDetailDrawerProps {
  node: CategoryTreeNode | null;
  onOpenChange: (open: boolean) => void;
}

/** Lightweight detail view — categories are otherwise managed inline in the tree, with no dedicated route. */
export function CategoryDetailDrawer({ node, onOpenChange }: CategoryDetailDrawerProps) {
  const t = useTranslations('categories.detailDrawer');

  return (
    <AppDrawer open={node !== null} onOpenChange={onOpenChange}>
      <AppDrawerContent>
        <AppDrawerHeader>
          <AppDrawerTitle>{node?.name ?? node?.code ?? t('title')}</AppDrawerTitle>
          <AppDrawerDescription>{t('description')}</AppDrawerDescription>
        </AppDrawerHeader>
        {node ? (
          <div className="space-y-4 px-4">
            <div className="space-y-1">
              <p className="text-muted-foreground text-xs">{t('code')}</p>
              <p className="text-sm font-medium">{node.code ?? '—'}</p>
            </div>
            <div className="space-y-1">
              <p className="text-muted-foreground text-xs">{t('status')}</p>
              <EntityStatusBadge status={node.status} />
            </div>
            <div className="space-y-1">
              <p className="text-muted-foreground text-xs">{t('id')}</p>
              <p className="font-mono text-xs">{node.id}</p>
            </div>
            {/*
              service is the originating backend service name, not the entity
              subtype — Category is owned by the Product service (see
              docs/backend/product/README.md), same as service="Product" used
              for actual Product entities. Passing "Category" here would
              silently return an empty list (see Task 1's root cause).
            */}
            <AuditTrailButton service="Product" entityId={node.id} />
          </div>
        ) : null}
      </AppDrawerContent>
    </AppDrawer>
  );
}
