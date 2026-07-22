import MockAdapter from 'axios-mock-adapter';
import { apiClient } from './client';
import { env } from '@/shared/lib/env';
import type { ApiResponseEnvelope } from './types';

/**
 * Dev-only mock adapter proving the Axios -> interceptor -> TanStack Query round trip
 * (see docs/roadmap.md Phase 1 completion criteria) before any real backend exists.
 * Replaced entirely in Phase 12 (Backend Integration Pass) — see docs/api/README.md.
 */
let installed = false;

export function setupMockApi() {
  if (installed || process.env.NODE_ENV !== 'development' || !env.useMockApi) {
    return;
  }
  installed = true;

  const mock = new MockAdapter(apiClient, { delayResponse: 300, onNoMatch: 'passthrough' });

  mock.onGet('/dashboard/summary').reply(200, {
    success: true,
    message: null,
    messageCode: null,
    details: null,
    data: {
      totalOrders: 128,
      pendingOrders: 7,
      lowStockItems: 3,
    },
  } satisfies ApiResponseEnvelope<{
    totalOrders: number;
    pendingOrders: number;
    lowStockItems: number;
  }>);
}
