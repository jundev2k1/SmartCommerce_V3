import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface LocalInventoryIdsState {
  inventoryIds: string[];
  addInventoryId: (inventoryId: string) => void;
}

/**
 * No endpoint in the Inventory contract ever returns/discovers an
 * `inventoryId` starting from a product or warehouse (GetProductStock only
 * returns a rollup total — see docs/backend/inventory/README.md). Every
 * inventory id this frontend ever learns comes from a manual "open by id"
 * lookup; this remembers those ids (and any acted on via stock-in/out/adjust)
 * so Stock/Stock Transactions/Warehouse Detail have a working set to show
 * instead of requiring re-entry every visit. See
 * docs/modules/inventory-management.md.
 */
export const useLocalInventoryIdsStore = create<LocalInventoryIdsState>()(
  persist(
    (set) => ({
      inventoryIds: [],
      addInventoryId: (inventoryId) =>
        set((state) => ({
          inventoryIds: state.inventoryIds.includes(inventoryId)
            ? state.inventoryIds
            : [inventoryId, ...state.inventoryIds],
        })),
    }),
    { name: 'simpleshopui-local-inventory-ids' },
  ),
);
