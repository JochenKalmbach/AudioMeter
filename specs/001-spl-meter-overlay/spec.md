# Feature Specification: Always-on-Top SPL Meter

**Feature Branch**: `001-spl-meter-overlay`

**Created**: 2026-10-06

**Status**: Draft

**Input**: User description: "A Windows application which stays on top of all windows and displays an SPL meter with 3 colors (green 0-70 dBA, yellow 70-85 dBA, red >85 dBA). In the middle of the meter the dBA value from the audio input is displayed. It is resizable and has a menu for settings, including selecting the audio input. A calibration dialog creates a manual dBA mapping: an external sound of known dBA is played, the measured input level is recorded, from 40 dBA to 95 dBA in 5 dBA steps; other values are interpolated. The input is measured every 0.5 seconds as the average of the mono signal and updates the UI. A chart at the bottom shows the values of the last 90 minutes."

## Clarifications

### Session 2026-10-06

- Q: Should the green/yellow and yellow/red limits be editable via the settings menu? → A: Yes; defaults are 70 and 85 dBA.
- Q: What rule applies when editing the limits? → A: The yellow/red limit must be higher than the green/yellow limit, both must be within the meter scale, and invalid input is rejected with a message.
- Q: What is the meter scale and calibration range? → A: The meter is a linear scale from 40 to 105 dBA; calibration runs 40–105 dBA in 5 dBA steps (14 steps); the editable limits must lie within 40–105 dBA.
- Q: Should the minimum of the calibration and display scale stay at 40 dBA? → A: Yes; 40 dBA is kept as the minimum, since quiet rooms and typical microphone noise floors are around 30–40 dBA.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Live sound level display (Priority: P1)

A user starts the application and sees a compact window that always stays above all other windows. The window shows a level meter filled in green, yellow or red according to the current sound level, with the current dBA value displayed in the middle. The value refreshes every half second from the selected audio input.

**Why this priority**: This is the core value: an always-visible, glanceable noise level indicator.

**Independent Test**: Start the app with a working audio input, make sounds of varying loudness, and verify the displayed value and meter color follow the sound level while other applications are in the foreground.

**Acceptance Scenarios**:

1. **Given** the app is running and other windows are focused, **When** the user looks at the screen, **Then** the meter window remains visible above all other windows.
2. **Given** the measured level is at or below the green/yellow limit (default 70 dBA), **When** the display updates, **Then** the meter is shown in green with the numeric dBA value in its center.
3. **Given** the measured level is above the green/yellow limit and at or below the yellow/red limit (default 85 dBA), **When** the display updates, **Then** the meter is shown in yellow.
4. **Given** the measured level is above the yellow/red limit, **When** the display updates, **Then** the meter is shown in red.
5. **Given** the sound level changes, **When** half a second passes, **Then** the displayed value reflects the average level of that interval.
6. **Given** the user resizes the window, **When** the new size is applied, **Then** the meter, value and chart scale to fit the window and remain readable.

---

### User Story 2 - Choose the audio input (Priority: P2)

A user opens the settings menu and selects which audio input (port/device) the meter should listen to. The meter switches to the chosen input and the choice is remembered at the next start.

**Why this priority**: Without choosing the right input, the meter cannot measure the intended source.

**Independent Test**: Open the menu, pick a different input, and verify the meter reacts to sound on that input only; restart and confirm the choice persisted.

**Acceptance Scenarios**:

1. **Given** several audio inputs are available, **When** the user opens the settings menu, **Then** all available inputs are listed with the current one indicated.
2. **Given** the user selects another input, **When** the selection is confirmed, **Then** measurement continues using the new input without restarting the app.
3. **Given** the app is restarted, **When** it starts, **Then** it uses the previously selected input if still available.

---

### User Story 2b - Adjust color limits (Priority: P2)

A user opens the settings menu and edits the two limits that separate the green, yellow and red zones (defaults 70 and 85 dBA). The meter immediately uses the new limits and they are remembered at the next start.

**Why this priority**: Acceptable noise levels differ per environment, so the zones must be adjustable.

**Independent Test**: Change the limits in the menu, play sounds around the new limits, and verify the color switches at the new values; restart and confirm persistence.

