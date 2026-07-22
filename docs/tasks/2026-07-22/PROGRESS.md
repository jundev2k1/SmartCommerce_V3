# Progress — 2026-07-22

Status legend: `[ ]` not started · `[~]` in progress · `[b]` blocked · `[x]` done

- [x] **Task 1 — Audit trail dialog empty despite success.** Fixed: bounded multi-page search (up to 5 pages) plus a distinct "search truncated" empty state. Server-side `entityId` filter remains the real long-term fix (backlog.md §5).
- [x] **Task 2 — Cannot create variation (`AddVariation`).** Resolved on the backend — global SKU uniqueness confirmed intentional, a real TOCTOU race found and fixed, error message improved. No frontend fix needed.
- [x] **Task 3 — Create-product UI: only one variation at creation.** Implemented multi-variation UI with add/remove rows, default-variation radio button, full per-variation fields via useFieldArray + Controller.
- [ ] **Task 4 — Category/Tag missing audit trail.** Shared component already generic; blocker is deciding where the button surfaces (no detail page exists for Category/Tag today).
  - Caution: confirm backend's audit `service` enum actually includes "Category"/"Tag" before wiring.
- [ ] **Task 5 — Order Detail missing customer name/phone etc.** Not a backend gap — backend already returns these fields (confirmed via a migration from today); this repo's types are stale.
  - Straightforward: update `GetOrderResponse` type + `OrderCustomer.tsx` + `docs/backend/order/README.md`.
- [ ] **Task 6 — Checkout flow redesign (multi-step, editable order owner).** Phase A (multi-step UI, editable owner fields, now including address) is unblocked — backend added `shippingAddress` support 2026-07-22. Only payment method + auto-complete-on-transfer remains blocked.
  - [b] Payment blocked — see backend Task 3 (deferred as its own follow-up, not yet scoped).
- [ ] **Task 7 — Inventory/Warehouse/Stock/Notification pages "not integrated."** Mixed causes, not one bug: `/notifications` history page is a genuine unbuilt placeholder (unblocked fix); Warehouse/Inventory/Stock-Transaction lists are sourced from browser-local id tracking, not a real backend list (blocked on backend list endpoints, or accept as documented tradeoff); `.env.local` has all three env vars set to empty string, silently breaking SignalR + disabling dev mocks (unblocked, zero-risk fix).
  - Caution: don't conflate the three causes — they need different fixes.
- [b] **Task 8 — Users page: split into Admin/Normal User tabs.** Blocked — User service has no list/search endpoint at all; current page shows only the logged-in user's own profile, not "same-role users" as originally described. Needs a new backend task raised for a list/search endpoint before this can be built.
- [ ] **Task 9 — Product search missing "Clear filters."** Carried over from before today's list; still open, no blocker.

## Cross-repo dependencies

- Task 2 resolved via SimpleShop Task 1 (2026-07-22).
- Task 5 needs no backend work (already resolved server-side, now including `shippingAddress` too) — just catch up this repo's types/UI.
- Task 6's address blocker resolved via SimpleShop Task 3 (2026-07-22); payment remains blocked on a not-yet-scoped follow-up task.
- Task 8 needs a _new_ backend task (User list/search endpoint) that doesn't exist yet in either repo's tracker — raise it before starting.
