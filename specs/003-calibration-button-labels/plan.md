# Implementation Plan: Calibration Dialog Button Labels

**Branch**: `003-calibration-button-labels` | **Date**: 2026-10-06 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/003-calibration-button-labels/spec.md`

## Summary

The calibration dialog's confirm button currently always reads "OK". It must read "Next" on every step except the last, and "Save" only on the last step. The "is this the last step" decision is added to the existing calibration session in the core library (unit-testable); the dialog only binds the button text to it whenever the step changes.

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0-windows`)

**Primary Dependencies**: Windows Forms (existing); none added

**Storage**: N/A

**Testing**: xUnit tests in `tests/AudioMeter.Core.Tests` (existing `CalibrationSessionTests`); manual check of the dialog

**Target Platform**: Windows desktop

**Project Type**: desktop-app

**Performance Goals**: N/A

**Constraints**: Cancel behaviour/label unchanged; confirm logic unchanged

**Scale/Scope**: One property in core, one dialog class touched

## Constitution Check

- I. Windows Desktop Platform: PASS (Windows Forms, no new frameworks).
- II. UI/logic separation: PASS (last-step rule lives in `CalibrationSession`; the dialog only renders it).
- III. Measurement correctness: PASS (no measurement change).
- IV. Responsive/resilient lifecycle: PASS (no blocking work added).
- V. Automated quality: PASS (new core property gets unit tests; dialog label verified manually).

Re-check after design: no violations.

## Project Structure

### Documentation (this feature)

```text
specs/003-calibration-button-labels/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── calibration-dialog-ui.md
└── tasks.md   # created by /speckit-tasks
```

### Source Code (repository root)

```text
src/
├── AudioMeter.Core/Calibration/CalibrationSession.cs   # add IsLastStep
└── AudioMeter.App/Dialogs/CalibrationDialog.cs         # button text bound to step; instruction text

tests/
└── AudioMeter.Core.Tests/CalibrationSessionTests.cs    # IsLastStep tests
```

**Structure Decision**: Existing three-project layout; changes confined to the files above.

## Complexity Tracking

No constitution violations.