**Acceptance Scenarios**:

1. **Given** the settings menu is open, **When** the user opens the limits setting, **Then** the current green/yellow and yellow/red limits are shown and editable.
2. **Given** valid new limits are entered, **When** the user confirms, **Then** the meter colors switch at the new values immediately.
3. **Given** the yellow/red limit is not higher than the green/yellow limit, or a value is outside 40–105 dBA, **When** the user confirms, **Then** the input is rejected with a message and the previous limits remain.
4. **Given** the app is restarted, **When** it starts, **Then** the saved limits are used.

---

### User Story 3 - Calibrate dBA mapping (Priority: P2)

A user opens a calibration dialog from the menu. Using an external sound source of known loudness, the dialog guides them through target levels from 40 dBA to 105 dBA in 5 dBA steps. For each step the dialog shows the target dBA value and the live measured input level; when the user has set the external sound to that dBA value, they press "OK" to record the measured input level for that step and move on to the next. After the last step, the mapping is saved and used to convert measured input levels to dBA values, with interpolation between recorded points.

**Why this priority**: Raw input levels are meaningless as dBA until mapped; calibration makes the displayed values meaningful.

**Independent Test**: Run the calibration through all 14 steps (40, 45, … 105) with different input levels, then feed intermediate levels and verify displayed dBA values lie proportionally between the neighbouring calibration points.

**Acceptance Scenarios**:

1. **Given** the user opens the calibration dialog, **When** it starts, **Then** it shows the first target of 40 dBA and the live measured input level.
2. **Given** a target dBA value is shown, **When** the user presses "OK", **Then** the current measured input level is stored for that dBA value and the next target (+5 dBA) is shown.
3. **Given** the final target of 105 dBA has been confirmed, **When** the user presses "OK", **Then** the mapping is saved, applied immediately, and the dialog closes.
4. **Given** a saved mapping, **When** a measured level falls between two calibrated points, **Then** the displayed dBA is linearly interpolated between them.
5. **Given** the user cancels the dialog before the last step, **When** it closes, **Then** the previous mapping remains unchanged.
6. **Given** the app is restarted, **When** it starts, **Then** the saved mapping is still used.

---

### User Story 4 - 90-minute history chart (Priority: P3)

At the bottom of the window, a chart shows the dBA values over time for the last 90 minutes, so the user can see trends and peaks.

**Why this priority**: Adds context over time, but the live meter is useful without it.

**Independent Test**: Run the app for a period with varying sound and verify the chart plots values over time and drops values older than 90 minutes.

**Acceptance Scenarios**:

1. **Given** the app is measuring, **When** new values arrive, **Then** they are appended to the right end of the chart.
2. **Given** more than 90 minutes of data exist, **When** the chart updates, **Then** only the most recent 90 minutes are shown.
3. **Given** the window is resized, **When** the new size is applied, **Then** the chart scales accordingly.

---

### Edge Cases

