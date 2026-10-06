# Tasks: Calibration Input Level

**Input**: Design documents from `/specs/002-input-calibration/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/input-level-and-calibration.md](./contracts/input-level-and-calibration.md), [quickstart.md](./quickstart.md)

**Tests**: Automated tests are included for changed core behavior and persisted settings, as required by the project constitution. Device and UI behavior requires focused Windows manual validation.

**Organization**: Tasks are grouped by the three independently testable user stories in the specification.

## Format: `- [ ] [ID] [P?] [Story?] Description`

- **[P]**: Can be completed in parallel with other marked tasks because files and prerequisites are independent.
- **[Story]**: User story covered by the task.
- Every task names the source or test file to change.

## Phase 1: Setup

**Purpose**: Reuse the existing solution, projects, and test infrastructure.

No project initialization is required. The feature uses the existing .NET 10 Core, WinForms app, and xUnit test projects; no package or project changes are planned.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Identify cross-story prerequisites.

No shared prerequisite blocks the independent core calibration-order story. The endpoint-volume service is introduced within User Story 2, where its first consumer is implemented.

**Checkpoint**: User Story 1 may start; User Stories 2 and 3 depend on the endpoint-volume service task in User Story 2.

---

## Phase 3: User Story 1 - Calibrate from the highest reference level (Priority: P1) 🎯 MVP

**Goal**: Capture all 14 targets starting at 105 dBA and descending to 40 dBA, while retaining the existing canonical ascending table representation and interpolation.

**Independent Test**: Run the core calibration session and verify the target sequence is 105, 100, …, 40 dBA; confirmation produces a valid table ordered 40, 45, …, 105 dBA with strictly increasing dBFS values.

### Tests for User Story 1

- [X] T001 [P] [US1] Add tests for descending targets, descending capture levels, completed ascending table points, and invalid non-decreasing captures in `tests/AudioMeter.Core.Tests/CalibrationSessionTests.cs`

### Implementation for User Story 1

- [X] T002 [US1] Change session target calculation to descend from 105 to 40 dBA and reverse captured points before table creation in `src/AudioMeter.Core/Calibration/CalibrationSession.cs` (depends on T001)
- [X] T003 [US1] Update calibration dialog's step guidance and invalid-order warning to reflect descending targets in `src/AudioMeter.App/Dialogs/CalibrationDialog.cs` (depends on T002)

**Checkpoint**: Calibration starts at 105 dBA, advances down by 5 dBA per confirmation, rejects invalid signal ordering, and still returns a canonical table accepted by existing interpolation.

---

## Phase 4: User Story 2 - Adjust input level during the first calibration point (Priority: P1)

**Goal**: Show the selected endpoint's 0–100 input level in the calibration window, allow adjustment only before confirming 105 dBA, and reflect external Windows Sound settings changes.

**Independent Test**: On Windows with an adjustable capture endpoint, change volume from the calibration slider and Windows Sound settings in turn and verify each direction is reflected. Confirm 105 dBA and verify the slider is read-only thereafter; change the endpoint volume externally and verify the session cannot continue with mixed-gain points.

### Tests for User Story 2

- [X] T004 [P] [US2] Add tests that input-level adjustment is allowed only before the first confirmation and that changing the level after the 105 dBA capture invalidates further confirmations in `tests/AudioMeter.Core.Tests/CalibrationSessionTests.cs`

### Implementation for User Story 2

- [X] T005 [US2] Implement selected capture endpoint volume read/write, 0–100 scalar conversion, change notifications, and deterministic endpoint disposal in `src/AudioMeter.App/Audio/InputLevelService.cs`
- [X] T006 [US2] Extend the calibration session to expose whether input-level adjustment is allowed, capture the level at the first 105 dBA confirmation, and reject further confirmations if the level changes afterward in `src/AudioMeter.Core/Calibration/CalibrationSession.cs` (depends on T003, T004)
- [X] T007 [US2] Add a 0–100 input-level slider and displayed value to the calibration dialog, enable editing only before confirming 105 dBA, and restore the prior endpoint value when cancelled before that confirmation in `src/AudioMeter.App/Dialogs/CalibrationDialog.cs` (depends on T005, T003, T004, T006)
- [X] T008 [US2] Wire the selected endpoint's input-level service into calibration launch, route endpoint notifications to the dialog on the UI thread, and refresh/recreate/dispose the endpoint service when input selection changes or the form closes in `src/AudioMeter.App/MainForm.cs` (depends on T005, T007)
- [X] T009 [US2] Display a clear restart-at-105 warning and prevent saving when Windows Sound settings changes the endpoint volume after the first point was recorded in `src/AudioMeter.App/Dialogs/CalibrationDialog.cs` (depends on T006, T007, T008)

**Checkpoint**: The slider is a live control for the selected Windows capture endpoint at step 105, becomes read-only after capture, and external changes cannot silently mix gains in one calibration.

---

## Phase 5: User Story 3 - Restore the calibration input configuration (Priority: P1)

**Goal**: Persist the completed calibration with its input device and 105 dBA input-level value, restore both before applying the calibration at startup, and flag later device/level changes as requiring recalibration.

**Independent Test**: Complete calibration for a device and level, restart the app, and verify the same device and exact rounded 0–100 level are applied before its calibration is activated. Change the device or level and verify the old mapping is no longer treated as valid. Load a legacy settings file and verify its calibration points remain usable without invented metadata.

### Tests for User Story 3

- [X] T010 [P] [US3] Add settings round-trip, 0–100 bounds, and pre-feature JSON compatibility tests for current and calibration-associated input-level metadata in `tests/AudioMeter.Core.Tests/SettingsStoreTests.cs`

### Implementation for User Story 3

- [X] T011 [US3] Add nullable current input level and calibration-associated endpoint ID and input-level fields to persisted settings in `src/AudioMeter.Core/Settings/AppSettings.cs` (depends on T010)
- [X] T012 [US3] Validate persisted input-level values as inclusive 0–100 while preserving older settings that omit the new fields in `src/AudioMeter.Core/Settings/SettingsStore.cs` (depends on T011)
- [X] T013 [US3] Save the selected endpoint and input-level value captured at 105 dBA together with completed calibration points in `src/AudioMeter.App/MainForm.cs` (depends on T007, T011, T012)
- [X] T014 [US3] Restore calibration-associated endpoint and level before activating its mapping and starting audio capture; visibly reject unavailable endpoints instead of applying calibration to a fallback device in `src/AudioMeter.App/MainForm.cs` (depends on T005, T011, T012)
- [X] T015 [US3] Mark calibration stale and require recalibration when the selected endpoint or its current input level differs from saved calibration metadata in `src/AudioMeter.App/MainForm.cs` (depends on T013, T014)

**Checkpoint**: New settings restore the calibration's device and gain before measurement; legacy settings remain readable; mismatches and unavailable devices are visible and do not silently reuse the old calibration.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Verify all stories together, including Windows endpoint lifecycle and feature documentation.

- [X] T016 Run the complete Core test project and resolve regressions from the feature in `tests/AudioMeter.Core.Tests/`
- [X] T017 Build the solution in `AudioMeter.sln`
- [ ] T018 Manually execute the Windows endpoint scenarios documented in `specs/002-input-calibration/quickstart.md` on an adjustable capture device

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No work is needed; existing projects and test infrastructure are reused.
- **Foundational (Phase 2)**: No shared infrastructure changes are needed before the user stories.
- **User Stories (Phase 3+)**: US1 can start after setup; US2 implements the endpoint-level service before the slider integration; US3 depends on that service and US2's calibration metadata.
- **Polish (Phase 6)**: Depends on the desired user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Starts after setup; calibration session order and canonical table behavior are independently testable.
- **User Story 2 (P1)**: Depends on US1's target/session behavior (T002) to lock the slider after the 105 dBA capture; T005 creates its endpoint-level service.
- **User Story 3 (P1)**: Depends on the endpoint-level service and calibration completion metadata produced by US2 (T005–T009); startup restore and stale-state behavior require the saved calibration model.

### Within Each User Story

- Tests for core logic and persistence precede their implementation.
- Session ordering must be implemented before dialog progression and slider lock behavior.
- The endpoint service (T005) must exist before the dialog or MainForm can share its value and notifications.
- Settings fields and validation precede saving and startup restoration.

### Parallel Opportunities

- T001 can be developed alongside T005 and T010 because the calibration session test, app audio service, and settings test are separate files.
- T004 edits the same test file as completed task T001; merge the test additions in that file before implementing T006.
- T016 and T017 are final validation tasks and depend on all implementation stories being integrated.

---

## Parallel Example: Independent Tasks

```text
Task: T001 Add descending calibration session tests in tests/AudioMeter.Core.Tests/CalibrationSessionTests.cs
Task: T005 Implement endpoint input-level service in src/AudioMeter.App/Audio/InputLevelService.cs
Task: T010 Add settings metadata and legacy compatibility tests in tests/AudioMeter.Core.Tests/SettingsStoreTests.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. T001–T003 (US1) are complete: descending calibration capture preserves the existing ascending table order.
2. To replay or verify the MVP, run `dotnet test tests\AudioMeter.Core.Tests` and confirm the calibration UI begins at 105 dBA.
3. Continue with endpoint volume synchronization and persistence in US2 and US3.

### Incremental Delivery

1. Deliver US1 and verify descending target order with a canonical completed table.
2. Deliver US2 and verify slider interaction, external updates, and session invalidation.
3. Deliver US3 and verify metadata persistence, startup restoration, compatibility, and stale-calibration handling.
4. Run the full quickstart scenarios on Windows with an adjustable input device.

## Notes

- `[P]` tasks edit distinct files and have no unmet prerequisites; do not run tasks against a shared file simultaneously.
- User story labels provide traceability to the corresponding scenario in `spec.md`.
- Windows endpoint volume notification and device-removal behavior require manual validation on hardware even when core tests pass.
- Do not commit changes unless separately requested.
