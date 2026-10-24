# Implementation Plan: Calibration Table Editor

**Branch**: `006-calibration-table-editor` | **Date**: 2026-10-24 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/006-calibration-table-editor/spec.md`

## Summary

Add a second calibration dialog in which the 14 dBFS values of the calibration table are edited directly in a grid, with a live dBFS / resulting-dBA readout that uses the edited table. Valid edits are applied to the meter immediately; Save persists and closes, Cancel restores the saved calibration. The "Calibrate…" menu item becomes a "Calibration" submenu with the existing step-by-step dialog and the new table dialog. Validation and parsing of the draft live in `AudioMeter.Core` (unit-testable); the dialog only wires the grid to it.

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0-windows`)

**Primary Dependencies**: Windows Forms (`DataGridView`), existing audio capture; no new packages

**Storage**: Existing JSON settings (`AppSettings.Calibration` points + `CalibrationInputDeviceId` / `CalibrationInputLevel`), via `SettingsStore`

**Testing**: Existing xUnit project `tests/AudioMeter.Core.Tests`; manual check for dialog/menu

**Target Platform**: Windows desktop

**Project Type**: desktop-app (`AudioMeter.App` + `AudioMeter.Core`)

**Performance Goals**: Readout and preview update within 1 s (SC-002); in practice per audio level callback

**Constraints**: No blocking work on the UI thread; dBFS cell range -120…0, strictly increasing

**Scale/Scope**: 14 rows, one new dialog, one new Core class, one menu change

## Constitution Check

| Principle | Status |
|-----------|--------|
| I. Windows Desktop Platform | Pass: Windows Forms only |
| II. Separation of UI and processing | Pass: draft parsing/validation/dBA in Core; form only coordinates |
| III. Measurement Correctness | Pass: invalid tables never reach the meter or storage; tests cover boundaries (-120, 0, equal neighbours, NaN, empty) |
| IV. Responsive device lifecycle | Pass: no new blocking work; reuses the existing level callback; dialog reference cleared on close, readout cleared on signal loss |
| V. Automated quality | Pass: new Core unit tests; manual quickstart for UI |

Post-design re-check: no violations; Complexity Tracking not needed.

## Project Structure

### Documentation (this feature)

```text
specs/006-calibration-table-editor/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── calibration-table-dialog-ui.md
└── tasks.md             # created by /speckit-tasks
```

### Source Code (repository root)

```text
src/
├── AudioMeter.Core/Calibration/
│   ├── CalibrationTable.cs               # existing; reused via TryCreate
│   └── CalibrationTableDraft.cs          # NEW: 14 editable cell texts, validation, TryBuild, live ToDba
├── AudioMeter.App/
│   ├── Dialogs/CalibrationDialog.cs      # existing; unchanged
│   ├── Dialogs/CalibrationTableDialog.cs # NEW: grid + live dBFS/dBA + Save/Cancel
│   └── MainForm.cs                       # menu → submenu; RunCalibrationTable(); level fan-out
tests/
└── AudioMeter.Core.Tests/CalibrationTableDraftTests.cs  # NEW
README.md                                 # document the new menu entry
```

**Structure Decision**: Reuse the existing two-project layout; the only Core addition is the draft/validation class, the rest is App UI.

## Complexity Tracking

None.
