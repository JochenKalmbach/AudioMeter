# Implementation Plan: Weekly Automated Dependency Updates (Renovate)

**Branch**: `005-renovate-weekly-updates` | **Date**: 2026-10-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/005-renovate-weekly-updates/spec.md`

## Summary

Add a Renovate configuration file to the repository so the hosted Renovate GitHub App checks NuGet packages and GitHub Actions weekly (Monday before 06:00 Europe/Berlin), opens pull requests automatically, groups minor/patch updates, separates major updates, caps open PRs at 5, and never auto-merges. The existing PR Build workflow validates each PR. A short README section documents the schedule.

## Technical Context

**Language/Version**: JSON (Renovate config); app is C#/.NET 10 (unchanged)

**Primary Dependencies**: Renovate (hosted GitHub App); managers `nuget` and `github-actions`

**Storage**: N/A

**Testing**: Config validation with `renovate-config-validator`; manual check of the first Renovate run (onboarding/dependency dashboard)

**Target Platform**: GitHub repository JochenKalmbach/AudioMeter (default branch `main`)

**Project Type**: Desktop app (Windows Forms); this feature is repository configuration only

**Performance Goals**: N/A

**Constraints**: No auto-merge; PRs target `main` so `.github/workflows/pr-build.yml` runs; no secrets in repo

**Scale/Scope**: 3 projects (`AudioMeter.App`, `AudioMeter.Core`, `AudioMeter.Core.Tests`), 2 workflows

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- I. Windows Desktop Platform: no change to target framework. Renovate only changes package/action versions, not the `net10.0-windows` target, and all changes need human review. PASS
- II/III/IV: No application code or measurement changes. PASS (N/A)
- V. Automated Quality: Every PR passes the existing build+test workflow; the config is validated. PASS
- Technology constraints: dependencies limited to what is needed; a config file adds no runtime dependency. PASS
- Workflow gates: no merge without review. PASS

Post-design re-check: PASS, no violations.

## Project Structure

### Documentation (this feature)

```text
specs/005-renovate-weekly-updates/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── renovate-config.md
└── tasks.md             # created by /speckit-tasks
```

### Source Code (repository root)

```text
.github/
└── renovate.json        # new: Renovate configuration
README.md                # updated: short "Dependency updates" section
```

**Structure Decision**: Configuration-only change; no source or test projects are modified.

