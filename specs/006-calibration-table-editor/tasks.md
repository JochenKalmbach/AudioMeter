# Tasks: Calibration Table Editor

**Input**: Design documents from `/specs/006-calibration-table-editor/`
**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/calibration-table-dialog-ui.md](./contracts/calibration-table-dialog-ui.md)

**Tests**: Core unit tests are included (constitution principles III and V require them for measurement/validation behaviour). UI is verified manually via [quickstart.md](./quickstart.md).

## Format: `- [ ] [ID] [P?] [Story] Description`

## Phase 1: Setup

No new projects or dependencies are needed (existing `AudioMeter.Core`, `AudioMeter.App`, `AudioMeter.Core.Tests`).

- [X] T001 Build the solution and run existing tests to record a green baseline (`dotnet build; dotnet test`) from the repo root

---

## Phase 2: Foundational (blocking)

**Purpose**: Core draft/validation logic used by all stories.

- [X] T002 Create `CalibrationTableDraft` in `src/AudioMeter.Core/Calibration/CalibrationTableDraft.cs` with: 14 rows (fixed `Dba` 40…105 step 5 plus editable cell `Text`); `FromTable(CalibrationTable)`; `SetCell(int index, string text)`; `RowError(int index)`; `IsValid`; `TryBuild(out CalibrationTable?)` via existing `CalibrationTable.TryCreate`; `ToDba(double dbfs)` returning `double?` (null when invalid). Validation rules verbatim: cell parses as a finite number (decimal point or comma accepted per culture); -120 ≤ value ≤ 0; value strictly greater than the previous row's value (the row breaking the order is the one flagged).
- [X] T003 [P] Create `tests/AudioMeter.Core.Tests/CalibrationTableDraftTests.cs` covering: `FromTable(Default)` is valid and round-trips; -120 and 0 accepted; -120.1 and 0.1 rejected; empty, text and NaN rejected; equal and decreasing neighbours flag the offending row; `TryBuild` result matches `CalibrationTable.TryCreate`; `ToDba` equals table `ToDba` when valid and is null when invalid; comma decimal accepted.
- [X] T004 Run `dotnet test tests/AudioMeter.Core.Tests --filter CalibrationTableDraft` and fix until green

**Checkpoint**: Core logic ready.

---

## Phase 3: User Story 1 - Edit the calibration table directly (P1) 🎯 MVP

**Goal**: Edit the 14 dBFS values in a grid; valid edits affect the meter immediately; Save persists and closes; Cancel reverts.

**Independent Test**: Quickstart steps 3, 5, 6 (needs the menu entry from T011, or temporarily call the method).

- [X] T005 [US1] Create `src/AudioMeter.App/Dialogs/CalibrationTableDialog.cs`: Windows Forms dialog with a `DataGridView` (read-only "dBA" column; editable "dBFS" column; no add/remove rows), initialised from a `CalibrationTableDraft` built from the active table, plus Save and Cancel buttons. Logic stays in the draft; the form only coordinates.
- [X] T006 [US1] In `CalibrationTableDialog.cs`, on each committed cell edit call `draft.SetCell`, show/clear per-cell error text from `RowError`, enable Save only when `IsValid`, and raise a `TableChanged` event carrying the built `CalibrationTable` only when the draft is valid (invalid drafts raise nothing).
- [X] T007 [US1] In `CalibrationTableDialog.cs`, expose `Result` (the valid `CalibrationTable` on Save; DialogResult.OK) and make Cancel return DialogResult.Cancel.
- [X] T008 [US1] In `src/AudioMeter.App/MainForm.cs`, add `RunCalibrationTable()`: open the dialog with the active `_table`; subscribe to `TableChanged` to set `_table` and call `ApplyZonesAndCalibration()` (live preview); on OK set `_storedCalibration`, `_settings.Calibration`, and, when `_inputLevelService` is available, `_settings.InputDeviceId`, `CalibrationInputDeviceId` and `CalibrationInputLevel = inputLevelService.CurrentLevel` (otherwise leave the metadata null); on Cancel discard; then call `RefreshCalibrationValidity()` and `SaveSettings()` (Cancel: restore only, no persist). Wrap in the same exception handling pattern as `RunCalibration()` and clear the dialog reference in `finally`.

**Checkpoint**: Table editing works end-to-end once the menu is wired.

---

## Phase 4: User Story 2 - Live dBFS and resulting dBA (P1)

**Goal**: The dialog shows the live input level and the dBA from the edited (unsaved) table.

**Independent Test**: Quickstart step 4 and step 5 (n/a when invalid).

- [X] T009 [US2] In `CalibrationTableDialog.cs`, add a label and `UpdateLevel(double dbfs)` / `ClearLevel()` showing `Measured input: x.x dBFS → y.y dBA` using `draft.ToDba`, and `→ n/a (table invalid)` when it returns null; keep the last level so the label refreshes immediately after each cell edit.
- [X] T010 [US2] In `src/AudioMeter.App/MainForm.cs`, hold the open table dialog in a field (alongside `_calibrationDialog`), forward `OnLevel` values via `UpdateLevel`, and call `ClearLevel()` where the existing code clears `_calibrationDialog` on signal loss.

**Checkpoint**: Live readout works.

---

## Phase 5: User Story 3 - Calibration submenu (P2)

**Goal**: "Calibration" menu with both methods.

**Independent Test**: Quickstart steps 2 and 7.

- [X] T011 [US3] In `src/AudioMeter.App/MainForm.cs` (constructor, around the existing `calibrate` item), replace "C&alibrate…" with a "C&alibration" `ToolStripMenuItem` with two children: "&Step-by-step…" → `RunCalibration()` (unchanged) and "&Edit table…" → `RunCalibrationTable()`; update the `settingsMenu.DropDownItems.AddRange` call accordingly.
- [X] T012 [US3] Verify the step-by-step dialog behaviour is unchanged (no edits to `src/AudioMeter.App/Dialogs/CalibrationDialog.cs`) and that a table saved in either dialog is reflected when opening the other (FR-010).

---

## Phase 6: Polish

- [X] T013 [P] Update `README.md` to describe the Calibration submenu and the table editor.
- [X] T014 Run `dotnet build` and `dotnet test` for the whole solution; fix any warnings/failures.
- [X] T015 Execute the manual steps in [quickstart.md](./quickstart.md) and fix deviations.

---

## Dependencies & Execution Order

- T001 → Phase 2 (T002 → T003/T004) → US1 (T005 → T006 → T007 → T008) → US2 (T009 → T010) → US3 (T011, T012) → Polish.
- US2 depends on the dialog from US1 (same file); US3 depends only on T008 existing. The menu (T011) is needed to exercise US1/US2 manually.
- T003 and T013 are parallelizable (different files).

## Implementation Strategy

- **MVP**: Phases 1–3 plus T011 (menu entry) gives editable, savable table calibration.
- Then add US2 (live readout), then finish US3 verification and polish.
