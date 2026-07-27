import type { IdempotencyOperationId } from './operations';

export interface IdempotentRequestConfig {
  /** Looked up in the IdempotencyManager to attach/reuse/clear the Idempotency-Key header. */
  operationId: IdempotencyOperationId;
}

declare module 'axios' {
  export interface AxiosRequestConfig {
    idempotency?: IdempotentRequestConfig;
  }
  export interface InternalAxiosRequestConfig {
    idempotency?: IdempotentRequestConfig;
  }
}
