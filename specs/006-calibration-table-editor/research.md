# Research: Calibration Table Editor

## Decision 1: Where validation lives
- **Decision**: New `CalibrationTableDraft` in Core holds 14 cell strings, parses them (invariant culture and current culture, decimals allowed), checks -120…0 and strict increase, and builds a `CalibrationTable` through the existing `CalibrationTable.TryCreate`.
- **Rationale**: Reuses the single source of truth for table rules; keeps the form logic-free (constitution II); unit-testable.
- **Alternatives**: Validate inside the form (untestable); extend `CalibrationTable` with range checks (would change behaviour of the existing flow and stored-settings loading).

## Decision 2: Live preview in the meter
- **Decision**: The dialog raises an event with the current valid `CalibrationTable?` after each committed cell edit; `MainForm` assigns it to the active table and re-applies the meter. Invalid drafts do not raise a table change (meter keeps the last valid one). On Cancel, `MainForm` calls the existing `RefreshCalibrationValidity()` which restores the saved/stored/default table.
- **Rationale**: Matches the clarified requirement (immediate effect, Cancel reverts) and reuses existing restore logic.
- **Alternatives**: Preview only in the dialog (contradicts clarification).

## Decision 3: Live dBFS / dBA readout
- **Decision**: `MainForm.OnLevel` already receives each dBFS value; it forwards it to whichever calibration dialog is open. The table dialog computes dBA from the draft table (`ToDba`), or shows "n/a (table invalid)" when the draft is invalid. Signal loss clears the readout, as in the existing dialog.
- **Rationale**: No new capture path; same dBFS the meter uses.

## Decision 4: Calibration metadata (stale detection)
- **Decision**: On Save, store the table plus the current input device id and input level (as the existing dialog does) when the input-level service is available; if it is unavailable, still allow editing and store the table with no input metadata (existing code treats absent metadata as "always valid").
- **Rationale**: Prevents the existing stale-calibration logic from discarding a table the user just typed, without blocking users whose device has no level control.
- **Alternatives**: Refuse the dialog without level service (needlessly restrictive for a manual-entry feature).

## Decision 5: Grid control and menu
- **Decision**: `DataGridView` with a read-only dBA column and an editable dBFS column; per-cell error text for invalid rows; Save disabled while invalid. Menu: "Calibration" `ToolStripMenuItem` with two children, built like the existing "Audio input" submenu (static children).
- **Rationale**: Built-in WinForms grid satisfies the Windows-only constraint; no extra dependency.

## Open items
None.
