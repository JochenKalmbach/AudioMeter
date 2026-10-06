# Implementation Plan: Always-on-Top SPL Meter

**Branch**: `001-spl-meter-overlay` | **Date**: 2026-10-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-spl-meter-overlay/spec.md`

## Summary

A single-window Windows Forms application (always on top, resizable) that captures the selected
audio input, computes a mono average level every 0.5 s, converts it to dBA through a user-made
calibration table (40–105 dBA, 5 dBA steps, linear interpolation), and shows it as a three-zone
meter with the value in the center plus a 90-minute history chart. A menu provides audio input
selection, color limit editing and the calibration dialog. Settings persist as JSON per user.
Core logic (level math, calibration, zones, history) lives in a UI-independent library so it can
be unit tested; capture and drawing are thin layers around it.

## Technical Context

**Language/Version**: C# 14 on .NET 10 (`net10.0-windows`)

**Primary Dependencies**: NAudio (WASAPI capture and device enumeration); no chart library (custom GDI+ painting)

**Storage**: JSON settings file in `%APPDATA%\AudioMeter\settings.json`

**Testing**: xUnit for the core library; manual checklist for device and UI behavior (see quickstart)

**Target Platform**: Windows 10/11 desktop

**Project Type**: desktop-app

**Performance Goals**: Update every 0.5 s ±0.1 s; UI repaint under 16 ms; chart repaint with up to 10,800 points

**Constraints**: No blocking work on the UI thread; stable for 8 h; memory bounded by 90-minute history (about 10,800 samples)

**Scale/Scope**: One window, two dialogs (calibration, limits), about 10 source files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status |
|-----------|------|--------|
| I. Windows Desktop Platform | `net10.0-windows`, Windows Forms only | Pass |
| II. UI/Audio separation | Core library with no UI references; forms only coordinate | Pass |
| III. Measurement correctness | Units/ranges documented in [data-model.md](./data-model.md); unit tests for silence, clipping, interpolation, clamping | Pass |
| IV. Responsive/resilient device lifecycle | Capture callbacks off the UI thread; device loss shows "no signal" state; capture disposed on stop/switch | Pass |
| V. Automated quality/simplicity | xUnit tests for core; no extra frameworks or duplicated state | Pass |

Post-design re-check: Pass. No complexity violations.

## Project Structure

### Documentation (this feature)

```text
specs/001-spl-meter-overlay/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── ui-and-settings.md
└── tasks.md             # Created by /speckit-tasks
```

### Source Code (repository root)

```text
AudioMeter.sln
src/
├── AudioMeter.Core/             # net10.0, no UI dependency
│   ├── Levels/                  # LevelCalculator, LevelZones
│   ├── Calibration/             # CalibrationTable, CalibrationSession
│   ├── History/                 # MeasurementHistory
│   └── Settings/                # AppSettings, SettingsStore
└── AudioMeter.App/              # net10.0-windows, WinForms + NAudio
    ├── Program.cs
    ├── MainForm.cs              # meter, chart, menu
    ├── Controls/                # MeterControl, HistoryChartControl
    ├── Dialogs/                 # CalibrationDialog, LimitsDialog
    └── Audio/                   # InputDeviceService, AudioCaptureService

tests/
└── AudioMeter.Core.Tests/       # xUnit
```

**Structure Decision**: Two projects plus a test project. The core library holds all testable
logic (principle II); the app project holds WinForms and NAudio code only.

## Complexity Tracking

No violations to justify.
