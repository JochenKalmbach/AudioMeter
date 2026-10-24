# Data Model: Calibration Table Editor

## CalibrationPoint (existing)
`Dba` (int, 40…105 step 5, fixed) and `LevelDbfs` (double).

## CalibrationTable (existing)
14 points; `TryCreate` enforces count, dBA grid, strictly increasing, non-NaN. `ToDba(dbfs)` is piecewise-linear and clamped.

## CalibrationTableDraft (new, Core)
| Member | Description |
|--------|-------------|
| `Rows` | 14 rows: fixed `Dba` + editable `Text` for the dBFS cell |
| `SetCell(index, text)` | Updates one cell and revalidates |
| `RowError(index)` | Message for an invalid row, else null (non-numeric, empty, outside -120…0, not greater than previous valid row) |
| `IsValid` | True when all 14 rows have no error |
| `TryBuild(out CalibrationTable)` | Builds the table via `CalibrationTable.TryCreate` when valid |
| `ToDba(dbfs)` | dBA from the draft, or null when invalid |
| `FromTable(table)` | Initial draft from the active table (default mapping if uncalibrated) |

### Validation rules
1. Cell parses as a finite number (decimal point or comma accepted per culture).
2. -120 ≤ value ≤ 0.
3. value > previous row's value (strictly increasing); the row that breaks the order is the one flagged.

### State
Draft (dialog open) → Valid draft → applied live to meter; Save → persisted + dialog closes; Cancel → discarded, meter restored.

## Persisted settings (existing, unchanged shape)
`AppSettings.Calibration` (points), `CalibrationInputDeviceId`, `CalibrationInputLevel`.
