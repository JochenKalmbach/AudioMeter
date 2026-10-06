---

description: "Task list for Automated Build and Release Pipeline"
---

# Tasks: Automated Build and Release Pipeline

**Input**: Design documents from `/specs/004-github-build-release/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/workflows.md, quickstart.md

**Tests**: No new test code requested; the existing xUnit project `tests/AudioMeter.Core.Tests` is executed by the workflows. Validation is via quickstart.md.

## Format: `[ID] [P?] [Story] Description`

## Phase 1: Setup

- [X] T001 Create the `.github/workflows/` directory at the repository root

---

## Phase 2: Foundational

No blocking prerequisites beyond Phase 1; the existing `AudioMeter.sln` builds all projects and the tests.

- [X] T002 Confirm locally that `dotnet build AudioMeter.sln -c Release` and `dotnet test AudioMeter.sln -c Release --no-build` succeed (warnings are errors per `Directory.Build.props`)

---

## Phase 3: User Story 1 - Pull request validation (Priority: P1) 🎯 MVP

**Goal**: Every PR targeting `main` is built and its unit tests are run, with a pass/fail check.

**Independent Test**: Open a PR to `main`; the `build` check runs and passes; a PR with a failing test or compile error fails the check (quickstart steps 1-2).

- [X] T003 [US1] Create `.github/workflows/pr-build.yml` with trigger `pull_request` on `branches: [main]` (types opened, synchronize, reopened) and `permissions: contents: read`
- [X] T004 [US1] In `.github/workflows/pr-build.yml` add job `build` on `windows-latest` with steps: `actions/checkout`, `actions/setup-dotnet` (`10.0.x`), `dotnet restore AudioMeter.sln`, `dotnet build AudioMeter.sln -c Release --no-restore`, `dotnet test AudioMeter.sln -c Release --no-build --logger "trx"` (fails the check if any test fails or none run)
- [X] T005 [US1] In `.github/workflows/pr-build.yml` add a step uploading TRX test results as an artifact (`actions/upload-artifact`, `if: always()`) so failed test details are accessible

**Checkpoint**: PR validation works independently.

---

## Phase 4: User Story 2 - Release package on every main commit (Priority: P2)

**Goal**: Each `main` commit is built, tested, and packaged into a ZIP kept as a run artifact.

**Independent Test**: Push/merge to `main`; a ZIP artifact `AudioMeter-v1.0.N-win` appears and runs on Windows (quickstart steps 3-4).

- [X] T006 [US2] Create `.github/workflows/release.yml` with trigger `push` on `branches: [main]`, `permissions: contents: write`, and `concurrency: group: release, cancel-in-progress: false`
- [X] T007 [US2] In `.github/workflows/release.yml` add job `release` on `windows-latest` with checkout, setup-dotnet `10.0.x`, restore, Release build and `dotnet test` steps (same as T004) so failures stop the run before packaging
- [X] T008 [US2] In `.github/workflows/release.yml` add a step setting version `v1.0.${{ github.run_number }}` and a step running `dotnet publish src/AudioMeter.App/AudioMeter.App.csproj -c Release --no-build -o publish`
- [X] T009 [US2] In `.github/workflows/release.yml` add a PowerShell step `Compress-Archive -Path publish\* -DestinationPath AudioMeter-<version>-win.zip` and an `actions/upload-artifact` step for the ZIP

**Checkpoint**: ZIP produced and downloadable per `main` commit.

---

## Phase 5: User Story 3 - Automatic release publishing (Priority: P3)

**Goal**: Each successful `main` run publishes a uniquely versioned release with the ZIP attached.

**Independent Test**: After a `main` commit, release `v1.0.N` exists with the ZIP; two quick commits give two distinct releases (quickstart steps 3, 5).

- [X] T010 [US3] In `.github/workflows/release.yml` add final step running `gh release create <version> <zip> --target ${{ github.sha }} --title <version> --generate-notes` with `GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}`, placed after the artifact upload so failures leave the artifact available

**Checkpoint**: Full pipeline complete.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T011 [P] Update `README.md` with a section describing the PR build check, the release workflow, version scheme, and where to download releases
- [X] T012 [P] Validate both workflow files are well-formed YAML (e.g. parse locally) and that action versions are current
- [ ] T013 Run the quickstart.md validation scenarios after merging; recommend a branch protection rule requiring the `build` check on `main`

---

## Dependencies & Execution Order

- Phase 1 -> Phase 2 -> US1 (P1) -> US2 (P2) -> US3 (P3) -> Polish.
- US1 is independent of US2/US3 (different file). US3 extends `release.yml` and so depends on US2.
- T011 and T012 can run in parallel with each other once workflows exist.

## Parallel Example

After T002: T003-T005 (`pr-build.yml`) can proceed in parallel with T006-T009 (`release.yml`) since they touch different files.

## Implementation Strategy

- **MVP**: Phase 1-3 (PR validation), merge and verify.
- **Incremental**: add release packaging (US2), then publishing (US3), then README.

