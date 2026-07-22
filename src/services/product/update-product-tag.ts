import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface UpdateProductTagRequest {
  name: string;
}

/** PUT /tags/{tagId} — renames a product tag. */
export function updateProductTag(tagId: string, request: UpdateProductTagRequest): Promise<void> {
  return apiClient.put(`${BASE_PATH}/tags/${tagId}`, request).then(() => undefined);
}
