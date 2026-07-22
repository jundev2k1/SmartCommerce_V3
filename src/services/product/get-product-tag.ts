import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface GetProductTagResponse {
  id: string;
  code: string | null;
  name: string | null;
  createdAt: string;
  updatedAt: string;
}

/** GET /tags/{tagId} */
export function getProductTag(tagId: string): Promise<GetProductTagResponse> {
  return apiClient.get(`${BASE_PATH}/tags/${tagId}`).then((res) => res.data);
}
