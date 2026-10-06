# Data Model: Calibration Dialog Button Labels

No persisted data changes.

## Calibration session (existing, extended)

| Member | Type | Description |
|--------|------|-------------|
| `StepIndex` | int | Zero-based number of points already recorded (existing) |
| `StepCount` | int | Total points in the sequence (existing) |
| `IsLastStep` | bool | New, derived: `StepIndex == StepCount - 1` |

## Derived UI state

| Condition | Confirm button text |
|-----------|---------------------|
| `IsLastStep == false` | `Next` |
| `IsLastStep == true` | `Save` |

A one-point sequence has `StepCount == 1`, so its only step is last and reads "Save".
