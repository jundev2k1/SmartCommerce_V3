'use client';

import { Plus, X, GripVertical } from 'lucide-react';
import { useTranslations } from 'next-intl';
import { AppInput, IconButton } from '@/shared/ui';

export interface ImageUrlListFieldProps {
  value: string[];
  onChange: (urls: string[]) => void;
  addLabel: string;
}

/**
 * Add/remove/reorder a list of image URL strings, with a preview per URL.
 * There is no image-upload endpoint on any of the 7 backend services — every
 * variation's `images` field is just `string[]` (URLs), so this manages that
 * list directly rather than pretending to be a file uploader. See
 * docs/decisions/0014-shared-entity-component-layer.md.
 */
export function ImageUrlListField({ value, onChange, addLabel }: ImageUrlListFieldProps) {
  const t = useTranslations('entity.imageUrlListField');

  function updateAt(index: number, url: string) {
    onChange(value.map((v, i) => (i === index ? url : v)));
  }

  function removeAt(index: number) {
    onChange(value.filter((_, i) => i !== index));
  }

  function moveUp(index: number) {
    if (index === 0) return;
    const next = [...value];
    [next[index - 1], next[index]] = [next[index], next[index - 1]];
    onChange(next);
  }

  return (
    <div className="space-y-2">
      {value.map((url, index) => (
        <div key={index} className="flex items-center gap-2">
          <IconButton
            type="button"
            aria-label={t('moveUp')}
            disabled={index === 0}
            onClick={() => moveUp(index)}
          >
            <GripVertical />
          </IconButton>
          {url ? (
            // eslint-disable-next-line @next/next/no-img-element -- arbitrary external URLs, not known at build time; next/image requires an allowlisted domain
            <img
              src={url}
              alt=""
              className="size-9 shrink-0 rounded object-cover"
              onError={(e) => {
                e.currentTarget.style.visibility = 'hidden';
              }}
            />
          ) : null}
          <AppInput
            value={url}
            placeholder={t('urlPlaceholder')}
            onChange={(e) => updateAt(index, e.target.value)}
          />
          <IconButton type="button" aria-label={t('removeImage')} onClick={() => removeAt(index)}>
            <X />
          </IconButton>
        </div>
      ))}
      <IconButton type="button" aria-label={addLabel} onClick={() => onChange([...value, ''])}>
        <Plus />
      </IconButton>
    </div>
  );
}
