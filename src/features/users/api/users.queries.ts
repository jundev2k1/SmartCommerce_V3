'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AppRole, createUser, getUser, searchUsers, updateUser } from '@/services/user';
import { CURRENT_USER_QUERY_KEY } from '@/features/auth';
import type { CreateUserFormValues, UpdateUserFormValues } from '../users.schema';

export const userKeys = {
  all: ['users'] as const,
  detail: (userId: string) => ['users', 'detail', userId] as const,
  searches: () => [...userKeys.all, 'search'] as const,
  search: (params: UsersSearchParams) => [...userKeys.searches(), params] as const,
};

export function useUserQuery(userId: string) {
  return useQuery({
    queryKey: userKeys.detail(userId),
    queryFn: () => getUser(userId),
    enabled: Boolean(userId),
  });
}

export interface UsersSearchParams {
  /** 'admin' matches Root + Admin (role != User); 'user' matches User only. */
  roleTab: 'admin' | 'user';
  search?: string;
  page?: number;
  pageSize?: number;
}

export function useUsersSearchQuery(params: UsersSearchParams) {
  return useQuery({
    queryKey: userKeys.search(params),
    queryFn: () =>
      searchUsers({
        keyword: params.search || undefined,
        filters: [
          {
            field: 'role',
            operator: params.roleTab === 'user' ? 'eq' : 'ne',
            value: AppRole.User,
          },
        ],
        page: params.page,
        pageSize: params.pageSize,
      }),
  });
}

export function useCreateUserMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (values: CreateUserFormValues) =>
      createUser({ ...values, tempPassword: values.tempPassword || undefined }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: userKeys.searches() }),
  });
}

export function useUpdateUserMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, values }: { userId: string; values: UpdateUserFormValues }) =>
      updateUser(userId, values),
    onSuccess: (_data, { userId }) => {
      queryClient.invalidateQueries({ queryKey: userKeys.detail(userId) });
      queryClient.invalidateQueries({ queryKey: userKeys.searches() });
      queryClient.invalidateQueries({ queryKey: CURRENT_USER_QUERY_KEY });
    },
  });
}
