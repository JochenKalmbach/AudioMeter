# UI Contract: Calibration Menu and Table Dialog

## Menu
Settings menu → **Calibration** (submenu, replaces "Calibrate…"):
1. **Step-by-step…**: opens the existing `CalibrationDialog`, behaviour unchanged.
2. **Edit table…**: opens the table dialog.

## Table dialog
| Element | Behaviour |
|---------|-----------|
| Grid column "dBA" | 40…105 step 5; read-only; no add/remove rows |
| Grid column "dBFS" | Editable; accepts -120…0, strictly increasing; invalid cell shows error icon/tooltip |
| Live label | `Measured input: x.x dBFS  →  y.y dBA` using the edited table; `→ n/a (table invalid)` when invalid; cleared on no signal |
| **Save** | Enabled only when valid; persists table (+ input metadata when available), closes with OK |
| **Cancel** | Closes; meter reverts to the previously saved/stored calibration; nothing persisted |
| Meter | Each valid committed edit is applied to the meter immediately |

## Error handling
- Invalid cell: flagged, Save disabled, meter keeps the last valid table.
- Settings write failure on Save: existing "Settings could not be saved" message.
