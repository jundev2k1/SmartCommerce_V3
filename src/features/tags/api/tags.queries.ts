'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  listProductTags,
  createProductTag,
  updateProductTag,
  deleteProductTag,
} from '@/services/product';
import type { CreateTagFormValues, UpdateTagFormValues } from '../tags.schema';

export const tagKeys = {
  all: ['tags'] as const,
  list: () => [...tagKeys.all, 'list'] as const,
};

export function useTagsQuery() {
  return useQuery({
    queryKey: tagKeys.list(),
    queryFn: listProductTags,
    select: (response) => response.tags ?? [],
  });
}

export function useCreateTagMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (values: CreateTagFormValues) => createProductTag(values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tagKeys.list() }),
  });
}

export function useUpdateTagMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ tagId, values }: { tagId: string; values: UpdateTagFormValues }) =>
      updateProductTag(tagId, values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tagKeys.list() }),
  });
}

export function useDeleteTagMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (tagId: string) => deleteProductTag(tagId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: tagKeys.list() }),
  });
}
