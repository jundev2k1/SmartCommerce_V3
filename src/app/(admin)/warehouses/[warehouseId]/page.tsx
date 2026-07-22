import { WarehouseDetailPage } from '@/features/inventory';

export default async function Page({ params }: { params: Promise<{ warehouseId: string }> }) {
  const { warehouseId } = await params;
  return <WarehouseDetailPage warehouseId={warehouseId} />;
}
