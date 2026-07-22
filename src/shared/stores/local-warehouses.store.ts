import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface LocalWarehousesState {
  warehouseIds: string[];
  addWarehouseId: (warehouseId: string) => void;
}

/**
 * The Inventory service has no list/search endpoint for warehouses (see
 * docs/backend/inventory/README.md — only CreateWarehouse and GetWarehouse
 * exist). This tracks the ids of warehouses created from this browser so
 * Warehouses has something to list; the Warehouses page also offers a manual
 * "open by id" lookup for warehouses that exist but weren't created here.
 * See docs/modules/inventory-management.md.
 */
export const useLocalWarehousesStore = create<LocalWarehousesState>()(
  persist(
    (set) => ({
      warehouseIds: [],
      addWarehouseId: (warehouseId) =>
        set((state) => ({
          warehouseIds: state.warehouseIds.includes(warehouseId)
            ? state.warehouseIds
            : [warehouseId, ...state.warehouseIds],
        })),
    }),
    { name: 'simpleshopui-local-warehouses' },
  ),
);
