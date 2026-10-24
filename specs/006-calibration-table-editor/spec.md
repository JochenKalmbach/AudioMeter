# Feature Specification: Calibration Table Editor

**Feature Branch**: `006-calibration-table-editor`

**Created**: 2026-10-24

**Status**: Draft

**Input**: User description: "Please add for the calibration an option that we can directly adjust the calibration table via editing a table in a new calibration dialog. So we have both way to calibrate the data. This dialog should also show the current dBFS and the resulting dBA with the usage of the currently edited table. But also keep the old calibration dialog (maybe make a "Calibration" menu entry with 2 sub-entries; similar as the audio-selection...)"

## Clarifications

### Session 2026-10-24

- Q: What range of dBFS values should the table editor accept in a cell? → A: -120 to 0 dBFS
- Q: Should Save close the dialog? → A: Save persists the table and closes the dialog. Valid cell edits take effect in the meter immediately, before Save.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Edit the calibration table directly (Priority: P1)

The user opens a new calibration dialog that lists the calibration table: one row per calibration point (40…105 dBA in 5 dBA steps) with an editable dBFS value for each. The user changes values directly and saves the table, which then becomes the active calibration.

**Why this priority**: Core new capability: calibrating by typing known values instead of stepping through a live measurement.

**Independent Test**: Open the table dialog, change several dBFS values, save, and verify the meter reading uses the new table.

**Acceptance Scenarios**:

1. **Given** the table dialog is opened, **When** it appears, **Then** it shows all calibration points with the currently active dBFS value for each dBA row (the default mapping if never calibrated).
2. **Given** the user edits a dBFS cell to a valid value, **When** the edit is committed, **Then** the meter immediately uses the edited table; **When** they then press Save, **Then** the table is persisted across restarts and the dialog closes.
3. **Given** the user edits values so the table is invalid (not strictly increasing, or non-numeric), **When** they try to save, **Then** saving is blocked, the offending row(s) are indicated, and a message explains the problem.
4. **Given** the user cancels the dialog after making edits, **When** it closes, **Then** the meter reverts to the previously saved calibration and nothing is persisted.

---

### User Story 2 - Live feedback of dBFS and resulting dBA (Priority: P1)

While the table dialog is open, it continuously shows the current measured input level (dBFS) and the resulting dBA computed with the table as currently edited (including unsaved edits).

**Why this priority**: Lets the user verify an edit immediately against a reference sound level before committing.

**Independent Test**: With audio input present, change a table value and observe that the displayed dBA changes accordingly while dBFS stays the same.

**Acceptance Scenarios**:

1. **Given** the dialog is open and input is present, **When** the level changes, **Then** the displayed dBFS and dBA update continuously.
2. **Given** the user edits a cell, **When** the edit is committed to the cell, **Then** the displayed dBA is immediately recomputed using the edited (unsaved) table.
3. **Given** the edited table is currently invalid, **When** the dBA display is shown, **Then** it indicates that no valid result is available instead of a misleading number.

---

### User Story 3 - Choose between both calibration methods in the menu (Priority: P2)

The existing "Calibration" entry becomes a submenu with two entries: the existing step-by-step calibration (unchanged) and the new table editor, in the same style as the existing audio-selection submenu.

**Why this priority**: Preserves the existing workflow while exposing the new one.

**Independent Test**: Open the Calibration menu, verify both entries exist and each opens its dialog.

**Acceptance Scenarios**:

1. **Given** the app menu, **When** the user opens "Calibration", **Then** two sub-entries are shown: the existing step-by-step calibration and the table editor.
2. **Given** the user picks the existing entry, **When** the dialog opens, **Then** it behaves exactly as before.
3. **Given** a table saved in either dialog, **When** the other dialog is opened, **Then** it reflects the same active calibration.

---

### Edge Cases

- Never calibrated: the table shows the default mapping, and saving writes it as a calibration.
- No audio input / silence: dBFS shows the lowest displayable value and dBA the lowest table value; no error.
- Level outside the table range: dBA is clamped to the table's minimum/maximum, as in normal metering.
- Empty cell, text, or out-of-range number: the cell is flagged invalid and saving is blocked.
- The dBA column is fixed (40…105 in 5 dBA steps) and not editable; rows cannot be added or removed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The app MUST provide a "Calibration" menu entry containing two sub-entries: the existing step-by-step calibration and a new table-editing calibration.
- **FR-002**: The existing step-by-step calibration dialog MUST remain unchanged in behaviour.
- **FR-003**: The table dialog MUST list every calibration point with a fixed dBA value and an editable dBFS value.
- **FR-004**: The table dialog MUST initially show the currently active calibration (or the default mapping if uncalibrated).
- **FR-005**: The table dialog MUST validate that values are numeric and strictly increasing with dBA, flag invalid rows, and block saving while invalid.
- **FR-006**: The table dialog MUST show the current input level in dBFS and the resulting dBA computed from the table as currently edited, updating continuously.
- **FR-007**: When the edited table is invalid, the dBA display MUST indicate that no valid result is available.
- **FR-008**: Each valid cell edit MUST take effect in the meter immediately (live preview), without requiring Save. Invalid edits MUST NOT change the meter's active table.
- **FR-009**: Save MUST persist the table as the calibration (surviving restarts) and close the dialog. Cancel MUST revert the meter to the previously saved calibration and persist nothing.
- **FR-011**: Each dBFS cell MUST accept only values from -120 to 0 dBFS inclusive; values outside this range are invalid.
- **FR-010**: Both calibration dialogs MUST read and write the same stored calibration.

### Key Entities

- **Calibration table**: Ordered set of calibration points mapping input level (dBFS) to sound level (dBA).
- **Calibration point**: A fixed dBA value paired with an editable dBFS value.
- **Edited table (draft)**: The unsaved table state in the dialog, used for live dBA display until saved or cancelled.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can change a single calibration value and save it in under 30 seconds.
- **SC-002**: The displayed dBA reflects an edit within 1 second of committing the cell, and live level changes within 1 second.
- **SC-003**: 100% of invalid tables are rejected with a visible indication of which rows are wrong; no invalid table is ever stored.
- **SC-004**: A calibration saved in either dialog is shown identically in the other and survives an application restart.
- **SC-005**: Users of the existing calibration workflow notice no change in its behaviour.

## Assumptions

- The calibration structure from spec 002 is unchanged: 14 points, 40…105 dBA in 5 dBA steps, dBFS strictly increasing.
- The dBFS shown is the same input level used by the existing calibration dialog and meter.
- Values are entered in dB with decimals allowed.
- Labels are in English, matching the existing UI.
- Importing/exporting tables is out of scope.
