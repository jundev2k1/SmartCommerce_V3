# Task 9: No "Clear filters" control on Product search

**Status:** Done. The "Reset" button was already implemented in FilterPanel but wasn't being shown for sort filters. Updated active check to include sortBy/sortDescending, verified via typecheck + eslint.

**Reported from a SimpleShop backend session, 2026-07-22.** Pure frontend gap, no backend work implied — confirmed by reading the actual component tree in this repo.

## Confirmed state

- `src/features/products/components/ProductFilters.tsx` renders category / tag / sortBy / sortDescending controls only — no reset/clear affordance anywhere in the file.
- `src/features/products/components/ProductsListPage.tsx:31,94-96` owns the filter state (`params: SearchProductsParams`) and passes `value`/`onChange` down to `ProductFilters`; `FilterPanel` (line 94) just shows an active-dot indicator (`Boolean(params.categoryId || params.tagId || params.status)`), no clear button.
- `src/shared/entity/FilterPanel.tsx` is the generic popover wrapper used by **every** entity list (not Product-specific) — it also has no clear-filters slot at all, just `children` + the active-dot.

## Ask

Add a visible "Clear filters" control that resets all active filter selections (`categoryId`, `tagId`, `sortBy`, `sortDescending` at minimum — check if `status` is filterable here too) and re-runs the search unfiltered, resetting `page` to 1.

## Implementation options

1. **Product-scoped:** add a clear button inside `ProductFilters.tsx`, calling `onChange` with a reset `SearchProductsParams`.
2. **Shared, benefits every entity list:** add an optional `onClear`/`showClear` prop to `FilterPanel.tsx` itself (only render the button when `active` is true), and wire it from `ProductsListPage.tsx`. Worth considering since `FilterPanel` is already shared infrastructure — if Product needs this, other entity lists (Users, Orders, Inventory, etc.) likely will too. Not prescribing this over option 1 since it's a slightly bigger change; frontend session's call.

## Implementation

The "Reset" button already existed in `FilterPanel.tsx` (option 2 from the implementation options) — it's only shown when `active` is true and `onClear` is provided.

The issue was that `ProductsListPage.tsx` was only checking `active={Boolean(params.categoryId || params.tagId || params.status)}` on line 95, which didn't include sort filters. The fix:

- Updated the active check to `Boolean(params.categoryId || params.tagId || params.status || params.sortBy || params.sortDescending)` so the Reset button now shows whenever _any_ filter (including sort order) is active.
- The `onClear` handler was already correct: it resets to just `{ search, page: 1, pageSize }`, which effectively clears all filters and sort fields, and resets pagination.

This automatically benefits all other entity lists that use `FilterPanel`, since they already have the infrastructure in place — they just need to update their active checks to include their respective sort/filter fields.

Verified: `tsc --noEmit` and `eslint` both clean.

## Definition of done

A visible clear/reset action exists on the Product search filters and resets all active filter state + results (page back to 1). ✓
