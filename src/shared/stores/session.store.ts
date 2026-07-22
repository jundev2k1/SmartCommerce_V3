import { create } from 'zustand';

/**
 * Minimal auth session cache — a derived snapshot of the real session (owned by
 * the backend / TanStack Query once Phase 2 lands), not an independent source of
 * truth. See docs/state/zustand-strategy.md.
 */
interface SessionUser {
  id: string;
  displayName: string;
  roles: string[];
}

interface SessionState {
  user: SessionUser | null;
  isAuthenticated: boolean;
  setSession: (user: SessionUser) => void;
  clear: () => void;
}

export const useSessionStore = create<SessionState>((set) => ({
  user: null,
  isAuthenticated: false,
  setSession: (user) => set({ user, isAuthenticated: true }),
  clear: () => set({ user: null, isAuthenticated: false }),
}));
