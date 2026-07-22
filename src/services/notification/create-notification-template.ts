import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { NotificationChannelType } from './types/enums';

export interface CreateNotificationTemplateCommand {
  name: string;
  channel: NotificationChannelType;
  subject?: string;
  body: string;
  /** Placeholder variable names usable in `subject`/`body` (e.g. "customerName"). */
  variables?: string[];
}

export interface CreateNotificationTemplateResponse {
  id: string;
}

/** POST /notification-templates — a reusable, channel-scoped template selected by rules/campaigns. */
export function createNotificationTemplate(
  command: CreateNotificationTemplateCommand,
): Promise<CreateNotificationTemplateResponse> {
  return apiClient.post(`${BASE_PATH}/notification-templates`, command).then((res) => res.data);
}
