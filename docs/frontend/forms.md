# Form Strategy

**Purpose:** Define the internal form API so business code never touches React Hook Form or Zod's raw APIs directly.

**Scope:** Form building/validation only. General UI wrapping strategy is in [ui-components.md](./ui-components.md).

**Related documents:** [ui-components.md](./ui-components.md), [conventions/naming.md](../conventions/naming.md), [decisions/0004-form-strategy.md](../decisions/0004-form-strategy.md)

**When to read:** Building any form (create/edit dialogs, login, checkout, filters).

**When to ignore:** No form work involved in the current task.

---

## The rule

Business code defines a Zod schema and renders `shared/forms` components — it never calls `useForm`, `Controller`, or `zodResolver` directly.

```ts
// features/products/product.schema.ts
export const productSchema = z.object({
  name: z.string().min(1),
  price: z.coerce.number().positive(),
});
```

```tsx
// features/products/components/ProductForm.tsx
import { useAppForm, Form, FormField } from '@/shared/forms';
import { productSchema } from '../product.schema';

function ProductForm() {
  const form = useAppForm({ schema: productSchema, defaultValues: { name: '', price: 0 } });

  return (
    <Form form={form} onSubmit={handleSubmit}>
      <FormField name="name" label="Name" />
      <FormField name="price" label="Price" type="number" />
    </Form>
  );
}
```

## What `shared/forms` provides

- **`useAppForm`** — wraps RHF's `useForm` + `zodResolver`, taking a Zod schema directly instead of separate `resolver`/type-generic setup.
- **`<Form>`** — wraps RHF's `FormProvider` + a native `<form onSubmit>` wired to RHF's `handleSubmit`.
- **`<FormField>`** — wraps RHF's `Controller` + the matching `shared/ui` input (text, number, select, date, etc., chosen via a `type`/`as` prop), plus label and error message rendering in one place.

Rationale in [decisions/0004-form-strategy.md](../decisions/0004-form-strategy.md): this keeps every form's markup declarative and short, and means an RHF version upgrade or validation-library swap touches one wrapper layer instead of every feature.

## Submission & mutations

Form `onSubmit` handlers call a feature's TanStack Query mutation hook (from `api/*.queries.ts`, see [state/query-strategy.md](../state/query-strategy.md)), not the raw `.service.ts` function directly — this keeps loading/error state and cache invalidation consistent with the rest of the state strategy.
