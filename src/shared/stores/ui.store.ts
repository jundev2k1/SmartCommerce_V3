import { create } from 'zustand';

/**
 * Global UI state: sidebar collapse + the cross-cutting modal registry
 * (see docs/decisions/0011-modal-strategy.md). Local/single-tree modals use
 * AppDialog/AppModal + useDisclosure directly and never touch this store.
 */
interface UiState {
  sidebarCollapsed: boolean;
  toggleSidebar: () => void;
  setSidebarCollapsed: (collapsed: boolean) => void;

  activeModalId: string | null;
  modalParams: Record<string, unknown> | null;
  openModal: (id: string, params?: Record<string, unknown>) => void;
  closeModal: () => void;
}

export const useUiStore = create<UiState>((set) => ({
  sidebarCollapsed: false,
  toggleSidebar: () => set((s) => ({ sidebarCollapsed: !s.sidebarCollapsed })),
  setSidebarCollapsed: (collapsed) => set({ sidebarCollapsed: collapsed }),

  activeModalId: null,
  modalParams: null,
  openModal: (id, params) => set({ activeModalId: id, modalParams: params ?? null }),
  closeModal: () => set({ activeModalId: null, modalParams: null }),
}));
