'use client';

import { login, logout, register } from '@/services/auth';
import { getUserDetail, type GetUserDetailResponse } from '@/services/user';
import { disconnect } from '@/shared/lib/realtime/signalr-client';
import { useSessionStore } from '@/shared/stores/session.store';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { redirect } from 'next/navigation';
import type { LoginFormValues, RegisterFormValues } from '../auth.schema';

export const CURRENT_USER_QUERY_KEY = ['auth', 'current-user'] as const;

export function toSessionUser(detail: GetUserDetailResponse) {
  const displayName =
    [detail.firstName, detail.lastName].filter(Boolean).join(' ').trim() ||
    detail.userName ||
    detail.email ||
    'User';
  return { id: detail.id, displayName, roles: detail.roles ?? [] };
}

/**
 * Auth service has no "current user" endpoint — this is the User service's
 * GetUserDetail (/profiles/current/detail). Used by AuthGuard/GuestGuard to
 * determine whether a session exists; `retry: false` since a 401 here just
 * means "not logged in," not a transient failure worth retrying.
 */
export function useCurrentUserQuery() {
  return useQuery({
    queryKey: CURRENT_USER_QUERY_KEY,
    queryFn: getUserDetail,
    retry: false,
    staleTime: 5 * 60_000,
  });
}

export function useLoginMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (values: LoginFormValues) => login(values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CURRENT_USER_QUERY_KEY }),
  });
}

/**
 * Register's own contract sets the same session cookies login does — a
 * successful register call means the user is already authenticated.
 */
export function useRegisterMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (values: RegisterFormValues) => register(values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CURRENT_USER_QUERY_KEY }),
  });
}

export function useLogoutMutation() {
  const queryClient = useQueryClient();
  const clearSession = useSessionStore((s) => s.clear);

  return useMutation({
    mutationFn: logout,
    onSettled: async () => {
      // Log out client-side even if the network call itself failed.
      await disconnect().catch(() => undefined);
      clearSession();
      queryClient.removeQueries({ queryKey: CURRENT_USER_QUERY_KEY });

      redirect('/login');
    },
  });
}
