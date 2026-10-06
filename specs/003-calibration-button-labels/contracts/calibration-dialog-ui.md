# UI Contract: Calibration Dialog Confirm Button

| State | Label | Effect when clicked |
|-------|-------|---------------------|
| Any step except last | `Next` | Records the point and shows the next step; calibration is not saved |
| Last step | `Save` | Records the final point, completes the calibration, closes the dialog with OK result |

Rules:
- The label is never "OK".
- The label is refreshed whenever the displayed step changes.
- The Cancel button label and behaviour are unchanged.
- Enabled/disabled rules of the confirm button (signal present, session not invalidated) are unchanged.
