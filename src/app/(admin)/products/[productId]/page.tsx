import { ProductDetailPage } from '@/features/products';

export default async function Page({ params }: { params: Promise<{ productId: string }> }) {
  const { productId } = await params;
  return <ProductDetailPage productId={productId} />;
}
