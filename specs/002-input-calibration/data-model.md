# Data Model: Calibration Input Level

## Constants

| Name | Value |
|------|-------|
| ScaleMinDba / ScaleMaxDba | 40 / 105 |
| CalibrationStepDba | 5 |
| CalibrationStepCount | 14 |
| InputLevelMin / InputLevelMax | 0 / 100 |

## AudioInput

| Field | Type | Notes |
|-------|------|-------|
| Id | string | WASAPI capture endpoint ID |
| Name | string | Display name |
| IsDefault | bool | Whether Windows currently marks it default |

## InputLevel

| Field | Type | Notes |
|-------|------|-------|
| DeviceId | string | Endpoint whose capture volume is controlled |
| Value | int | Inclusive range 0–100, displayed as the Windows capture endpoint's normalized input volume |

Rules:

- One application-level input-level service is authoritative for the selected endpoint.
- The calibration slider displays the current value from the selected endpoint.
- A slider change is applied to the endpoint; a change made in Windows Sound settings is reflected in the slider.
- If the endpoint is missing or volume control is unsupported, report the condition; do not claim a requested value was applied.

## CalibrationPoint

| Field | Type | Notes |
|-------|------|-------|
| Dba | int | One fixed target from 40 to 105 in 5 dBA increments |
| LevelDbfs | double | Captured mean-absolute input level |

The canonical `CalibrationTable` remains ordered by increasing dBA and requires strictly increasing `LevelDbfs`, regardless of the order in which points were captured.

## Calibration

| Field | Type | Notes |
|-------|------|-------|
| Points | list of CalibrationPoint | Exactly 14 valid points for a completed calibration |
| InputDeviceId | string? | Capture endpoint used to produce the points |
| InputLevel | int? | 0–100 value at the time the 105 dBA target was confirmed |

Rules:

- Session targets are 105, 100, …, 40 dBA.
- Confirming a target records the current dBFS measurement; each lower dBA target must have a lower dBFS level than the point captured immediately before it.
- Completed session points are reordered into ascending dBA before validating and building the table.
- The saved calibration is active only while both the selected device ID and input level match its associated values.
- Changing the selected device or input level after completion marks the existing calibration stale; it must not be used as if it matched the new configuration.
- During a session, changing the device or changing input level after the 105 dBA point was committed invalidates that session and requires a restart.

## AppSettings (persisted)

| Field | Type | Notes |
|-------|------|-------|
| InputDeviceId | string? | Selected capture endpoint |
| InputLevel | int? | Current endpoint input level; null when not yet set by this application |
| Calibration | list of CalibrationPoint | Existing JSON array retained for backward compatibility |
| CalibrationInputDeviceId | string? | Endpoint associated with the saved calibration |
| CalibrationInputLevel | int? | Level associated with the saved calibration |
| GreenYellowLimit, YellowRedLimit | double | Existing meter thresholds |
| Window | WindowSettings? | Existing window bounds |

Validation and migration:

- Current and calibration-associated input-level values, when present, must be within 0–100; invalid values are ignored with a visible settings warning or safely treated as unset.
- Existing settings with no `InputLevel`, `CalibrationInputDeviceId`, or `CalibrationInputLevel` remain readable. Historical calibration points remain intact; missing metadata is not synthesized.
- When saved calibration metadata is present and valid, startup selects its associated device and applies its associated input level before activating its calibration mapping.
- If the associated device is unavailable, the application reports the problem and does not apply that calibration to a fallback device.

## Calibration session state

| State | Input level | Transition |
|-------|-------------|------------|
| Awaiting105 | Editable | Confirm valid measurement → Captured105 |
| Captured105 through Captured45 | Read-only in calibration; settings changes invalidate session | Confirm strictly lower measurement → next lower target |
| Completed | Read-only | Save points with device ID and 105 dBA input level |
| Cancelled | Closed | Before first confirmation, restore pre-session input level and keep old mapping |
| Invalidated | Not savable | Device or post-105 input-level change requires restart at 105 |
