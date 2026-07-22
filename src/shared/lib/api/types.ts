import type { ApiError } from '@/shared/types';

/**
 * Real backend response envelope (confirmed against Swagger in Phase 1.5 —
 * supersedes the Phase 1 placeholder). Every endpoint across all services
 * returns this shape. `success` can be false even on an HTTP 200.
 */
export interface ApiResponseEnvelope<T> {
  success: boolean;
  message: string | null;
  messageCode: string | null;
  data: T;
  details: unknown | null;
}

export function isEnvelope(value: unknown): value is ApiResponseEnvelope<unknown> {
  return typeof value === 'object' && value !== null && 'success' in value && 'data' in value;
}

export function isApiError(value: unknown): value is ApiError {
  return (
    typeof value === 'object' &&
    value !== null &&
    'code' in value &&
    'message' in value &&
    'status' in value
  );
}
