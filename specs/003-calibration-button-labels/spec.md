# Feature Specification: Calibration Dialog Button Labels

**Feature Branch**: `[003-calibration-button-labels]`

**Created**: 2026-10-06

**Status**: Draft

**Input**: User description: "In the calibration dialog: Please rename the "Ok" button to "Next" until all dbA values are calibrated. Only the last dialog should have an "Save" button"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Clear progression through calibration points (Priority: P1)

While calibrating, the user steps through the reference levels one at a time. For every point except the last, the confirm button reads "Next", signalling that more points remain. Only at the final point does the button read "Save", signalling that confirming completes and stores the calibration.

**Why this priority**: The button label is the user's only cue whether confirming will advance or finish the calibration; a generic "OK" is ambiguous.

**Independent Test**: Start a calibration and step through every point, checking the confirm button label at each step.

**Acceptance Scenarios**:

1. **Given** the calibration dialog shows any point other than the last one, **When** the user looks at the confirm button, **Then** it is labelled "Next" and not "OK" or "Save".
2. **Given** the calibration dialog shows the last point, **When** the user looks at the confirm button, **Then** it is labelled "Save".
3. **Given** the user confirms a point labelled "Next", **When** the action completes, **Then** the dialog advances to the next calibration point without saving the calibration.
4. **Given** the user confirms the last point labelled "Save", **When** the action completes, **Then** the calibration is saved and the dialog closes.

---

### User Story 2 - Label stays correct when the sequence is restarted (Priority: P2)

If the calibration is restarted (for example because the input level changed), the button label reflects the new position in the sequence.

**Why this priority**: Prevents a stale "Save" label from appearing on an early point.

**Independent Test**: Reach the last point, trigger a restart, and verify the button reads "Next" again on the first point.

**Acceptance Scenarios**:

1. **Given** the calibration restarts at its first point, **When** the first point is shown, **Then** the confirm button reads "Next".

### Edge Cases

- The button never reads "OK" at any point during calibration.
- The cancel action is unaffected and keeps its existing label and behaviour.
- If a calibration consists of only one point, that point is the last and its button reads "Save".

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The calibration dialog's confirm button MUST be labelled "Next" for every calibration point except the last one.
- **FR-002**: The calibration dialog's confirm button MUST be labelled "Save" only on the last calibration point.
- **FR-003**: The confirm button MUST NOT be labelled "OK" at any point in the calibration.
- **FR-004**: Confirming a point labelled "Next" MUST advance to the next point and MUST NOT save the calibration.
- **FR-005**: Confirming the point labelled "Save" MUST save the calibration and close the dialog.
- **FR-006**: The button label MUST update immediately whenever the displayed point changes, including after a restart of the sequence.

### Key Entities

- **Calibration point**: One reference level (dBA) to be captured; its position in the sequence (last or not) determines the confirm button label.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of calibration runs, the confirm button reads "Next" on every point except the last and "Save" on the last.
- **SC-002**: Users can tell without trial whether confirming will continue or finish the calibration (no "OK" label is ever shown).
- **SC-003**: No calibration is saved before the "Save" button on the last point is confirmed.

## Assumptions

- The calibration sequence's current order and set of points (see spec 002) are unchanged; only the button label changes.
- Labels are shown in English, matching the existing UI language.
- The existing cancel behaviour and label remain as they are.
