function readEnv(key: string, fallback: string): string {
  return process.env[key] ?? fallback;
}

export const env = {
  // Gateway origin only — each src/services/<service> module supplies its own
  // '/api/<service>' prefix already, per that service's Swagger `servers.url`.
  apiBaseUrl: readEnv('NEXT_PUBLIC_API_BASE_URL', ''),
  signalrHubUrl: readEnv('NEXT_PUBLIC_SIGNALR_HUB_URL', '/hubs/global'),
  useMockApi: readEnv('NEXT_PUBLIC_USE_MOCK_API', 'true') === 'true',
} as const;
