# Tasks: Calibration Dialog Button Labels

**Input**: Design documents from `/specs/003-calibration-button-labels/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/calibration-dialog-ui.md

**Tests**: Included for the core `IsLastStep` rule (constitution principle V); dialog label verified manually via quickstart.md.

## Format: `- [ ] [ID] [P?] [Story] Description`

## Phase 1: Setup

- [X] T001 Verify the baseline builds and tests pass with `dotnet test tests\AudioMeter.Core.Tests`

## Phase 2: Foundational

- [X] T002 Add derived property `public bool IsLastStep => StepIndex == StepCount - 1;` (summary comment: true when the current step is the final calibration point) to `src/AudioMeter.Core/Calibration/CalibrationSession.cs`

**Checkpoint**: `IsLastStep` available for the dialog.

## Phase 3: User Story 1 - Clear progression through calibration points (Priority: P1) 🎯 MVP

**Goal**: Confirm button reads "Next" on all steps except the last, and "Save" only on the last.

**Independent Test**: Step through a calibration and check the button label at each step (quickstart.md, Manual steps 2-4).

### Tests for User Story 1

- [X] T003 [P] [US1] Add xUnit test in `tests/AudioMeter.Core.Tests/CalibrationSessionTests.cs` that `IsLastStep` is false at the first step and after each non-final `Confirm`, and true only when `StepIndex == StepCount - 1`

### Implementation for User Story 1

- [X] T004 [US1] In `src/AudioMeter.App/Dialogs/CalibrationDialog.cs` rename the `_ok` button field's initial text from "OK" to "Next"
- [X] T005 [US1] In `ShowStep()` of `src/AudioMeter.App/Dialogs/CalibrationDialog.cs` set `_ok.Text = _session.IsLastStep ? "Save" : "Next";` so the label refreshes at construction and after each advance
- [X] T006 [US1] In `src/AudioMeter.App/Dialogs/CalibrationDialog.cs` change the instruction label text "press OK to record the measured input level" to refer to "Next" (and "Save" on the last step); ensure no "OK" label text remains (the `DialogResult.OK` code is unchanged)

**Checkpoint**: US1 fully functional and testable.

## Phase 4: User Story 2 - Label stays correct when the sequence is restarted (Priority: P2)

**Goal**: A freshly opened dialog (after an invalidated session is cancelled and calibration restarted) shows "Next" on step 1.

**Independent Test**: quickstart.md Manual step 5.

- [X] T007 [US2] Verify in `src/AudioMeter.App/Dialogs/CalibrationDialog.cs` that no code path leaves "Save" displayed after invalidation or reopening (a new dialog calls `ShowStep()` in its constructor); adjust only if a path is found

**Checkpoint**: Restart shows "Next" again.

## Phase 5: Polish & Cross-Cutting Concerns

- [X] T008 Build `AudioMeter.sln` and run `dotnet test tests\AudioMeter.Core.Tests`; all tests must pass
- [X] T009 Run the manual validation in `specs/003-calibration-button-labels/quickstart.md` (confirm Cancel label and behaviour unchanged)

## Dependencies & Execution Order

- T001 → T002 → (T003 [P] with T004-T006) → T007 → T008 → T009
- T004, T005, T006 edit the same file: run sequentially.
- T003 (tests file) can run in parallel with T004-T006 once T002 is done.
- US2 depends on US1's `ShowStep()` change.

## Parallel Example

After T002: T003 (tests) in parallel with T004-T006 (dialog).

## Implementation Strategy

- **MVP**: Phases 1-3 (User Story 1) deliver the requested behaviour.
- US2 is a verification step on top; Polish confirms build, tests, and manual check.
