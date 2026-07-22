import { env } from '@/shared/lib/env';
import { useLocaleStore } from '@/shared/stores/locale.store';
import { useSessionStore } from '@/shared/stores/session.store';
import type { ApiError } from '@/shared/types';
import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import type { ApiResponseEnvelope } from './types';

const REFRESH_ENDPOINT = '/api/auth/refresh-token';

declare module 'axios' {
  export interface InternalAxiosRequestConfig {
    _retry?: boolean;
  }
}

export const apiClient = axios.create({
  baseURL: env.apiBaseUrl,
  withCredentials: true,
});

apiClient.interceptors.request.use((config) => {
  config.headers.set('Accept-Language', useLocaleStore.getState().locale);
  // Only 2 endpoints formally require this (auth/register, user/profiles POST) per their
  // Swagger contract, but we send it on every request for consistent distributed tracing.
  config.headers.set('X-Correlation-Id', crypto.randomUUID());
  return config;
});

function toApiError(
  status: number,
  envelope: Partial<ApiResponseEnvelope<unknown>> | undefined,
  fallbackMessage: string,
): ApiError {
  return {
    code: envelope?.messageCode ?? 'UNKNOWN_ERROR',
    message: envelope?.message ?? fallbackMessage,
    status,
  };
}

function redirectToLogin() {
  useSessionStore.getState().clear();
  if (typeof window !== 'undefined' && window.location.pathname !== '/login') {
    window.location.href = '/login';
  }
}

let refreshPromise: Promise<void> | null = null;

function refreshSession(): Promise<void> {
  if (!refreshPromise) {
    refreshPromise = apiClient
      .post(REFRESH_ENDPOINT, undefined, { _retry: true } as Partial<InternalAxiosRequestConfig>)
      .then(() => undefined)
      .finally(() => {
        refreshPromise = null;
      });
  }
  return refreshPromise;
}

apiClient.interceptors.response.use(
  (response) => {
    const envelope = response.data as ApiResponseEnvelope<unknown>;
    if (!envelope.success) {
      return Promise.reject(toApiError(response.status, envelope, 'Request failed.'));
    }
    response.data = envelope.data;
    return response;
  },
  async (error: AxiosError<ApiResponseEnvelope<unknown>>) => {
    const originalRequest = error.config;
    const status = error.response?.status;
    const isRefreshCall = originalRequest?.url === REFRESH_ENDPOINT;

    if (status === 401 && originalRequest && !originalRequest._retry && !isRefreshCall) {
      originalRequest._retry = true;
      try {
        await refreshSession();
        return apiClient(originalRequest);
      } catch {
        redirectToLogin();
        return Promise.reject(toApiError(401, error.response?.data, error.message));
      }
    }

    if (status === 401 && isRefreshCall) {
      redirectToLogin();
    }

    return Promise.reject(toApiError(status ?? 0, error.response?.data, error.message));
  },
);
