# Task 5: Customer delete button is permanently disabled

**Status:** Blocked — depends on SimpleShop `docs/tasks/2026-07-27/Task1_customer-delete-endpoint-missing.md`.

## Source

Full-system business-requirements audit, 2026-07-27.

## Current state

`UsersPage.tsx:105-109` renders a disabled `DeleteButton` with tooltip text key `deleteNotAvailable`, because no backend `DELETE /profiles/{userId}` endpoint exists yet.

## Suggested acceptance criteria (once unblocked)

- Enable the delete action with a confirm dialog, consistent with Product's existing delete UX.

**Cross-ref:** SimpleShop `docs/tasks/2026-07-27/Task1_customer-delete-endpoint-missing.md`.
