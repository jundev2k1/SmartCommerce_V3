import { AppEmpty } from '@/shared/ui';

export interface PlaceholderModulePageProps {
  title: string;
  description?: string;
}

/**
 * Rendered by every not-yet-implemented module's page.tsx (see docs/roadmap.md).
 * Replaced by that module's real feature page once its phase starts.
 */
export function PlaceholderModulePage({ title, description }: PlaceholderModulePageProps) {
  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">{title}</h1>
      <AppEmpty title={title} description={description} />
    </div>
  );
}
