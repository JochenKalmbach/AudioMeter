# Data Model: Always-on-Top SPL Meter

## Constants

| Name | Value |
|------|-------|
| ScaleMinDba / ScaleMaxDba | 40 / 105 |
| CalibrationStepDba | 5 (14 steps: 40, 45, … 105) |
| MeasurementIntervalSeconds | 0.5 |
| HistoryDuration | 90 minutes |
| SilenceFloorDbfs | -100 |

## AudioInput

| Field | Type | Notes |
|-------|------|-------|
| Id | string | WASAPI endpoint ID; persisted |
| Name | string | Display name |
| IsDefault | bool | |

## Measurement

| Field | Type | Notes |
|-------|------|-------|
| Timestamp | DateTimeOffset | End of the 0.5 s window |
| LevelDbfs | double | Mean absolute mono level in dBFS, ≤ 0, floored at -100 |
| Dba | double? | Calibrated value, clamped to 40–105; `null` when no signal (gap) |

## CalibrationTable

| Field | Type | Notes |
|-------|------|-------|
| Points | list of (Dba: int, LevelDbfs: double) | Exactly 14 points when calibrated |
| IsCalibrated | bool | False means the default mapping is used |

Rules:
- Dba values are the fixed steps 40…105.
- LevelDbfs MUST be strictly increasing with Dba; otherwise the step is rejected (FR-016).
- Conversion: piecewise-linear between neighbouring points; below the first point → 40, above the last → 105.
- Default mapping (uncalibrated): -90 dBFS → 40 dBA, 0 dBFS → 105 dBA, linear.

## LevelZones

| Field | Type | Default | Validation |
|-------|------|---------|------------|
| GreenYellowLimit | double | 70 | 40 ≤ value < YellowRedLimit |
| YellowRedLimit | double | 85 | GreenYellowLimit < value ≤ 105 |

Zone of a value `v`: green if `v ≤ GreenYellowLimit`; yellow if `v ≤ YellowRedLimit`; otherwise red.

## History

Ordered list of `Measurement`, oldest first. On each add, entries older than 90 minutes are removed. `Dba == null` entries render as gaps.

## AppSettings (persisted)

| Field | Type |
|-------|------|
| InputDeviceId | string? |
| Calibration | CalibrationTable (points) |
| GreenYellowLimit, YellowRedLimit | double |
| Window (X, Y, Width, Height) | ints |

## State: capture lifecycle

`Stopped → Starting → Running → (DeviceLost → Starting on recovery) → Stopped`.
In `DeviceLost`/`Stopped` the UI shows "No signal" and history receives gap entries.

## State: calibration session

`Step[i] (i = 0…13)` → OK → `Step[i+1]`; OK on step 13 → Save and close; Cancel at any step → discard. Non-increasing level on OK → warning, stay on the same step.
