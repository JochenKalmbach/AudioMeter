# Tasks: Always-on-Top SPL Meter

**Input**: Design documents from `/specs/001-spl-meter-overlay/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/ui-and-settings.md, quickstart.md

**Tests**: Included for the core library because the project constitution (principles III and V) requires automated tests for non-UI behavior. UI and device behavior is validated manually via quickstart.md.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: US1 live meter, US2 input selection, US2b color limits, US3 calibration, US4 history chart

## Phase 1: Setup

- [X] T001 Install the .NET 10 SDK (`dotnet --list-sdks` must list 10.x); the machine currently only has 8.0.300
- [X] T002 Create solution `AudioMeter.sln` with `src/AudioMeter.Core/AudioMeter.Core.csproj` (net10.0), `src/AudioMeter.App/AudioMeter.App.csproj` (net10.0-windows, UseWindowsForms) and `tests/AudioMeter.Core.Tests/AudioMeter.Core.Tests.csproj` (xUnit); add project references (App → Core, Tests → Core)
- [X] T003 [P] Add the NAudio package to `src/AudioMeter.App/AudioMeter.App.csproj`
- [X] T004 [P] Add `.gitignore` (bin/, obj/) and `Directory.Build.props` with nullable enabled and warnings as errors at repository root

---

## Phase 2: Foundational (blocks all user stories)

- [X] T005 [P] Create constants (ScaleMinDba 40, ScaleMaxDba 105, CalibrationStepDba 5, MeasurementIntervalSeconds 0.5, HistoryDuration 90 min, SilenceFloorDbfs -100) in `src/AudioMeter.Core/Constants.cs`
- [X] T006 [P] Create `Measurement` record (Timestamp DateTimeOffset, LevelDbfs double ≤ 0 floored at -100, Dba double? clamped 40–105, `null` when no signal) in `src/AudioMeter.Core/Levels/Measurement.cs`
- [X] T007 [P] Create `LevelCalculator` in `src/AudioMeter.Core/Levels/LevelCalculator.cs`: mix interleaved float channels to mono by averaging, accumulate samples for a 0.5 s window (counted in samples, not wall-clock), emit mean absolute level converted to dBFS (`20·log10`), floored at -100
- [X] T008 [P] Write tests in `tests/AudioMeter.Core.Tests/LevelCalculatorTests.cs`: silence → -100, full-scale clipping → 0 dBFS, stereo → mono mixing, window boundary after exactly sampleRate × 0.5 samples
- [X] T009 [P] Create `CalibrationTable` in `src/AudioMeter.Core/Calibration/CalibrationTable.cs`: 14 points (Dba 40…105 step 5, LevelDbfs strictly increasing with Dba), piecewise-linear dBFS→dBA conversion clamped to 40–105, default mapping (-90 dBFS → 40, 0 dBFS → 105) with `IsCalibrated = false`
- [X] T010 [P] Write tests in `tests/AudioMeter.Core.Tests/CalibrationTableTests.cs`: exact points return the target dBA, midpoint interpolation, clamping below 40 and above 105, rejection of non-increasing levels, default mapping flagged uncalibrated
- [X] T011 [P] Create `LevelZones` in `src/AudioMeter.Core/Levels/LevelZones.cs`: GreenYellowLimit (default 70, 40 ≤ value < YellowRedLimit), YellowRedLimit (default 85, GreenYellowLimit < value ≤ 105); zone of v is green if v ≤ GreenYellowLimit, yellow if v ≤ YellowRedLimit, otherwise red; validation method rejecting invalid values
- [X] T012 [P] Write tests in `tests/AudioMeter.Core.Tests/LevelZonesTests.cs`: boundary values (exactly at each limit), defaults, invalid ranges rejected (outside 40–105, equal or reversed limits)
- [X] T013 [P] Create `AppSettings` and `SettingsStore` in `src/AudioMeter.Core/Settings/` per contracts/ui-and-settings.md: JSON at `%APPDATA%\AudioMeter\settings.json` (path injectable for tests), atomic write (temp file then replace), missing or corrupt file → defaults, calibration ignored unless exactly 14 strictly increasing points
- [X] T014 [P] Write tests in `tests/AudioMeter.Core.Tests/SettingsStoreTests.cs`: round trip, missing file, corrupt JSON, invalid calibration ignored, invalid limits fall back to defaults

**Checkpoint**: Core logic compiles and `dotnet test tests\AudioMeter.Core.Tests` passes.

---

## Phase 3: User Story 1 - Live sound level display (P1) 🎯 MVP

**Goal**: Always-on-top, resizable window showing the live three-zone meter with the dBA value every 0.5 s.

**Independent Test**: quickstart scenario 1 and 6 (device loss) with default input and default mapping.

- [X] T015 [P] [US1] Create `AudioCaptureService` in `src/AudioMeter.App/Audio/AudioCaptureService.cs`: WASAPI capture of the default input on a non-UI thread via NAudio, feeds `LevelCalculator`, raises a `MeasurementReady` event every 0.5 s, raises a device-lost/stopped event, disposes capture on stop
- [X] T016 [P] [US1] Create `MeterControl` in `src/AudioMeter.App/Controls/MeterControl.cs`: double-buffered, linear 40–105 dBA bar with zone widths proportional to the limits, fill in the current zone color, centered value text "NN dBA", "No signal" state, "Uncalibrated" indicator
- [X] T017 [US1] Create `MainForm` in `src/AudioMeter.App/MainForm.cs`: `TopMost = true`, resizable with minimum size 320 × 240, hosts `MeterControl`, marshals measurements to the UI thread, converts via `CalibrationTable`, shows the no-signal state on capture failure with a clear message (FR-014), restores/saves window bounds via `SettingsStore`
- [X] T018 [US1] Create `Program.cs` in `src/AudioMeter.App/Program.cs` with application bootstrap, loading settings and showing `MainForm`
- [X] T019 [US1] Add a Settings `MenuStrip` skeleton to `MainForm` (items filled in by later stories) in `src/AudioMeter.App/MainForm.cs`

**Checkpoint**: App runs, stays on top, shows live value and colors.

---

## Phase 4: User Story 2 - Choose the audio input (P2)

**Goal**: Select the audio input from the menu; choice persists.

**Independent Test**: quickstart scenario 2.

- [X] T020 [P] [US2] Create `InputDeviceService` in `src/AudioMeter.App/Audio/InputDeviceService.cs`: enumerate active capture endpoints (Id, Name, IsDefault); resolve a stored endpoint ID, falling back to the default device with a notice
- [X] T021 [US2] Add *Settings → Audio input* radio-checked submenu to `src/AudioMeter.App/MainForm.cs`; switching stops and disposes the current capture, starts the new one without restarting the app
- [X] T022 [US2] Persist `InputDeviceId` and restore it at startup in `src/AudioMeter.App/MainForm.cs`

---

## Phase 5: User Story 2b - Adjust color limits (P2)

**Goal**: Edit the green/yellow and yellow/red limits via the menu; applied immediately and persisted.

**Independent Test**: quickstart scenario 3.

- [X] T023 [P] [US2b] Create `LimitsDialog` in `src/AudioMeter.App/Dialogs/LimitsDialog.cs`: two numeric fields, OK/Cancel, uses `LevelZones` validation (outside 40–105 or not strictly ascending → message, dialog stays open, prior limits unchanged)
- [X] T024 [US2b] Add *Settings → Color limits…* to `src/AudioMeter.App/MainForm.cs`, apply to `MeterControl` immediately and persist via `SettingsStore`

---

## Phase 6: User Story 3 - Calibrate dBA mapping (P2)

**Goal**: Interactive 14-step calibration (40…105 dBA, 5 dBA steps) producing a persisted mapping.

**Independent Test**: quickstart scenario 4.

- [X] T025 [P] [US3] Create `CalibrationSession` in `src/AudioMeter.Core/Calibration/CalibrationSession.cs`: steps 0…13, `Confirm(currentLevelDbfs)` records and advances, rejects a level not strictly above the previous step's (stays on step, returns warning), final confirm produces a `CalibrationTable`, cancel discards
- [X] T026 [P] [US3] Write tests in `tests/AudioMeter.Core.Tests/CalibrationSessionTests.cs`: 14 steps in order, non-increasing level rejected without advancing, completion yields a calibrated table, cancel leaves nothing
- [X] T027 [US3] Create `CalibrationDialog` in `src/AudioMeter.App/Dialogs/CalibrationDialog.cs`: large target dBA, live measured level (dBFS), "n of 14", OK and Cancel, warning on non-increasing level (FR-016)
- [X] T028 [US3] Add *Settings → Calibrate…* to `src/AudioMeter.App/MainForm.cs`: feed live levels to the dialog, on completion save to `SettingsStore` and apply immediately, cancel keeps the old mapping (FR-012)

---

## Phase 7: User Story 4 - 90-minute history chart (P3)

**Goal**: Bottom chart of dBA values over the last 90 minutes.

**Independent Test**: quickstart scenario 5.

- [X] T029 [P] [US4] Create `MeasurementHistory` in `src/AudioMeter.Core/History/MeasurementHistory.cs`: ordered measurements, prunes entries older than 90 minutes on add, `null` Dba entries represent gaps
- [X] T030 [P] [US4] Write tests in `tests/AudioMeter.Core.Tests/MeasurementHistoryTests.cs`: pruning at 90 minutes, ordering, gap entries kept, bounded count (about 10,800 at 2 Hz)
- [X] T031 [US4] Create `HistoryChartControl` in `src/AudioMeter.App/Controls/HistoryChartControl.cs`: double-buffered, right edge = now over 90 minutes, vertical axis 40–105 dBA, zone limit lines, gaps as line breaks, scales with window size
- [X] T032 [US4] Add the chart below the meter in `src/AudioMeter.App/MainForm.cs`, add a measurement per tick (gap entries on no signal), and layout meter above chart so both scale on resize

---

## Phase 8: Polish & Cross-Cutting

- [X] T033 Verify clean shutdown in `src/AudioMeter.App/MainForm.cs`: capture disposed, settings flushed, no UI-thread blocking in capture paths (constitution principle IV)
- [X] T034 Run all quickstart.md scenarios manually, plus an 8-hour soak run (SC-007), and record any deviations
- [X] T035 Run `dotnet build AudioMeter.sln` and `dotnet test` and fix warnings

---

## Dependencies & Execution Order

- Phase 1 → Phase 2 → user stories → Phase 8.
- US1 depends only on Phase 2. US2, US2b, US3, US4 each depend on US1 (they extend `MainForm`) but not on each other.
- Within a story: Core logic and tests before UI wiring; `MainForm.cs` edits are sequential (same file).
- Phase 2 tasks T005–T014 are largely parallel (separate files); each test task follows its implementation task.

## Parallel Examples

- Phase 2: T005, T006, T007, T009, T011, T013 together, then their test tasks.
- US1: T015 and T016 together, then T017.
- After US1: T020, T023, T025/T026, T029/T030 can start in parallel.

## Implementation Strategy

1. **MVP**: Phases 1–3 (US1) — live always-on-top meter with default mapping.
2. Add US3 (calibration) next so values become meaningful, then US2 and US2b.
3. Add US4 (history chart), then Phase 8.
