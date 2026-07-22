'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  searchProducts,
  getProduct,
  createProduct,
  updateProduct,
  deleteProduct,
  addVariation,
  updateVariation,
  deleteVariation,
  reorderVariations,
  setDefaultVariation,
  assignProductCategory,
  removeProductCategory,
  assignProductTag,
  removeProductTag,
  type SearchProductsParams,
} from '@/services/product';
import type {
  CreateProductFormValues,
  UpdateProductFormValues,
  VariationFormValues,
} from '../products.schema';

export const productKeys = {
  all: ['products'] as const,
  lists: () => [...productKeys.all, 'list'] as const,
  list: (params: SearchProductsParams) => [...productKeys.lists(), params] as const,
  details: () => [...productKeys.all, 'detail'] as const,
  detail: (id: string) => [...productKeys.details(), id] as const,
};

export function useSearchProductsQuery(params: SearchProductsParams) {
  return useQuery({
    queryKey: productKeys.list(params),
    queryFn: () => searchProducts(params),
  });
}

export function useProductQuery(productId: string) {
  return useQuery({
    queryKey: productKeys.detail(productId),
    queryFn: () => getProduct(productId),
    enabled: Boolean(productId),
  });
}

export function useCreateProductMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (params: { values: CreateProductFormValues; defaultIndex: number }) =>
      createProduct({
        code: params.values.code,
        name: params.values.name,
        description: params.values.description,
        slug: params.values.slug,
        variations: params.values.variations.map((v, index) => ({
          ...toVariationRequest(v),
          isDefault: index === params.defaultIndex,
        })),
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: productKeys.lists() }),
  });
}

export function useUpdateProductMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, values }: { productId: string; values: UpdateProductFormValues }) =>
      updateProduct(productId, values),
    onSuccess: (_data, { productId }) => {
      queryClient.invalidateQueries({ queryKey: productKeys.detail(productId) });
      queryClient.invalidateQueries({ queryKey: productKeys.lists() });
    },
  });
}

export function useDeleteProductMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (productId: string) => deleteProduct(productId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: productKeys.lists() }),
  });
}

function invalidateProduct(queryClient: ReturnType<typeof useQueryClient>, productId: string) {
  queryClient.invalidateQueries({ queryKey: productKeys.detail(productId) });
  queryClient.invalidateQueries({ queryKey: productKeys.lists() });
}

function toVariationRequest(values: VariationFormValues) {
  return {
    sku: values.sku,
    price: Number(values.price),
    barcode: values.barcode || undefined,
    cost: values.cost ? Number(values.cost) : undefined,
    weight: values.weight ? Number(values.weight) : undefined,
    images: values.images,
  };
}

export function useAddVariationMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, values }: { productId: string; values: VariationFormValues }) =>
      addVariation(productId, toVariationRequest(values)),
    onSuccess: (_data, { productId }) => invalidateProduct(queryClient, productId),
  });
}

export function useUpdateVariationMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      productId,
      variationId,
      values,
    }: {
      productId: string;
      variationId: string;
      values: VariationFormValues;
    }) =>
      updateVariation(productId, variationId, {
        ...toVariationRequest(values),
        status: values.status,
      }),
    onSuccess: (_data, { productId }) => invalidateProduct(queryClient, productId),
  });
}

export function useDeleteVariationMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, variationId }: { productId: string; variationId: string }) =>
      deleteVariation(productId, variationId),
    onSuccess: (_data, { productId }) => invalidateProduct(queryClient, productId),
  });
}

export function useReorderVariationsMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      productId,
      orderedVariationIds,
    }: {
      productId: string;
      orderedVariationIds: string[];
    }) => reorderVariations(productId, { orderedVariationIds }),
    onSuccess: (_data, { productId }) => invalidateProduct(queryClient, productId),
  });
}

export function useSetDefaultVariationMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, variationId }: { productId: string; variationId: string }) =>
      setDefaultVariation(productId, variationId),
    onSuccess: (_data, { productId }) => invalidateProduct(queryClient, productId),
  });
}

export function useAssignCategoryMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, categoryId }: { productId: string; categoryId: string }) =>
      assignProductCategory(productId, categoryId),
    onSuccess: (_data, { productId }) => invalidateProduct(queryClient, productId),
  });
}

export function useRemoveCategoryMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, categoryId }: { productId: string; categoryId: string }) =>
      removeProductCategory(productId, categoryId),
    onSuccess: (_data, { productId }) => invalidateProduct(queryClient, productId),
  });
}

export function useAssignTagMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, tagId }: { productId: string; tagId: string }) =>
      assignProductTag(productId, tagId),
    onSuccess: (_data, { productId }) => invalidateProduct(queryClient, productId),
  });
}

export function useRemoveTagMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, tagId }: { productId: string; tagId: string }) =>
      removeProductTag(productId, tagId),
    onSuccess: (_data, { productId }) => invalidateProduct(queryClient, productId),
  });
}
