'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  listProductCategories,
  createProductCategory,
  updateProductCategory,
  deleteProductCategory,
} from '@/services/product';
import { buildCategoryTree } from '../categories.utils';
import type { CreateCategoryFormValues, UpdateCategoryFormValues } from '../categories.schema';

export const categoryKeys = {
  all: ['categories'] as const,
  list: () => [...categoryKeys.all, 'list'] as const,
};

/** ListProductCategories is flat + unpaginated ("small reference data") — this assembles the tree client-side. */
export function useCategoriesTreeQuery() {
  return useQuery({
    queryKey: categoryKeys.list(),
    queryFn: listProductCategories,
    select: (response) => buildCategoryTree(response.categories ?? []),
  });
}

export function useCreateCategoryMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (values: CreateCategoryFormValues) => createProductCategory(values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: categoryKeys.list() }),
  });
}

export function useUpdateCategoryMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      categoryId,
      values,
    }: {
      categoryId: string;
      values: UpdateCategoryFormValues;
    }) => updateProductCategory(categoryId, values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: categoryKeys.list() }),
  });
}

export function useDeleteCategoryMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (categoryId: string) => deleteProductCategory(categoryId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: categoryKeys.list() }),
  });
}
