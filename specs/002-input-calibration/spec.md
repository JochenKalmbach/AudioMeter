# Feature Specification: Calibration Input Level

**Feature Branch**: `[002-input-calibration]`

**Created**: 2026-10-06

**Status**: Draft

**Input**: User description: "The calibration should start with the highest value (105) and not the lowest (40), so we can better adjust the input level to the max. Also display the device input level (0–100) in the calibration window as a slider, editable only during the first calibration point at 105 dBA and read-only afterward. Keep it synchronized with the input settings, store it with the calibration and input channel, and restore it when the application starts."

## Clarifications

### Session 2026-10-06

- Q: Should only the calibration capture sequence be descending while the completed table remains ascending? → A: Yes. Capture targets from 105 down to 40 dBA, and keep the completed calibration table ordered from 40 up to 105 dBA.

## User Scenarios & Testing

### User Story 1 - Calibrate from the highest reference level (Priority: P1)

When calibrating the sound meter, the user starts at 105 dBA and adjusts the input level to obtain a suitable maximum signal before recording the remaining calibration points.

**Why this priority**: Adjusting the input at the loudest reference level is the primary purpose of this change and establishes the gain used by the rest of the calibration.

**Independent Test**: Start a calibration and verify that the first target is 105 dBA, followed by all remaining targets in descending 5 dBA increments through 40 dBA.

**Acceptance Scenarios**:

1. **Given** the user starts calibration, **When** the calibration window opens, **Then** its first target is 105 dBA.
2. **Given** a calibration target is shown, **When** the user confirms it, **Then** the next target is 5 dBA lower until 40 dBA is recorded.
3. **Given** the user cancels before completing calibration, **When** the window closes, **Then** the previous calibration remains in effect.

### User Story 2 - Adjust input level during the first calibration point (Priority: P1)

While recording the 105 dBA calibration point, the user can view and adjust the selected device's input level on a 0–100 slider. After recording that point, the level is read-only for the rest of that calibration.

**Why this priority**: The user needs to set a suitable device input level at maximum reference loudness and keep that gain consistent across all lower reference points.

**Independent Test**: During the 105 dBA point, adjust the slider and verify the input setting changes; record the point and verify that the slider cannot be changed at any later point.

**Acceptance Scenarios**:

1. **Given** the calibration is at its first 105 dBA point, **When** the user moves the input-level slider, **Then** the selected device's input level changes to the displayed value.
2. **Given** the 105 dBA point has been recorded, **When** any subsequent calibration point is displayed, **Then** the input-level slider displays the current value but does not allow changes.
3. **Given** the input level is changed in the device's sound settings while the calibration window is open, **When** the change takes effect, **Then** the calibration slider immediately displays the same value; the converse applies while the calibration slider is editable.
4. **Given** the input level is changed in the device's sound settings after the 105 dBA point has been recorded, **When** the user attempts to continue, **Then** the application requires calibration to restart at 105 dBA rather than saving points recorded at different input levels.

### User Story 3 - Restore the calibration input configuration (Priority: P1)

After completing calibration, the user can restart the application and continue measuring with the input channel and input level used for that calibration.

**Why this priority**: A calibration is meaningful only when its associated input channel and gain are restored and used consistently.

**Independent Test**: Complete and save calibration for a selected channel and input level, restart the application, and verify that both are restored and applied.

**Acceptance Scenarios**:

1. **Given** a completed calibration is saved with an input channel and level, **When** the application starts, **Then** it selects that channel and applies that level.
2. **Given** the input level changes during calibration, **When** the user completes calibration, **Then** the saved calibration records the final input channel and input-level value together with its calibration points.
3. **Given** a saved calibration exists, **When** the input level is changed afterward, **Then** the application indicates that recalibration is required and does not treat the old mapping as valid for the changed level.

### Edge Cases

