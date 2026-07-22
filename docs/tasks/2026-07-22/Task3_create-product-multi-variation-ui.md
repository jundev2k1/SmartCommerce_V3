# Task 3: Create-product UI should support multiple variations from the start

**Status:** Done. Implemented multi-variation UI, verified via typecheck + eslint.

## Ask

Admin should be able to create a product with several variations up front, in one create flow, rather than creating the product with one variation and adding the rest afterward on the detail page.

## Current state

`src/features/products/components/ProductCreateDialog.tsx` — its own docstring (:15-19) states the current design intentionally:

> "CreateProduct requires at least one variation up front (aggregate creation) — this dialog collects the minimum (sku + price); the rest of variant management (add more, reorder, set default) happens on the product's own detail page afterward."

The form is flat, not a repeatable list: `code`, `name`, `slug`, `description`, `variationSku` (:63-66), `variationPrice` (:67-71) — a single scalar SKU/price pair.

Schema `createProductSchema` (`src/features/products/products.schema.ts:21-28`) matches: `variationSku: z.string()`, `variationPrice: numericString(...)` — scalar fields, not an array of variation objects. No field-array (`useFieldArray`) support exists here.

Mutation mapping, `useCreateProductMutation` (`src/features/products/api/products.queries.ts:50-63`):

```ts
createProduct({
  code: values.code,
  name: values.name,
  description: values.description,
  slug: values.slug,
  variations: [{ sku: values.variationSku, price: Number(values.variationPrice) }],
}),
```

Wraps the single pair into a one-element array purely to satisfy the request type.

## Backend already supports N variations at creation — no backend change needed

`src/services/product/create-product.ts:9-18`:

```ts
export interface CreateProductRequest {
  code: string;
  name: string;
  description?: string;
  slug: string;
  /** At least one variation required. If none is marked isDefault, the first is used. */
  variations: CreateProductVariationRequest[];
  categoryIds?: string[];
  tagIds?: string[];
}
```

`variations` is already `CreateProductVariationRequest[]`, and `CreateProductVariationRequest` extends the same `VariationInput` used by `AddVariation` (sku, price, barcode, cost, weight, dimensions, images, plus `isDefault?`). The gap is entirely in `ProductCreateDialog.tsx` + `createProductSchema` + `useCreateProductMutation` — none of which currently build or send more than one element.

## Suggested implementation

1. Convert the single sku/price fields in `createProductSchema` into a `variations: z.array(variationFormSchema).min(1)`-shaped field, reusing the same per-variation fields already built for `VariantFormDialog` (sku/price/barcode/cost/weight/images) as repeatable rows via `useFieldArray`, with one marked `isDefault`.
2. Update `ProductCreateDialog.tsx` to render the variation rows as an add/remove-able list instead of the current flat sku/price pair.
3. Update `useCreateProductMutation`'s mapping to pass the full array through instead of wrapping a single pair.
4. No changes needed to `src/services/product/create-product.ts` — the request type already supports this.

## Implementation

1. **Schema:** Converted `createProductSchema` to have `variations: z.array(variationFormSchema).min(1)` instead of scalar `variationSku`/`variationPrice` fields, reusing `variationFormSchema` already defined for the mutation layers.

2. **Component:** Updated `ProductCreateDialog.tsx` to render variations as add/remove-able rows via `useFieldArray`, with:
   - A radio button per variation to select the default (first is selected by default)
   - Add/remove buttons for variations (at least one required)
   - Full per-variation fields (sku, price, barcode, cost, images) via `Controller` for nested form paths
   - Scrollable form body to keep modal height reasonable with many variations

3. **Mutation:** Updated `useCreateProductMutation` to map `variations` array and set `isDefault` based on the selected index, passing `{ values, defaultIndex }` to the API.

4. **Types:** No changes needed to `src/services/product/create-product.ts` — the request type already supported N variations.

Verified: `tsc --noEmit` and `eslint` both clean.
