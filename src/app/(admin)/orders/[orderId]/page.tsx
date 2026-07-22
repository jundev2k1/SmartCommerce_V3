import { OrderDetailPage } from '@/features/orders';

export default async function Page({ params }: { params: Promise<{ orderId: string }> }) {
  const { orderId } = await params;
  return <OrderDetailPage orderId={orderId} />;
}
