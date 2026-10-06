# Quickstart: Calibration Input Level Validation

## Prerequisites

- Windows 10/11 with an active capture endpoint that exposes an adjustable input volume.
- .NET 10 SDK.
- A known reference sound source capable of producing the 105 dBA and lower calibration targets.

## Build and core tests

```powershell
dotnet build AudioMeter.sln
dotnet test tests\AudioMeter.Core.Tests
dotnet run --project src\AudioMeter.App
```

Expected: build succeeds and core tests pass, including descending session targets, ascending completed calibration points, strict signal ordering, persistence metadata, and legacy settings compatibility.

## Manual end-to-end scenarios

1. **Device input-level synchronization**: Open Windows Sound settings for the selected capture endpoint and the calibration window. Verify the slider spans 0–100 and shows the endpoint's current value. Change the value with the calibration slider and verify Windows Sound settings updates; then change it in Windows Sound settings and verify the calibration slider reflects it within 1 second.
2. **Calibration order and adjustable first point**: Open calibration. Verify 105 dBA is shown first and the slider is editable. Change the input level; confirm the 105 dBA point. Verify the next target is 100 dBA and the slider is read-only. Continue through 40 dBA.
3. **Direction and calibration output**: Complete a run with strictly decreasing measured dBFS for each descending dBA target. Verify all 14 points are accepted and the resulting mapping behaves in the existing ascending dBA order.
4. **Mid-session setting change**: Confirm 105 dBA, change input level from Windows Sound settings, and verify calibration reports the session invalid and requires restart at 105 dBA rather than saving mixed-gain points.
5. **Cancel rollback**: Begin a calibration, adjust the input level before confirming 105 dBA, then cancel. Verify the pre-session level and prior calibration remain in effect.
6. **Save and restart**: Complete calibration and restart the application. Verify the calibrated endpoint and the exact associated input-level value are applied before the mapping is used.
7. **Stale calibration**: After calibration, change input level or device. Verify the application indicates recalibration is required and does not display the old mapping as valid for the changed configuration.
8. **Legacy settings**: Start with a pre-feature settings file containing the old calibration array and `inputDeviceId` only. Verify it loads without losing its points or inventing calibration metadata.
9. **Unavailable endpoint**: Make the saved calibration endpoint unavailable and restart. Verify a visible message appears and the calibration is not used with another endpoint.

See [contracts/input-level-and-calibration.md](./contracts/input-level-and-calibration.md) and [data-model.md](./data-model.md) for persisted field and state details.
