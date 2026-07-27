import { IDEMPOTENCY_KEY_TTL_MS } from '@/shared/lib/constants';
import type { IdempotencyOperationId } from './operations';

/** Matches the backend's `HeaderKeys.IdempotencyKey` (see docs/services/api-layer.md). */
export const IDEMPOTENCY_HEADER_NAME = 'Idempotency-Key';

interface IdempotencyEntry {
  key: string;
  createdAt: number;
}

/**
 * Tracks one Idempotency-Key per in-progress business operation so the Axios
 * layer can attach/reuse/clear it without any page or component knowing this
 * exists. Lifecycle per operationId:
 *  - `getKey` creates a key on first call, then keeps returning the same one
 *    for every retry/resubmit until either `complete` runs or the entry's
 *    TTL lapses (an abandoned operation nobody ever finished).
 *  - `complete` (called by the response interceptor on a real success) clears
 *    the entry immediately, so the very next call starts a brand-new key.
 * A failed/rejected request does nothing here by design — the entry survives
 * so a retry of the same operation reuses its key, per the refresh strategy.
 */
export class IdempotencyManager {
  private readonly entries = new Map<IdempotencyOperationId, IdempotencyEntry>();

  constructor(private readonly ttlMs: number = IDEMPOTENCY_KEY_TTL_MS) {}

  getKey(operationId: IdempotencyOperationId): string {
    this.evictExpired();

    const existing = this.entries.get(operationId);
    if (existing) {
      return existing.key;
    }

    const entry: IdempotencyEntry = { key: crypto.randomUUID(), createdAt: Date.now() };
    this.entries.set(operationId, entry);
    return entry.key;
  }

  complete(operationId: IdempotencyOperationId): void {
    this.entries.delete(operationId);
  }

  private evictExpired(): void {
    const now = Date.now();
    for (const [operationId, entry] of this.entries) {
      if (now - entry.createdAt > this.ttlMs) {
        this.entries.delete(operationId);
      }
    }
  }
}

export const idempotencyManager = new IdempotencyManager();
