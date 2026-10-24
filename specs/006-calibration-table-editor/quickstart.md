# Quickstart: Calibration Table Editor

## Automated
```powershell
dotnet test tests/AudioMeter.Core.Tests --filter CalibrationTableDraft
```
Expected: boundary cases pass (-120 and 0 accepted; -121, 0.1, empty, text, NaN, equal or decreasing neighbours rejected; `ToDba` null when invalid).

## Manual
1. `dotnet run --project src/AudioMeter.App`, with an audio input present.
2. Settings → Calibration shows two entries. **Step-by-step…** works as before.
3. **Edit table…**: grid shows 14 rows with the active dBFS values.
4. Observe the live label; change a dBFS value: the dBA changes, dBFS does not, and the meter needle follows immediately.
5. Enter an invalid value (e.g. `5`, or a value ≤ the row above): row flagged, Save disabled, label shows n/a.
6. **Cancel**: meter returns to the earlier calibration. Reopen, edit, **Save**: dialog closes; restart the app and confirm the table persists.
7. Open **Step-by-step…** after saving: calibration state is consistent (FR-010).