- If the user cancels calibration before recording the 105 dBA point, the prior calibration and prior applied input level remain unchanged.
- If a user changes the input level in device sound settings after the 105 dBA point has been recorded, the calibration slider shows the new value but remains read-only.
- If the input level changes after a saved calibration, that calibration is no longer valid for the changed gain and the application indicates that recalibration is required.
- If the input level changes through device sound settings after the 105 dBA point is recorded but before the current calibration is complete, the application must not save points captured at mixed input levels.
- If saved calibration data has no input-level value because it predates this feature, the existing calibration remains usable and the application's current input-level setting is retained until a new calibration records the value.
- If the saved input channel is unavailable at startup, the application reports that it cannot restore that channel and does not present the saved calibration as an active calibration for a different channel.

## Requirements

### Functional Requirements

- **FR-001**: A new calibration MUST begin at 105 dBA and offer all 14 existing target values in descending 5 dBA increments through 40 dBA.
- **FR-002**: For each target, the calibration window MUST show the target and live measured input level, and confirmation MUST record the measured level for that target before advancing.
- **FR-003**: The calibration window MUST display the selected input device's current input-level value on a slider whose range is 0 through 100, matching the device sound settings' value and range.
- **FR-004**: The input-level slider MUST be changeable only before the 105 dBA point is confirmed. Once that point is confirmed, it MUST be read-only for the remainder of the calibration.
- **FR-005**: A change to input level made from either the editable calibration slider or the device's sound settings MUST be applied to the same selected device and reflected in the calibration window immediately.
- **FR-006**: Completing calibration MUST persist its calibration points together with the associated input channel and the input-level value in effect when the 105 dBA point is confirmed.
- **FR-007**: On application startup, when saved calibration data includes its input channel and input level, the application MUST select that channel and apply that input level before using the calibration for measurement.
- **FR-008**: Cancelling calibration MUST leave the previously saved calibration unchanged and, if no calibration point was committed, restore the input level that was active before the calibration began.
- **FR-009**: Calibration points MUST remain associated with the input channel and input level used to record them; they MUST NOT be silently applied to a different input channel.
- **FR-010**: Calibration data saved before this feature, without an input-level value, MUST remain usable without inventing a value for that historical calibration; the application MUST retain its current input-level setting until a new calibration saves an input-level value.
- **FR-011**: If the input level changes after a calibration is saved, the application MUST indicate that recalibration is required and MUST NOT present the old calibration as valid for the changed input level.
- **FR-012**: If the input level changes from device sound settings after the 105 dBA point is recorded during an unfinished calibration, the application MUST prevent saving a mapping recorded at mixed input levels and require the user to restart at 105 dBA.

### Key Entities

- **Calibration**: The ordered set of 14 reference dBA and measured input-level pairs, plus the input channel and device input-level value used for the calibration.
- **Input Level**: The selected device's adjustable capture volume from 0 to 100, reflected by the calibration slider when changed in device sound settings.
- **Calibration Session**: The in-progress capture of targets from 105 down to 40 dBA; the input level is editable before the first target is committed and read-only afterward.

## Success Criteria

### Measurable Outcomes

- **SC-001**: In every new calibration, the first displayed target is 105 dBA and all 14 targets are confirmed in descending 5 dBA steps ending at 40 dBA.
- **SC-002**: During the 105 dBA step, users can select any input-level value from 0 through 100; after confirming that step, the value cannot be changed from the calibration window for the remaining 13 steps.
- **SC-003**: While the calibration window is open, a change made in either the slider or Windows Sound settings is reflected in the other within 1 second.
- **SC-004**: In 10 consecutive save-and-restart checks, the saved input channel and input-level value are restored exactly before the saved calibration is used.
- **SC-005**: Cancelling before committing the first calibration point leaves the prior saved calibration and active input level unchanged.

## Assumptions

- The calibration slider controls the selected audio input device's capture-endpoint volume, which is also shown in Windows Sound settings on a 0–100 scale.
- “First calibration” means the first (105 dBA) point of each calibration run; the input level may be adjusted again at that point during a later recalibration.
- The input-level value saved with calibration is the value in effect when the 105 dBA point is confirmed, and it remains fixed for the other points in that run.
- Existing calibration records without an input-level value are retained and used as before, with the current input-level setting, until the user completes a calibration that records the value.
