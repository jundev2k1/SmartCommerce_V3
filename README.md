# SimpleShopUI

Admin dashboard frontend for the SimpleShopUI microservice platform. Next.js (App Router) + TypeScript + Tailwind + shadcn/ui, following a Feature-First architecture.

**Start here:** [`docs/README.md`](./docs/README.md) — the documentation index. It explains the architecture, conventions, and points to exactly which docs matter for the task at hand. [`docs/roadmap.md`](./docs/roadmap.md) tracks implementation phases.

## Getting started

```bash
yarn install
yarn dev
```

Open [http://localhost:3000](http://localhost:3000).

## Scripts

| Command                             | Purpose              |
| ----------------------------------- | -------------------- |
| `yarn dev`                          | Start the dev server |
| `yarn build`                        | Production build     |
| `yarn lint` / `yarn lint:fix`       | ESLint               |
| `yarn typecheck`                    | `tsc --noEmit`       |
| `yarn format` / `yarn format:check` | Prettier             |

A pre-commit hook (Husky + lint-staged) runs lint/format on staged files automatically.
