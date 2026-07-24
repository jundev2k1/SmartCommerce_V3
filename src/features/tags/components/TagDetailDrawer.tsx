'use client';

import { useTranslations } from 'next-intl';
import {
  AppDrawer,
  AppDrawerContent,
  AppDrawerHeader,
  AppDrawerTitle,
  AppDrawerDescription,
} from '@/shared/ui';
import { AuditTrailButton } from '@/shared/entity';
import type { ProductTagItemResponse } from '@/services/product';

export interface TagDetailDrawerProps {
  tag: ProductTagItemResponse | null;
  onOpenChange: (open: boolean) => void;
}

/** Lightweight detail view — tags are otherwise managed inline in the list, with no dedicated route. */
export function TagDetailDrawer({ tag, onOpenChange }: TagDetailDrawerProps) {
  const t = useTranslations('tags.detailDrawer');

  return (
    <AppDrawer open={tag !== null} onOpenChange={onOpenChange}>
      <AppDrawerContent>
        <AppDrawerHeader>
          <AppDrawerTitle>{tag?.name ?? tag?.code ?? t('title')}</AppDrawerTitle>
          <AppDrawerDescription>{t('description')}</AppDrawerDescription>
        </AppDrawerHeader>
        {tag ? (
          <div className="space-y-4 px-4">
            <div className="space-y-1">
              <p className="text-muted-foreground text-xs">{t('code')}</p>
              <p className="text-sm font-medium">{tag.code ?? '—'}</p>
            </div>
            <div className="space-y-1">
              <p className="text-muted-foreground text-xs">{t('id')}</p>
              <p className="font-mono text-xs">{tag.id}</p>
            </div>
            {/* service is the backend service name (Product owns Tag), not the entity subtype — see CategoryDetailDrawer for why. */}
            <AuditTrailButton service="Product" entityId={tag.id} />
          </div>
        ) : null}
      </AppDrawerContent>
    </AppDrawer>
  );
}
