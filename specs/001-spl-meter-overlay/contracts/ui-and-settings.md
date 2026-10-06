# Contracts: UI and Settings File

## Main window

- Always on top; resizable; minimum size 320 × 240.
- Top: `MenuStrip` — **Settings** → *Audio input* (submenu, radio-checked list), *Color limits…*, *Calibrate…*.
- Middle: meter, linear 40–105 dBA scale, three zones sized proportionally; fill color is the current zone's color; value centered as an integer-rounded number with "dBA". States: *Live*, *No signal* (text instead of value), *Uncalibrated* (indicator shown).
- Bottom: history chart, 90-minute time axis (right edge = now), 40–105 dBA vertical axis, zone limit lines, gaps as breaks.

## Calibration dialog

- Shows: target dBA (large), live measured level (dBFS), step "n of 14".
- Buttons: **OK** (record and advance; last step saves and closes), **Cancel** (discard).
- Warning shown when the recorded level is not above the previous step's level; step is not advanced.

## Color limits dialog

- Two numeric fields (green/yellow, yellow/red), **OK** / **Cancel**.
- On invalid input (outside 40–105 or not strictly ascending): message, dialog stays open, prior limits unchanged.

## Settings file `%APPDATA%\AudioMeter\settings.json`

```json
{
  "inputDeviceId": "string or null",
  "greenYellowLimit": 70,
  "yellowRedLimit": 85,
  "calibration": [ { "dba": 40, "levelDbfs": -62.1 } ],
  "window": { "x": 100, "y": 100, "width": 480, "height": 360 }
}
```

- `calibration` is empty or contains exactly 14 points with strictly increasing `levelDbfs`; otherwise it is ignored (uncalibrated).
- Unknown or invalid fields fall back to defaults.
