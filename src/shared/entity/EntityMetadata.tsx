export interface EntityMetadataItem {
  label: string;
  value: string;
}

export interface EntityMetadataProps {
  items: EntityMetadataItem[];
}

/** Small key/value grid for detail-page metadata (id, createdAt, updatedAt, ...). */
export function EntityMetadata({ items }: EntityMetadataProps) {
  return (
    <dl className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      {items.map((item) => (
        <div key={item.label} className="space-y-1">
          <dt className="text-muted-foreground text-sm">{item.label}</dt>
          <dd className="text-sm font-medium break-all">{item.value}</dd>
        </div>
      ))}
    </dl>
  );
}
