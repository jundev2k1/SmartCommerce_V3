# Task 2: Cannot create a new variation of an existing product

**Status:** Resolved on the backend side, 2026-07-22 — see cross-ref. No frontend code change needed.

## Reported payload

```json
{
  "sku": "E2EB2SKU-1784707349",
  "price": 70,
  "barcode": "1234512412",
  "cost": 20,
  "images": []
}
```

No HTTP status/response body was captured alongside this report.

## Investigation — this repo's side

`src/services/product/add-variation.ts:6-21`:

```ts
export interface VariationInput {
  sku: string; // required
  price: number; // required
  barcode?: string;
  cost?: number;
  weight?: number;
  dimensionsLength?: number;
  dimensionsWidth?: number;
  dimensionsHeight?: number;
  images?: string[];
}
export interface AddVariationRequest extends VariationInput {
  makeDefault?: boolean;
}
```

The reported payload satisfies this type exactly (only `sku`/`price` are required; everything else is optional and correctly typed as numbers, not strings).

- Zod schema `variationFormSchema` (`src/features/products/products.schema.ts:42-50`) matches: only `sku`/`price` required, no dimensions field exists in the schema at all (dimensions aren't collected by the form).
- Form component `src/features/products/components/VariantFormDialog.tsx` renders `sku` (:77), `price` (:78), `barcode` (:81), `cost` (:82), `status` (:84, edit-mode only), `images` (:85-92) — no `weight` field rendered despite existing in the type/schema, no dimensions fields anywhere.
- Payload construction, `toVariationRequest` (`src/features/products/api/products.queries.ts:90-99`):
  ```ts
  function toVariationRequest(values: VariationFormValues) {
    return {
      sku: values.sku,
      price: Number(values.price),
      barcode: values.barcode || undefined,
      cost: values.cost ? Number(values.cost) : undefined,
      weight: values.weight ? Number(values.weight) : undefined,
      images: values.images,
    };
  }
  ```
  This reproduces the exact reported payload shape when `sku`/`price`/`barcode`/`cost` are filled and `weight` is left blank (dropped as `undefined`, not sent as `null`/`0`). `dimensionsLength/Width/Height` are never populated by the UI at all, even though `VariationInput` declares them.

**Conclusion — no frontend-side type/schema violation.** Nothing in this repo's own required-field contract is broken by this payload; the request the UI actually sends matches what was reported.

## Investigation — backend side (from a paired backend session, see `SimpleShop/docs/tasks/2026-07-22/Task1_verify-add-variation-contract.md`)

Backend confirms this payload passes `AddVariationValidator` cleanly (field-level rules don't reject it). The leading hypothesis is a **409 Conflict from a global (not product-scoped) SKU-uniqueness check** — `ProductRepo.SkuExistsAsync` scans every product's variations, not just the target product's, so if this exact SKU was already inserted once (e.g. a repeated test run — the SKU's timestamp-looking suffix suggests an automated/E2E origin), any later `AddVariation` call with the same SKU 409s regardless of which product it targets.

## Resolved 2026-07-22 (backend session)

Backend confirmed: global (cross-product) SKU uniqueness is **intentional**, not a bug — the 409 was correct behavior for a reused/stale SKU. The backend also found and fixed a real, separate bug that's the more likely actual cause of an _intermittent_ repro: a TOCTOU race where two concurrent `AddVariation` requests for the same new SKU could both pass the uniqueness pre-check and then crash with an unhandled 500 instead of a clean 409 — now fixed and tested.

**Also improved:** the 409 message now names which product currently owns the conflicting SKU (e.g. `"Variation with SKU (X) already exists (used by product \"Y\")"`), which directly answers this task's second open question — the toast will now be self-diagnosing without needing a cross-repo investigation next time.

## Status

**Resolved, no frontend action needed.** Nothing here indicated a frontend bug for the fields given, and that's confirmed — the payload was correct, the 409 (or intermittent 500, now fixed) came entirely from backend-side SKU-uniqueness enforcement.

**Cross-ref:** SimpleShop `docs/tasks/2026-07-22/Task1_verify-add-variation-contract.md`.
