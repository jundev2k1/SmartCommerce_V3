'use client';

import { useEffect } from 'react';
import { on, off } from './signalr-client';

/**
 * Generic "subscribe to one hub event for the lifetime of this component"
 * hook — the reusable abstraction docs/realtime/signalr-strategy.md calls
 * for, so a feature never touches `signalr-client` directly. Re-subscribes
 * whenever `handler`'s identity changes, so pass a `useCallback`'d handler
 * from the caller to avoid churn.
 */
export function useHubEvent<T = unknown>(
  event: string,
  handler: (payload: T) => void,
  enabled = true,
) {
  useEffect(() => {
    if (!enabled) {
      return;
    }
    on<T>(event, handler);
    return () => off(event, handler as (...args: unknown[]) => void);
  }, [event, handler, enabled]);
}
