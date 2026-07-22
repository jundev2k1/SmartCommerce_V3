'use client';

import { useMemo, useState, type ReactNode } from 'react';
import {
  type ColumnDef,
  type RowSelectionState,
  type VisibilityState,
  flexRender,
  getCoreRowModel,
  useReactTable,
} from '@tanstack/react-table';
import { Columns3 } from 'lucide-react';
import {
  AppTable,
  AppTableHeader,
  AppTableBody,
  AppTableRow,
  AppTableHead,
  AppTableCell,
} from './AppTable';
import { AppPagination } from './AppPagination';
import { AppSkeleton } from './AppSkeleton';
import { AppEmpty } from './AppEmpty';
import { AppCheckbox } from './AppCheckbox';
import { IconButton } from './buttons/IconButton';
import {
  AppDropdown,
  AppDropdownTrigger,
  AppDropdownContent,
  AppDropdownCheckboxItem,
} from './AppDropdown';

export interface AppDataTableProps<TData> {
  columns: ColumnDef<TData>[];
  data: TData[];
  page: number;
  pageCount: number;
  onPageChange: (page: number) => void;
  isLoading?: boolean;
  toolbar?: ReactNode;
  emptyTitle?: string;
  emptyDescription?: string;
  rowActions?: (row: TData) => ReactNode;
  /** Adds a checkbox column. Requires `getRowId` so selection keys match entity ids, not row index. */
  enableRowSelection?: boolean;
  rowSelection?: RowSelectionState;
  onRowSelectionChange?: (selection: RowSelectionState) => void;
  getRowId?: (row: TData) => string;
  /**
   * `shared/ui` has no i18n dependency of its own (text always comes in via
   * props) — these default to English so existing call sites are unaffected;
   * a caller introducing a second locale overrides them without touching
   * this component. See docs/frontend/i18n.md.
   */
  selectAllLabel?: string;
  selectRowLabel?: string;
  toggleColumnsLabel?: string;
}

/**
 * Generic TanStack Table wrapper in manual-pagination mode — features only
 * configure columns/data/pagination, never call useReactTable directly.
 * See docs/frontend/ui-components.md and docs/state/query-strategy.md.
 */
export function AppDataTable<TData>({
  columns,
  data,
  page,
  pageCount,
  onPageChange,
  isLoading = false,
  toolbar,
  emptyTitle,
  emptyDescription,
  rowActions,
  enableRowSelection = false,
  rowSelection,
  onRowSelectionChange,
  getRowId,
  selectAllLabel = 'Select all',
  selectRowLabel = 'Select row',
  toggleColumnsLabel = 'Toggle columns',
}: AppDataTableProps<TData>) {
  const [columnVisibility, setColumnVisibility] = useState<VisibilityState>({});

  const resolvedColumns = useMemo<ColumnDef<TData>[]>(() => {
    let result = columns;

    if (enableRowSelection) {
      result = [
        {
          id: 'select',
          enableHiding: false,
          header: ({ table }) => (
            <AppCheckbox
              checked={table.getIsAllPageRowsSelected()}
              onCheckedChange={(checked) => table.toggleAllPageRowsSelected(Boolean(checked))}
              aria-label={selectAllLabel}
            />
          ),
          cell: ({ row }) => (
            <AppCheckbox
              checked={row.getIsSelected()}
              onCheckedChange={(checked) => row.toggleSelected(Boolean(checked))}
              aria-label={selectRowLabel}
            />
          ),
        },
        ...result,
      ];
    }

    if (rowActions) {
      result = [
        ...result,
        {
          id: 'actions',
          header: '',
          enableHiding: false,
          cell: ({ row }) => rowActions(row.original),
        },
      ];
    }

    return result;
  }, [columns, rowActions, enableRowSelection, selectAllLabel, selectRowLabel]);

  const table = useReactTable({
    data,
    columns: resolvedColumns,
    pageCount,
    manualPagination: true,
    enableRowSelection,
    getRowId,
    state: {
      columnVisibility,
      ...(enableRowSelection ? { rowSelection: rowSelection ?? {} } : {}),
    },
    onColumnVisibilityChange: setColumnVisibility,
    onRowSelectionChange: (updater) => {
      if (!onRowSelectionChange) return;
      const next = typeof updater === 'function' ? updater(rowSelection ?? {}) : updater;
      onRowSelectionChange(next);
    },
    getCoreRowModel: getCoreRowModel(),
  });

  const hideableColumns = table.getAllColumns().filter((column) => column.getCanHide());

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2">
        <div className="flex-1">{toolbar}</div>
        {hideableColumns.length > 0 ? (
          <AppDropdown>
            <AppDropdownTrigger asChild>
              <IconButton aria-label={toggleColumnsLabel}>
                <Columns3 />
              </IconButton>
            </AppDropdownTrigger>
            <AppDropdownContent align="end">
              {hideableColumns.map((column) => (
                <AppDropdownCheckboxItem
                  key={column.id}
                  checked={column.getIsVisible()}
                  onCheckedChange={(checked) => column.toggleVisibility(Boolean(checked))}
                  onSelect={(e) => e.preventDefault()}
                >
                  {column.id}
                </AppDropdownCheckboxItem>
              ))}
            </AppDropdownContent>
          </AppDropdown>
        ) : null}
      </div>
      <div className="rounded-md border">
        <AppTable>
          <AppTableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <AppTableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <AppTableHead key={header.id}>
                    {header.isPlaceholder
                      ? null
                      : flexRender(header.column.columnDef.header, header.getContext())}
                  </AppTableHead>
                ))}
              </AppTableRow>
            ))}
          </AppTableHeader>
          <AppTableBody>
            {isLoading ? (
              <AppTableRow>
                <AppTableCell colSpan={resolvedColumns.length}>
                  <AppSkeleton className="h-8 w-full" />
                </AppTableCell>
              </AppTableRow>
            ) : table.getRowModel().rows.length > 0 ? (
              table.getRowModel().rows.map((row) => (
                <AppTableRow key={row.id} data-state={row.getIsSelected() ? 'selected' : undefined}>
                  {row.getVisibleCells().map((cell) => (
                    <AppTableCell key={cell.id}>
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </AppTableCell>
                  ))}
                </AppTableRow>
              ))
            ) : (
              <AppTableRow>
                <AppTableCell colSpan={resolvedColumns.length}>
                  <AppEmpty title={emptyTitle} description={emptyDescription} />
                </AppTableCell>
              </AppTableRow>
            )}
          </AppTableBody>
        </AppTable>
      </div>
      <AppPagination page={page} pageCount={pageCount} onPageChange={onPageChange} />
    </div>
  );
}
