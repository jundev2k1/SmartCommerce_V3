'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createUser, getUser, updateUser } from '@/services/user';
import { CURRENT_USER_QUERY_KEY } from '@/features/auth';
import type { CreateUserFormValues, UpdateUserFormValues } from '../users.schema';

export const userKeys = {
  all: ['users'] as const,
  detail: (userId: string) => ['users', 'detail', userId] as const,
};

/**
 * No list endpoint exists (see docs/backend/user/README.md), so this is the
 * only read hook beyond the current-user query already used for the session.
 */
export function useUserQuery(userId: string) {
  return useQuery({
    queryKey: userKeys.detail(userId),
    queryFn: () => getUser(userId),
    enabled: Boolean(userId),
  });
}

export function useCreateUserMutation() {
  return useMutation({
    mutationFn: (values: CreateUserFormValues) =>
      createUser({ ...values, tempPassword: values.tempPassword || undefined }),
  });
}

export function useUpdateUserMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, values }: { userId: string; values: UpdateUserFormValues }) =>
      updateUser(userId, values),
    onSuccess: (_data, { userId }) => {
      queryClient.invalidateQueries({ queryKey: userKeys.detail(userId) });
      queryClient.invalidateQueries({ queryKey: CURRENT_USER_QUERY_KEY });
    },
  });
}
