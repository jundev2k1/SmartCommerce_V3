import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface CreateProductTagRequest {
  code: string;
  name: string;
}

export interface CreateProductTagResponse {
  productTagId: string;
}

/** POST /tags — creates a new flat product tag (no hierarchy). */
export function createProductTag(
  request: CreateProductTagRequest,
): Promise<CreateProductTagResponse> {
  return apiClient.post(`${BASE_PATH}/tags`, request).then((res) => res.data);
}
