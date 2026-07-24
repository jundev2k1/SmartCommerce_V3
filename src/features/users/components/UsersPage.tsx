'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import type { ColumnDef } from '@tanstack/react-table';
import {
  AppDataTable,
  AppBadge,
  AppTabs,
  AppTabsList,
  AppTabsTrigger,
  AppTabsContent,
  AppTooltip,
  CreateButton,
  IconButton,
  DeleteButton,
} from '@/shared/ui';
import { EntityHeader, EntityToolbar, EntityStatusBadge, AuditTrailDialog } from '@/shared/entity';
import { Pencil, ShieldAlert } from 'lucide-react';
import type { SearchUsersItemResponse } from '@/services/user';
import { UserStatus } from '@/services/user';
import { useUsersSearchQuery } from '../api/users.queries';
import { UserFormDialog, type UserFormDialogState } from './UserFormDialog';

const PAGE_SIZE = 20;

const USER_STATUS_LABELS: Record<string, string> = {
  [UserStatus.Active]: 'Active',
  [UserStatus.Inactive]: 'Inactive',
  [UserStatus.Suspended]: 'Suspended',
};

interface UserRoleTableProps {
  roleTab: 'admin' | 'user';
  onEdit: (user: SearchUsersItemResponse) => void;
  onAudit: (userId: string) => void;
}

function UserRoleTable({ roleTab, onEdit, onAudit }: UserRoleTableProps) {
  const t = useTranslations('users');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);

  const { data, isLoading } = useUsersSearchQuery({ roleTab, search, page, pageSize: PAGE_SIZE });

  const columns: ColumnDef<SearchUsersItemResponse>[] = [
    {
      id: 'name',
      header: t('table.name'),
      cell: ({ row }) =>
        `${row.original.firstName ?? ''} ${row.original.lastName ?? ''}`.trim() || '—',
    },
    { accessorKey: 'email', header: t('table.email') },
    { accessorKey: 'userName', header: t('table.username') },
    {
      id: 'roles',
      header: t('table.roles'),
      cell: ({ row }) =>
        row.original.roles && row.original.roles.length > 0 ? (
          <div className="flex flex-wrap gap-1">
            {row.original.roles.map((role) => (
              <AppBadge key={role} variant="secondary">
                {role}
              </AppBadge>
            ))}
          </div>
        ) : (
          '—'
        ),
    },
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
      <EntityToolbar
        onSearchChange={(value) => {
          setSearch(value);
          setPage(1);
        }}
        searchPlaceholder={t('searchPlaceholder')}
      />

      <AppDataTable<SearchUsersItemResponse>
        columns={columns}
        data={data?.items ?? []}
        page={page}
        pageCount={data?.totalPages ?? 1}
        onPageChange={setPage}
        isLoading={isLoading}
        rowActions={(user) => (
          <div className="flex items-center justify-end gap-1">
            <IconButton aria-label={t('actions.edit')} onClick={() => onEdit(user)}>
              <Pencil />
            </IconButton>
            <IconButton aria-label={t('actions.auditTrail')} onClick={() => onAudit(user.id)}>
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
    </div>
  );
}

export function UsersPage() {
  const t = useTranslations('users');
  const tCommon = useTranslations('common.actions');
  const [formState, setFormState] = useState<UserFormDialogState | null>(null);
  const [auditUserId, setAuditUserId] = useState<string | null>(null);

  function handleEdit(user: SearchUsersItemResponse) {
    setFormState({
      mode: 'edit',
      userId: user.id,
      defaultValues: {
        firstName: user.firstName ?? '',
        lastName: user.lastName ?? '',
        phoneNumber: user.phoneNumber ?? '',
      },
    });
  }

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

      <p className="text-muted-foreground text-sm">{t('rolesSnapshotNote')}</p>

      <AppTabs defaultValue="admin">
        <AppTabsList>
          <AppTabsTrigger value="admin">{t('tabs.admin')}</AppTabsTrigger>
          <AppTabsTrigger value="user">{t('tabs.user')}</AppTabsTrigger>
        </AppTabsList>
        <AppTabsContent value="admin">
          <UserRoleTable roleTab="admin" onEdit={handleEdit} onAudit={setAuditUserId} />
        </AppTabsContent>
        <AppTabsContent value="user">
          <UserRoleTable roleTab="user" onEdit={handleEdit} onAudit={setAuditUserId} />
        </AppTabsContent>
      </AppTabs>

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
