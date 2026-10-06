# Implementation Plan: Calibration Input Level

**Branch**: `002-input-calibration` | **Date**: 2026-10-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-input-calibration/spec.md`

## Summary

Change only the calibration capture sequence to descend from 105 to 40 dBA; completed calibration tables remain in their existing ascending 40-to-105 dBA order so interpolation and persisted point semantics do not change. Add a 0–100 slider to the calibration window backed by the selected Windows capture endpoint's input volume, and associate each completed calibration with its input device and the level used to record it. Reflect changes from Windows Sound settings in the calibration window and apply slider changes to that same device setting. Restore a saved calibration's device and level before using that mapping at startup. Preserve the existing persisted calibration points and support old settings files.

## Technical Context

**Language/Version**: C# 14 on .NET 10 (`net10.0-windows`)

**Primary Dependencies**: NAudio 3.1.0 for WASAPI capture, endpoint enumeration, capture endpoint volume and notifications; Windows Forms controls

**Storage**: Existing JSON settings file at `%APPDATA%\AudioMeter\settings.json`; extend compatibly with the selected input-level and calibration-associated device/level metadata.

**Testing**: xUnit for core ordering, mapping, metadata validation, and settings compatibility; Windows/manual integration validation for endpoint volume changes, UI synchronization, and startup restoration.

**Target Platform**: Windows 10/11 desktop

**Project Type**: desktop-app

**Performance Goals**: Reflect endpoint volume changes in the calibration control within 1 second; retain the existing 0.5-second measurement cadence.

**Constraints**: Keep endpoint operations and capture lifecycle responsive; do not apply a calibration to a different device or input level; preserve old settings; report unsupported or unavailable endpoint-volume controls clearly.

**Scale/Scope**: Core calibration/session and settings behavior, selected capture-endpoint volume access and notifications, calibration dialog, startup restoration, and focused tests; no new project or external package.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status |
|-----------|------|--------|
| I. Windows Desktop Platform | Keep the feature on .NET 10 Windows Forms and supported Windows audio APIs. | Pass |
| II. UI/Audio separation | Keep calibration ordering and persisted data in Core; isolate endpoint volume operations in the app audio layer; forms coordinate controls and events. | Pass |
| III. Measurement correctness | Descend only during session capture; reverse captured points to the existing ascending table order before validation and interpolation. | Pass |
| IV. Responsive/resilient device lifecycle | Surface unavailable devices/volume controls, restore the endpoint before measurement, and safely handle endpoint changes/removal. | Pass |
| V. Automated quality/simplicity | Add focused core tests and settings backward-compatibility tests; manually validate Windows endpoint behavior. Reuse NAudio and current settings storage. | Pass |

Post-design re-check: Pass. The design adds no package, project, or unnecessary second settings surface; the selected endpoint's Windows volume remains the shared source of truth.

## Project Structure

### Documentation (this feature)

```text
specs/002-input-calibration/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── input-level-and-calibration.md
└── tasks.md                 # Created by /speckit-tasks
```

### Source Code (repository root)

```text
src/
├── AudioMeter.Core/
│   ├── Calibration/
│   │   ├── CalibrationSession.cs
│   │   └── CalibrationTable.cs
│   └── Settings/
│       ├── AppSettings.cs
│       └── SettingsStore.cs
└── AudioMeter.App/
    ├── MainForm.cs
    ├── Audio/
    │   ├── AudioCaptureService.cs
    │   ├── InputDeviceService.cs
    │   └── InputLevelService.cs
    └── Dialogs/
        ├── CalibrationDialog.cs

tests/
└── AudioMeter.Core.Tests/
    ├── CalibrationSessionTests.cs
    └── SettingsStoreTests.cs
```

**Structure Decision**: Extend the existing two-project plus test-project layout. Core owns order-independent calibration table representation and settings validation; the app audio layer owns endpoint volume access and notifications; the existing MainForm owns application lifecycle and routes endpoint changes to the calibration dialog, which is the application's input-level control.

## Complexity Tracking

No violations to justify.
