'use client';

import { useState } from 'react';
import { ChevronRight, ChevronDown, Eye, Pencil, Trash2, Plus } from 'lucide-react';
import { useTranslations } from 'next-intl';
import { IconButton } from '@/shared/ui';
import type { CategoryTreeNode as TreeNode } from '../categories.utils';

export interface CategoryTreeNodeProps {
  node: TreeNode;
  depth: number;
  onView: (node: TreeNode) => void;
  onEdit: (node: TreeNode) => void;
  onDelete: (node: TreeNode) => void;
  onAddChild: (node: TreeNode) => void;
}

/** Recursive by construction — no depth assumption, matches arbitrary backend nesting. */
export function CategoryTreeNode({
  node,
  depth,
  onView,
  onEdit,
  onDelete,
  onAddChild,
}: CategoryTreeNodeProps) {
  const t = useTranslations('categories');
  const tCommon = useTranslations('common.actions');
  const [expanded, setExpanded] = useState(true);
  const hasChildren = node.children.length > 0;

  return (
    <div>
      <div
        className="hover:bg-muted/50 flex items-center gap-1 rounded-md px-2 py-1.5"
        style={{ paddingLeft: `${depth * 1.5 + 0.5}rem` }}
      >
        <IconButton
          aria-label={expanded ? tCommon('collapse') : tCommon('expand')}
          onClick={() => setExpanded((e) => !e)}
          className={hasChildren ? '' : 'invisible'}
        >
          {expanded ? <ChevronDown /> : <ChevronRight />}
        </IconButton>
        <span className="flex-1 text-sm font-medium">{node.name}</span>
        <span className="text-muted-foreground text-xs">{node.code}</span>
        <IconButton aria-label={tCommon('view')} onClick={() => onView(node)}>
          <Eye />
        </IconButton>
        <IconButton aria-label={t('addChild')} onClick={() => onAddChild(node)}>
          <Plus />
        </IconButton>
        <IconButton aria-label={tCommon('edit')} onClick={() => onEdit(node)}>
          <Pencil />
        </IconButton>
        <IconButton aria-label={tCommon('delete')} onClick={() => onDelete(node)}>
          <Trash2 />
        </IconButton>
      </div>
      {expanded && hasChildren ? (
        <div>
          {node.children.map((child) => (
            <CategoryTreeNode
              key={child.id}
              node={child}
              depth={depth + 1}
              onView={onView}
              onEdit={onEdit}
              onDelete={onDelete}
              onAddChild={onAddChild}
            />
          ))}
        </div>
      ) : null}
    </div>
  );
}
