# Task 9: No "Clear filters" control on Product search

**Status:** Not started. Migrated from the former flat `docs/tasks/product-search-clear-filter.md` (same content, relocated into the dated-folder convention — see [../README.md](../README.md)). Not part of the 2026-07-22 numbered task list given by the user; carried over as an existing open item from the same date.

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

## Definition of done

A visible clear/reset action exists on the Product search filters and resets all active filter state + results (page back to 1).
