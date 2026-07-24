'use client';

import { useTranslations } from 'next-intl';
import { AppModal, type AppSelectOption } from '@/shared/ui';
import { CreateCategoryForm } from './CreateCategoryForm';
import { EditCategoryForm } from './EditCategoryForm';

export type CategoryFormDialogState =
  { mode: 'create'; defaultParentId?: string } | { mode: 'edit'; categoryId: string };

export interface CategoryFormDialogProps {
  state: CategoryFormDialogState | null;
  categoryOptions: AppSelectOption[];
  onOpenChange: (open: boolean) => void;
}

export function CategoryFormDialog({
  state,
  categoryOptions,
  onOpenChange,
}: CategoryFormDialogProps) {
  const t = useTranslations('categories.formDialog');
  const isCreate = state?.mode === 'create';

  return (
    <AppModal
      open={state !== null}
      onOpenChange={onOpenChange}
      title={isCreate ? t('createTitle') : t('editTitle')}
    >
      {state?.mode === 'create' ? (
        <CreateCategoryForm
          categoryOptions={categoryOptions}
          defaultParentId={state.defaultParentId}
          onDone={() => onOpenChange(false)}
        />
      ) : state?.mode === 'edit' ? (
        <EditCategoryForm
          categoryId={state.categoryId}
          categoryOptions={categoryOptions}
          onDone={() => onOpenChange(false)}
        />
      ) : null}
    </AppModal>
  );
}