- No audio input is available or the selected input is removed while running: the app shows a clear message and a non-misleading "no signal" state instead of a stale or zero value, and recovers when an input becomes available.
- No calibration has been performed: the app uses a clearly indicated default mapping and shows that values are uncalibrated.
- Measured level below the lowest calibrated point (40 dBA) or above the highest (105 dBA): values are clamped to the calibration range limits.
- Calibration points that are not increasing (a higher dBA step recorded with a lower or equal input level than a lower step): the dialog warns the user and lets them redo that step.
- Complete silence: the meter shows the lowest value (40 dBA, empty bar) in the green zone without error.
- Very small window sizes: a minimum window size keeps the value readable.
- Measurement gaps (e.g., input switched or lost): the chart shows a gap rather than invented values.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application window MUST stay on top of all other windows while running.
- **FR-002**: The application MUST display a level meter with three zones/colors: green up to the green/yellow limit, yellow above it up to the yellow/red limit, and red above that. The default limits are 70 and 85 dBA. The meter MUST use a linear scale from 40 to 105 dBA, with zone widths proportional to their dBA ranges. The meter MUST use a linear scale from 40 to 105 dBA, with the zone widths proportional to their dBA ranges.
- **FR-017**: Users MUST be able to edit the green/yellow and yellow/red limits via the settings menu; changes MUST apply immediately and persist across restarts.
- **FR-018**: The application MUST reject limit values outside 40–105 dBA or where the yellow/red limit is not higher than the green/yellow limit, show a message, and keep the previous limits.
- **FR-003**: The application MUST show the current dBA value as a number in the middle of the meter area.
- **FR-004**: The application MUST measure the audio input every 0.5 seconds, using the average of the mono signal over that interval, and update the meter and value after each measurement.
- **FR-005**: The window MUST be resizable, with meter, value and chart scaling with the window size.
- **FR-006**: The application MUST provide a menu giving access to settings.
- **FR-007**: Users MUST be able to select the audio input to be measured from the list of available inputs, and the selection MUST persist across restarts.
- **FR-008**: The application MUST provide a calibration dialog that steps through target values from 40 dBA to 105 dBA in 5 dBA increments (14 steps).
- **FR-009**: For each calibration step, the dialog MUST show the target dBA value and the live measured input level, and record the current input level for that target when the user presses "OK".
- **FR-010**: The application MUST store the calibration mapping persistently and use it to convert measured input levels to dBA values.
- **FR-011**: For measured levels between calibration points, the application MUST determine the dBA value by interpolation between the neighbouring points.
- **FR-012**: Cancelling calibration MUST leave the existing mapping unchanged.
- **FR-013**: The application MUST show a time chart of dBA values covering the last 90 minutes at the bottom of the window, discarding older values.
- **FR-014**: The application MUST inform the user clearly when no audio input is available or measurement fails, and MUST NOT display stale or fabricated values as live readings.
- **FR-015**: The application MUST indicate when no calibration exists and values are therefore approximate.
- **FR-016**: The application MUST warn the user when recorded calibration points are not monotonically increasing and allow correcting the step.

### Key Entities

- **Audio Input**: A selectable source of sound available on the system, identified by a name; one is active at a time.
- **Measurement**: A level value for a 0.5-second interval (raw input level, converted dBA value, timestamp).
- **Calibration Mapping**: An ordered set of pairs (reference dBA value, recorded input level) for 40–105 dBA in 5 dBA steps, used for conversion.
- **Level Zone**: A named dBA range (green, yellow, red) with its color, bounded by two user-editable limits (defaults 70 and 85 dBA).
- **History**: The sequence of measurements for the most recent 90 minutes.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Within 1 second of a sound level change, the displayed value and meter color reflect the change.
- **SC-002**: The displayed value is updated every 0.5 seconds (±0.1 s) during continuous operation.
- **SC-003**: After calibration, displayed values at each calibrated step are within 1 dBA of the target value, and intermediate values change monotonically with input level.
- **SC-004**: A user can complete the full 14-step calibration in under 5 minutes of active interaction.
- **SC-005**: The meter window remains visible above other applications in 100% of normal usage, and stays readable at all window sizes down to the minimum size.
- **SC-006**: The chart shows a continuous 90-minute history after 90 minutes of operation, with no more than 90 minutes of data displayed.
- **SC-007**: The application runs continuously for 8 hours without becoming unresponsive or needing a restart.
- **SC-008**: A first-time user can select an audio input and see a live reading within 1 minute of starting the app.

## Assumptions

- The target platform is Windows desktop (per project constitution), single user, single window.
- The reported value is an approximate dBA reading derived from user calibration, not a certified measurement.
- Zone boundaries are inclusive of the lower zone: a value exactly at the green/yellow limit is green and exactly at the yellow/red limit is yellow.
- The meter scale is linear and covers the calibrated range of 40–105 dBA; values outside are clamped for display and calculation.
- Interpolation between calibration points is linear.
- The 0.5-second measurement is the average level (magnitude) of the mono-mixed signal over the interval.
- History is kept in memory only and starts empty at each launch.
- Settings (selected input, calibration, window size/position) are stored per user.
- The calibration step range (40–105 dBA in 5 dBA steps) is fixed in this version and is not user-configurable.
- A calibration mapping is tied to the input device and gain it was created with; the user is expected to recalibrate when these change.
