# Tasks

**Purpose:** Dated, per-task tracking for cross-cutting work items (bugs, gaps, feature requests) — often raised from a session in the sibling backend repo (`SimpleShop`) without this repo's context loaded, or from manual QA against a real running backend.

**Scope:** Task tracking only. Not architectural decisions (see [decisions/](../decisions/template.md)), not the technical-debt list (see [backlog.md](../backlog.md) — a _task_ here is dated and gets closed out; a _backlog_ item is durable, undated, known debt), not phase planning (see [roadmap.md](../roadmap.md)).

**Related documents:** [../backlog.md](../backlog.md), [../roadmap.md](../roadmap.md), the sibling backend repo's own `docs/tasks/` (`SimpleShop/docs/tasks/`) for the other side of any cross-repo task.

**When to read:** Starting a new session, to see what's currently open. Before closing out debt in `backlog.md`, check here first in case it's already tracked as an active task.

**When to ignore:** A task fully resolved and merged — delete the file's relevance is over, but per convention below, don't delete the folder; just mark it closed in the top-level `PROGRESS.md`.

---

## Structure

```
docs/tasks/
  README.md              <- this file
  PROGRESS.md            <- overall status across every date folder; short, one line per open task
  2026-07-22/
    Task1_<slug>.md       <- one file per task, numbered in the order raised that day
    Task2_<slug>.md
    ...
    PROGRESS.md          <- todo-list status for just this date's tasks (status / issue / caution)
  2026-07-23/
    ...
```

## Conventions

- One task = one file. Filename: `Task<N>_<kebab-slug>.md`; `N` restarts at 1 in each new date folder.
- A task file states: the report as given (verbatim request/response payloads if provided), the grounded current-state investigation (file:line citations, not paraphrase), and an explicit "open questions" section rather than papering over gaps. It does not need a finished fix — investigation-only is a complete, valid task file.
- Cross-reference the paired backend task (`SimpleShop/docs/tasks/<date>/TaskN_*.md`) when a task originates from or affects the backend — both repos are siblings under `workspace/projects/`.
- Update the date folder's `PROGRESS.md` whenever a task's status changes. Keep it a todo list, not prose.
- Update this folder's top-level `PROGRESS.md` at the end of a session: one line per still-open task across all dates. No per-task detail — that belongs in the task file itself.
- Once every task in a date folder is Done, leave the folder in place as a historical record; mark it closed in the top-level `PROGRESS.md` rather than deleting it.
