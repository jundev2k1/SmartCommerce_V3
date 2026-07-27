export const APP_NAME = 'SimpleShopUI';

export const DEFAULT_PAGE_SIZE = 20;
export const PAGE_SIZE_OPTIONS = [10, 20, 50, 100] as const;

export const DEFAULT_LOCALE = 'en';
export const SUPPORTED_LOCALES = ['en'] as const;

export const QUERY_STALE_TIME_MS = 30_000;

/**
 * How long a client-generated Idempotency-Key stays reusable for an
 * operation that hasn't succeeded yet (retries, validation-error resubmits).
 * Independent of the backend's own replay-cache TTL for completed requests
 * (24h, see BuildingBlock.Application IdempotencyOptions) — this one only
 * bounds how long an abandoned/never-completed attempt can still claim a key
 * before the next attempt for that operation gets a fresh one.
 */
export const IDEMPOTENCY_KEY_TTL_MS = 30 * 60 * 1000;
