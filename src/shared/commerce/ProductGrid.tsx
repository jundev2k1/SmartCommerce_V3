import type { ReactNode } from 'react';

export interface ProductGridProps<T> {
  items: T[];
  renderItem: (item: T) => ReactNode;
  getKey: (item: T) => string;
}

/** Generic responsive grid layout — presentation only, no fetching/pagination opinions. */
export function ProductGrid<T>({ items, renderItem, getKey }: ProductGridProps<T>) {
  return (
    <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
      {items.map((item) => (
        <div key={getKey(item)}>{renderItem(item)}</div>
      ))}
    </div>
  );
}
