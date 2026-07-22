'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import type { ColumnDef } from '@tanstack/react-table';
import { AppDataTable, AppTooltip, CreateButton, IconButton, DeleteButton } from '@/shared/ui';
import { EntityHeader, EntityStatusBadge, AuditTrailDialog } from '@/shared/entity';
import { Pencil, ShieldAlert } from 'lucide-react';
import { useCurrentUserQuery } from '@/features/auth';
import type { GetUserDetailResponse } from '@/services/user';
import { UserStatus } from '@/services/user';
import { UserFormDialog, type UserFormDialogState } from './UserFormDialog';

const USER_STATUS_LABELS: Record<string, string> = {
  [UserStatus.Active]: 'Active',
  [UserStatus.Inactive]: 'Inactive',
  [UserStatus.Suspended]: 'Suspended',
};

export function UsersPage() {
  const t = useTranslations('users');
  const tCommon = useTranslations('common.actions');
  const { data: currentUser, isLoading } = useCurrentUserQuery();
  const [formState, setFormState] = useState<UserFormDialogState | null>(null);
  const [auditUserId, setAuditUserId] = useState<string | null>(null);

  const columns: ColumnDef<GetUserDetailResponse>[] = [
    {
      id: 'name',
      header: t('table.name'),
      cell: ({ row }) =>
        `${row.original.firstName ?? ''} ${row.original.lastName ?? ''}`.trim() || '—',
    },
    { accessorKey: 'email', header: t('table.email') },
    { accessorKey: 'userName', header: t('table.username') },
    {
      id: 'status',
      header: t('table.status'),
      cell: ({ row }) => (
        <EntityStatusBadge status={row.original.status} labels={USER_STATUS_LABELS} />
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <EntityHeader
        title={t('title')}
        actions={
          <CreateButton onClick={() => setFormState({ mode: 'create' })}>
            {tCommon('create')}
          </CreateButton>
        }
      />

      <p className="text-muted-foreground text-sm">{t('listLimitationNote')}</p>

      <AppDataTable<GetUserDetailResponse>
        columns={columns}
        data={currentUser ? [currentUser] : []}
        page={1}
        pageCount={1}
        onPageChange={() => undefined}
        isLoading={isLoading}
        rowActions={(user) => (
          <div className="flex items-center justify-end gap-1">
            <IconButton
              aria-label={t('actions.edit')}
              onClick={() =>
                setFormState({
                  mode: 'edit',
                  userId: user.id,
                  defaultValues: {
                    firstName: user.firstName ?? '',
                    lastName: user.lastName ?? '',
                    phoneNumber: user.phoneNumber ?? '',
                  },
                })
              }
            >
              <Pencil />
            </IconButton>
            <IconButton
              aria-label={t('actions.auditTrail')}
              onClick={() => setAuditUserId(user.id)}
            >
              <ShieldAlert />
            </IconButton>
            <AppTooltip content={t('deleteNotAvailable')}>
              <span>
                <DeleteButton disabled size="icon" aria-label={t('actions.delete')} />
              </span>
            </AppTooltip>
          </div>
        )}
      />

      <UserFormDialog state={formState} onOpenChange={(open) => !open && setFormState(null)} />
      {auditUserId ? (
        <AuditTrailDialog
          service="User"
          entityId={auditUserId}
          open={auditUserId !== null}
          onOpenChange={(open) => !open && setAuditUserId(null)}
        />
      ) : null}
    </div>
  );
}
