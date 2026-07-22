export interface ProductPriceProps {
  value: number | null | undefined;
  className?: string;
}

/**
 * No currency field exists anywhere in the Product or Order contracts, so this
 * renders a plain 2-decimal number rather than inventing a currency symbol —
 * same reasoning as the `.toFixed(2)` rendering used throughout `features/products`.
 */
export function ProductPrice({ value, className }: ProductPriceProps) {
  return <span className={className}>{value != null ? value.toFixed(2) : '—'}</span>;
}
