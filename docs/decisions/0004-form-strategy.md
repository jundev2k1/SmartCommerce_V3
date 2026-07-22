# 0004 — Internal Form Wrapper Over Raw React Hook Form + Zod

**Status:** Accepted

**Date:** 2026-07-20

## Context

Nearly every module in this admin dashboard has at least one form (create/edit dialogs, login, checkout, filters). Used directly, React Hook Form's `useForm`/`Controller`/`zodResolver` boilerplate would be repeated with minor variations across dozens of forms, and any future change to that boilerplate (e.g. adding analytics on submit, or swapping validation libraries) would require touching every form.

## Decision

`src/shared/forms` wraps RHF + Zod behind a simplified API (`useAppForm`, `<Form>`, `<FormField>`). Business code defines a Zod schema and renders these wrapper components; it never calls RHF's `useForm`/`Controller`/`zodResolver` directly.

## Rationale

- Keeps every form's markup short and declarative, which matters when dozens of features each need 1-3 forms.
- Centralizes label/error rendering so every form looks and behaves consistently without each feature reinventing it.
- One layer to update if RHF or the validation library is ever upgraded or swapped.

## Alternatives considered

- **Use RHF/Zod directly per feature**: rejected — the boilerplate-repetition and consistency costs above, at this project's scale, outweigh the simplicity of "just use the library directly."
- **A heavier form-generation approach (schema-driven full-form renderer)**: rejected as premature — `<FormField>` composition is simpler to reason about per-form than a fully generic schema-to-form renderer, and the latter tends to fight custom layouts that admin CRUD forms often need.

## Consequences

- New form field types (e.g. a date-range picker) require adding a case to `<FormField>` and its underlying `shared/ui` input, not per-feature RHF wiring.
- See [frontend/forms.md](../frontend/forms.md) for the concrete API.

## Related documents

[frontend/forms.md](../frontend/forms.md), [frontend/ui-components.md](../frontend/ui-components.md)
