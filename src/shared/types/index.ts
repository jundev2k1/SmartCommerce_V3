export interface ApiError {
  code: string;
  message: string;
  status: number;
}

export interface PaginatedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CursorResult<T> {
  items: T[];
  nextCursor: string | null;
}

export type SortDirection = 'asc' | 'desc';
