# Quickstart: Validate Calibration Dialog Button Labels

## Prerequisites
- Windows, .NET 10 SDK, an audio input device.

## Automated
```powershell
dotnet test tests\AudioMeter.Core.Tests
```
Expect the `IsLastStep` tests to pass (false until the last step, true on the last step).

## Manual
1. Press F5 (runs AudioMeter.App) and open the calibration dialog.
2. On step 1 confirm the button reads **Next**; the instruction text does not mention "OK".
3. Confirm each step; the button stays **Next** until the final step.
4. On the final step the button reads **Save**; clicking it saves and closes the dialog.
5. Reopen the dialog, reach a later step, change the input level to invalidate, cancel, reopen: the button reads **Next** again.
6. Cancel at any step: the previous calibration is unchanged.

See [contracts/calibration-dialog-ui.md](contracts/calibration-dialog-ui.md).
