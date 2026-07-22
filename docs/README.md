# SimpleShopUI — Documentation Index

**Purpose:** Entry point to the documentation system. Explains how the docs are organized and how to find what's relevant to a given task without reading everything.

**Scope:** Meta-documentation only. No architectural decisions live here.

**Related documents:** [roadmap.md](./roadmap.md), all subdirectories below.

**When to read:** Start of every new session/task, before touching any other doc.

**When to ignore:** Never — this is always the first file to read.

---

## How this documentation is organized

Every doc (except templates and the changelog) opens with a header block:

```
Purpose / Scope / Related documents / When to read / When to ignore
```

Read the header first. If "When to ignore" matches your task, skip the rest of the file. This keeps future sessions from loading unnecessary context — only load what the current task actually touches.

## Directory map

| Directory                                   | Contents                                                                                         | Read when...                                                                                                    |
| ------------------------------------------- | ------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------- |
| [architecture/](./architecture/overview.md) | High-level structure, folder layout, tech stack                                                  | Starting any new feature, unsure where code should live                                                         |
| [conventions/](./conventions/naming.md)     | Naming, imports, feature boundaries                                                              | Creating new files/folders                                                                                      |
| [frontend/](./frontend/routing.md)          | Routing, feature module anatomy, UI/forms/theme/i18n strategy                                    | Building UI, forms, or routes                                                                                   |
| [state/](./state/overview.md)               | Zustand vs TanStack Query vs local state                                                         | Adding any stateful logic                                                                                       |
| [services/](./services/api-layer.md)        | Axios client, auth, error handling                                                               | Calling or integrating an API                                                                                   |
| [realtime/](./realtime/signalr-strategy.md) | SignalR hub/event strategy                                                                       | Building any realtime feature (notifications, live order updates)                                               |
| [backend/](./backend/README.md)             | Per-service backend contract docs, feature mapping, full coverage report                         | Calling any real backend endpoint, or checking whether a gap/ambiguity is already known                         |
| [api/](./api/README.md)                     | Superseded by `backend/` since Phase 1.5 — kept only for historical context                      | Essentially never; go to `backend/` instead                                                                     |
| [modules/](./modules/overview.md)           | Business module list, nav reservations, per-module docs                                          | Starting a specific business module's implementation phase                                                      |
| [decisions/](./decisions/template.md)       | ADRs — the "why" behind every architectural choice                                               | Questioning or extending an existing decision                                                                   |
| [changelog/](./changelog/CHANGELOG.md)      | Dated log of doc/architecture changes                                                            | Checking what changed recently                                                                                  |
| [backlog.md](./backlog.md)                  | Structured technical-debt/deferred-work list                                                     | Planning what to work on next, or before starting any new phase                                                 |
| [tasks/](./tasks/README.md)                 | Dated, per-task tracking (bugs/gaps/feature requests), one file per task, grouped by date folder | Starting a session — check [tasks/PROGRESS.md](./tasks/PROGRESS.md) for what's open before doing unrelated work |

## Roadmap

[roadmap.md](./roadmap.md) is the master guide for implementation order. Every phase lists exactly which docs to read and which to ignore — always check the current phase's entry there before starting work.
