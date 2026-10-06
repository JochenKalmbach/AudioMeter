---

description: "Task list for weekly Renovate dependency updates"
---

# Tasks: Weekly Automated Dependency Updates (Renovate)

**Input**: Design documents from `specs/005-renovate-weekly-updates/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/renovate-config.md, quickstart.md

**Tests**: No test projects requested; validation uses the Renovate config validator and the quickstart.

## Format: `[ID] [P?] [Story] Description`

## Phase 1: Setup

- [X] T001 Create `.github/renovate.json` with `$schema`, `extends: ["config:recommended"]`, and `labels: ["dependencies"]` per contracts/renovate-config.md

---

## Phase 2: Foundational

- [X] T002 In `.github/renovate.json` add `enabledManagers: ["nuget", "github-actions"]` and `automerge: false`

**Checkpoint**: Valid base config exists; nothing merges automatically.

---

## Phase 3: User Story 1 - Weekly dependency update proposals (Priority: P1) 🎯 MVP

**Goal**: Renovate checks weekly and opens pull requests automatically, without duplicates.

**Independent Test**: Quickstart steps 1-3: validator passes, and a PR is opened for an outdated dependency.

- [X] T003 [US1] In `.github/renovate.json` add `schedule: ["before 6am on monday"]` and `timezone: "Europe/Berlin"`
- [X] T004 [US1] In `.github/renovate.json` add `prConcurrentLimit: 5`
- [ ] T005 [US1] (Node/npx not installed locally; JSON syntax checked only) Run `npx --yes --package renovate renovate-config-validator .github/renovate.json` and fix any errors

---

## Phase 4: User Story 2 - Existing build checks validate proposals (Priority: P2)

**Goal**: Every Renovate PR runs the existing PR Build.

**Independent Test**: Quickstart step 4.

- [X] T006 [US2] Confirm `.github/renovate.json` does not set a `baseBranches` other than `main` and that `.github/workflows/pr-build.yml` triggers on pull requests to `main`; adjust the config if not

---

## Phase 5: User Story 3 - Cover all dependency kinds, grouped (Priority: P3)

**Goal**: NuGet packages and workflow actions are covered; minor/patch grouped, majors separate.

**Independent Test**: Quickstart step 5.

- [X] T007 [US3] In `.github/renovate.json` add `packageRules`: group `minor` and `patch` as "all non-major dependencies"; label `major` updates `major` and keep them in separate PRs
- [ ] T008 [US3] (blocked as T005) Re-run the config validator on `.github/renovate.json` (same command as T005)

---

## Phase 6: Polish & Cross-Cutting

- [X] T009 [P] Add a "Dependency updates" section to `README.md` explaining the Monday schedule, PR-only behavior (no auto-merge), the one-time Renovate GitHub App install, and how to change or pause the schedule
- [ ] T010 Walk through `specs/005-renovate-weekly-updates/quickstart.md` after merging to `main` and the owner installing the app (dashboard issue appears, PR opens, PR Build runs)

---

## Dependencies & Execution Order

- T001 → T002 → T003/T004 → T005; T006 after T005; T007 → T008 (same file, sequential)
- T009 is parallel with any config task (different file)
- T010 needs everything merged plus the app installed (manual, owner action)

## Implementation Strategy

- MVP: Phases 1-3 (config with weekly schedule opening PRs).
- Then US2 check, US3 grouping, and README.

