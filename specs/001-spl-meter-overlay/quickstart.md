# Quickstart: Validation Guide

## Prerequisites

- Windows 10/11 with a working audio input.
- .NET 10 SDK (`dotnet --list-sdks` must list 10.x).
- A way to produce a reference sound of known dBA (external, e.g. a calibrated source or phone app plus reference meter).

## Build and test

```powershell
dotnet build AudioMeter.sln
dotnet test tests\AudioMeter.Core.Tests
dotnet run --project src\AudioMeter.App
```

Expected: build succeeds; unit tests pass (level math, calibration, zones, history, settings).

## Validation scenarios

1. **Live meter (US1)**: Start the app, focus another window → meter stays on top. Clap/talk → value and color change within 1 s; updates every 0.5 s.
2. **Input selection (US2)**: Settings → Audio input → pick another device; meter follows it. Restart → selection retained.
3. **Color limits (US2b)**: Settings → Color limits → set 60 / 80 → colors switch at new values. Enter 90 / 80 or 30 / 100 → rejected with message.
4. **Calibration (US3)**: Settings → Calibrate; for each target 40…105 set the external sound to that level and press OK. After step 14 the dialog closes; displayed values at reference levels are within 1 dBA. Cancel midway → old mapping unchanged.
5. **Chart (US4)**: Let it run; the chart grows leftwards; after 90 minutes older data scrolls out. Resize window → everything scales.
6. **Failure handling**: Disable/unplug the input while running → "No signal", gap in chart; re-enable → recovers.
7. **Uncalibrated**: Delete `%APPDATA%\AudioMeter\settings.json` → "Uncalibrated" indicator is shown.

See [contracts/ui-and-settings.md](./contracts/ui-and-settings.md) and [data-model.md](./data-model.md) for details.
