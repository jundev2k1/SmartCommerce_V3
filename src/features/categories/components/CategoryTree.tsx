'use client';

import { CategoryTreeNode } from './CategoryTreeNode';
import type { CategoryTreeNode as TreeNode } from '../categories.utils';

export interface CategoryTreeProps {
  nodes: TreeNode[];
  onEdit: (node: TreeNode) => void;
  onDelete: (node: TreeNode) => void;
  onAddChild: (node: TreeNode) => void;
}

export function CategoryTree({ nodes, onEdit, onDelete, onAddChild }: CategoryTreeProps) {
  return (
    <div className="rounded-md border py-1">
      {nodes.map((node) => (
        <CategoryTreeNode
          key={node.id}
          node={node}
          depth={0}
          onEdit={onEdit}
          onDelete={onDelete}
          onAddChild={onAddChild}
        />
      ))}
    </div>
  );
}
