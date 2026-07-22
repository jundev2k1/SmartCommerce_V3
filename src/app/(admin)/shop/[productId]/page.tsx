import { ShopProductDetailPage } from '@/features/shop';

export default async function Page({ params }: { params: Promise<{ productId: string }> }) {
  const { productId } = await params;
  return <ShopProductDetailPage productId={productId} />;
}
