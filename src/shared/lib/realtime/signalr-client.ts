import * as signalR from '@microsoft/signalr';
import { env } from '@/shared/lib/env';
import { useSignalrStore } from '@/shared/stores/signalr.store';

/**
 * Single SignalR hub connection manager (see docs/realtime/signalr-strategy.md).
 * Nothing calls connect() yet — this is architecture only until Phase 10 wires
 * real feature subscriptions (e.g. notifications) on top of it.
 */
let connection: signalR.HubConnection | null = null;

function getConnection(): signalR.HubConnection {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(env.signalrHubUrl, { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    connection.onreconnecting(() => useSignalrStore.getState().setStatus('connecting'));
    connection.onreconnected(() => useSignalrStore.getState().setStatus('connected'));
    connection.onclose(() => useSignalrStore.getState().setStatus('disconnected'));
  }
  return connection;
}

export async function connect(): Promise<void> {
  const hub = getConnection();
  if (hub.state !== signalR.HubConnectionState.Disconnected) {
    return;
  }
  useSignalrStore.getState().setStatus('connecting');
  await hub.start();
  useSignalrStore.getState().setStatus('connected');
}

export async function disconnect(): Promise<void> {
  if (!connection) {
    return;
  }
  await connection.stop();
  useSignalrStore.getState().setStatus('disconnected');
}

export function on<T = unknown>(event: string, handler: (payload: T) => void): void {
  getConnection().on(event, handler as (...args: unknown[]) => void);
}

export function off(event: string, handler: (...args: unknown[]) => void): void {
  getConnection().off(event, handler);
}
