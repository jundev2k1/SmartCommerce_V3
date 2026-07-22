import type { ProductCategoryItemResponse } from '@/services/product';

export interface CategoryTreeNode extends ProductCategoryItemResponse {
  children: CategoryTreeNode[];
}

/**
 * ListProductCategories returns a flat list with parentCategoryId per item
 * ("so client can assemble hierarchy tree itself" — see docs/backend/product/README.md).
 * Recursive by construction, so it handles arbitrary depth for free.
 */
export function buildCategoryTree(flat: ProductCategoryItemResponse[]): CategoryTreeNode[] {
  const byId = new Map<string, CategoryTreeNode>();
  flat.forEach((item) => byId.set(item.id, { ...item, children: [] }));

  const roots: CategoryTreeNode[] = [];
  byId.forEach((node) => {
    const parent = node.parentCategoryId ? byId.get(node.parentCategoryId) : undefined;
    if (parent) {
      parent.children.push(node);
    } else {
      roots.push(node);
    }
  });
  return roots;
}

/** Descendant ids of a node — used to exclude a category from its own parent picker. */
export function collectDescendantIds(node: CategoryTreeNode): string[] {
  return node.children.flatMap((child) => [child.id, ...collectDescendantIds(child)]);
}

export function flattenTree(nodes: CategoryTreeNode[]): CategoryTreeNode[] {
  return nodes.flatMap((node) => [node, ...flattenTree(node.children)]);
}
