# Contract: Input Level and Calibration

## Device input level

- The calibration window presents an integer slider from 0 through 100 for the selected audio input's Windows capture-endpoint volume.
- A slider change applies to that endpoint; the displayed value reflects the endpoint's actual level.
- A change made in Windows Sound settings is reflected by the calibration slider within 1 second.
- Unsupported or unavailable endpoint volume control is reported clearly; the UI does not display a requested value as applied when the endpoint rejected it.
- Changing the selected device changes the endpoint represented by the slider. A calibration for another device is not treated as active.

## Calibration dialog

- Targets appear in descending order: 105, 100, …, 40 dBA (14 steps).
- The dialog shows the target, measured live input in dBFS, and current input-level value (0–100).
- The input-level slider is editable before confirming the first 105 dBA point and read-only after that point is recorded.
- Confirm requires a live measurement. Every subsequent point must be lower in dBFS than the preceding capture; invalid ordering warns and does not advance.
- Changing input device or input level through Windows Sound settings after capturing 105 dBA invalidates the in-progress session; the user must restart it at 105 dBA.
- Confirming 40 dBA completes the session. The points are persisted in ascending dBA order with the selected device ID and input level captured at 105 dBA confirmation.
- Cancel before confirming 105 dBA restores the session-start input-level value and leaves prior calibration untouched. Incomplete points are never persisted.

## Persisted settings compatibility

The existing calibration point array and selected endpoint ID remain supported. The settings document adds current input-level and calibration-associated metadata fields. Conceptual example:

```json
{
  "inputDeviceId": "capture-endpoint-id",
  "inputLevel": 72,
  "calibrationInputDeviceId": "capture-endpoint-id",
  "calibrationInputLevel": 72,
  "calibration": [
    { "dba": 40, "levelDbfs": -62.1 }
  ]
}
```

- Completed mappings contain all 14 points and are saved in ascending dBA order, even though the dialog captures in descending order.
- Older settings files containing only `inputDeviceId` and a calibration point array remain readable; absent input-level metadata is not inferred.
- If calibration-associated metadata exists, startup selects its device and applies its input level before treating its mapping as active.
- A missing device or a mismatch between the selected endpoint/input level and saved calibration metadata causes a visible stale/unavailable state; the old mapping is not silently applied to a different configuration.
