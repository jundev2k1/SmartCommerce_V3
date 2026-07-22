import type { ReactNode } from 'react';

export interface EntityHeaderProps {
  title: string;
  description?: string;
  actions?: ReactNode;
}

/** Consistent title + actions row atop every list/detail page. See docs/decisions/0014-shared-entity-component-layer.md. */
export function EntityHeader({ title, description, actions }: EntityHeaderProps) {
  return (
    <div className="flex items-start justify-between gap-4">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">{title}</h1>
        {description ? <p className="text-muted-foreground text-sm">{description}</p> : null}
      </div>
      {actions ? <div className="flex items-center gap-2">{actions}</div> : null}
    </div>
  );
}
