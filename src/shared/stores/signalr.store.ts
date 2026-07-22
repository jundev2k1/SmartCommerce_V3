import { create } from 'zustand';

export type SignalrConnectionStatus = 'disconnected' | 'connecting' | 'connected';

interface SignalrState {
  status: SignalrConnectionStatus;
  setStatus: (status: SignalrConnectionStatus) => void;
}

export const useSignalrStore = create<SignalrState>((set) => ({
  status: 'disconnected',
  setStatus: (status) => set({ status }),
}));
