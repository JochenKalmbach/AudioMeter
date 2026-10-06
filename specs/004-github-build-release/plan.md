# Implementation Plan: Automated Build and Release Pipeline

**Branch**: `004-github-build-release` | **Date**: 2026-10-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/004-github-build-release/spec.md`

## Summary

Add two GitHub Actions workflows on Windows runners: a PR validation workflow (restore, build, run unit tests) triggered by pull requests to `main`, and a release workflow triggered by pushes to `main` that builds and tests, publishes the Release configuration of the WinForms app, zips it, uploads the ZIP as a run artifact, and creates a uniquely versioned GitHub Release with the ZIP attached.

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0-windows` app, `net10.0` core/tests)

**Primary Dependencies**: GitHub Actions (`actions/checkout`, `actions/setup-dotnet`, `actions/upload-artifact`), GitHub CLI (`gh`, preinstalled on runners) for release creation

**Storage**: N/A (run artifacts and GitHub Releases)

**Testing**: xUnit via `dotnet test` on `tests/AudioMeter.Core.Tests`

**Target Platform**: GitHub-hosted `windows-latest` runner; output runs on Windows

**Project Type**: desktop-app (CI/CD configuration only; no application code changes)

**Performance Goals**: ZIP available within 15 minutes of a `main` commit (SC-003)

**Constraints**: `TreatWarningsAsErrors` is on; release job needs `contents: write`; PR jobs from forks get read-only tokens and must never publish

**Scale/Scope**: 2 workflow files, README update

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- I. Windows Desktop Platform: PASS. Builds on Windows with the .NET 10 SDK; target unchanged.
- II. UI/audio separation: N/A, no application code changes.
- III. Measurement correctness: N/A, but unit tests (which cover measurement) become a merge-time gate.
- IV. Device lifecycle: N/A.
- V. Automated quality: PASS. Build and tests run on every PR and every `main` commit; this implements the constitution's quality gate.
- Technology constraints: PASS. No new application dependencies.

Post-design re-check: PASS, no violations.

## Project Structure

### Documentation (this feature)

```text
specs/004-github-build-release/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── workflows.md
├── checklists/
└── tasks.md             # created by /speckit-tasks
```

### Source Code (repository root)

```text
.github/
└── workflows/
    ├── pr-build.yml     # PR to main: build + test
    └── release.yml      # push to main: build + test + zip + release
README.md                # document pipeline and releases
```

**Structure Decision**: Only CI configuration is added; existing `src/` and `tests/` layout and `AudioMeter.sln` are used unchanged.

## Complexity Tracking

No constitution violations.
