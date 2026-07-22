import type { ReactNode } from 'react';
import Link from 'next/link';
import { AppCard, AppCardHeader, AppCardTitle, AppCardContent, AppBadge } from '@/shared/ui';
import { ProductPrice } from './ProductPrice';
import type { SearchProductsItemResponse } from '@/services/product';

export interface ProductCardProps {
  product: SearchProductsItemResponse;
  href: string;
  /** Rendered outside the link (e.g. an "Add to cart" button) so it doesn't trigger navigation. */
  actions?: ReactNode;
}

/**
 * Reused by both the admin card-grid browse page (features/product-search)
 * and the customer-facing shop (features/shop) — see docs/modules/product-search.md
 * and docs/modules/client-mock.md.
 */
export function ProductCard({ product, href, actions }: ProductCardProps) {
  return (
    <AppCard className="overflow-hidden">
      <Link href={href}>
        {product.thumbnail ? (
          // eslint-disable-next-line @next/next/no-img-element -- arbitrary external URL, see shared/entity/ImageUrlListField
          <img
            src={product.thumbnail}
            alt={product.name ?? ''}
            className="h-32 w-full object-cover"
          />
        ) : (
          <div className="bg-muted h-32 w-full" />
        )}
        <AppCardHeader>
          <AppCardTitle className="text-base">{product.name}</AppCardTitle>
        </AppCardHeader>
        <AppCardContent className="space-y-2">
          <p className="text-sm font-medium">
            <ProductPrice value={product.defaultPrice} />
          </p>
          <div className="flex flex-wrap gap-1">
            {product.tagNames?.map((name) => (
              <AppBadge key={name} variant="outline">
                {name}
              </AppBadge>
            ))}
          </div>
        </AppCardContent>
      </Link>
      {actions ? <div className="px-6 pb-4">{actions}</div> : null}
    </AppCard>
  );
}
