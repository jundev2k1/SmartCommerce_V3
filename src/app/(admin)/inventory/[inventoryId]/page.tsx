import { InventoryDetailPage } from '@/features/inventory';

export default async function Page({ params }: { params: Promise<{ inventoryId: string }> }) {
  const { inventoryId } = await params;
  return <InventoryDetailPage inventoryId={inventoryId} />;
}
