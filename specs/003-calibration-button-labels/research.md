# Research: Calibration Dialog Button Labels

## Decision 1: Where the "last step" rule lives
- **Decision**: Add `IsLastStep` (`StepIndex == StepCount - 1`) to `CalibrationSession` in AudioMeter.Core.
- **Rationale**: `CalibrationSession` already owns `StepIndex`/`StepCount` and completion; keeping the rule there satisfies the UI/logic separation principle and is unit-testable.
- **Alternatives considered**: Compute inline in the dialog (untestable, duplicates step logic).

## Decision 2: When the label updates
- **Decision**: Set the button text in the dialog's `ShowStep()`, which runs at construction and after each advance.
- **Rationale**: Covers first point and every advance. The dialog has no in-place restart: an invalidated session requires cancelling and reopening, which builds a new dialog starting at "Next" (FR-006).
- **Alternatives considered**: Data binding (overkill for one label).

## Decision 3: Wording
- **Decision**: Labels "Next" and "Save"; the instruction text that says "press OK" is changed to "press Next (Save on the last step)".
- **Rationale**: FR-003 forbids showing "OK"; stale instruction text would contradict the button.

## Decision 4: Enter key / DialogResult
- **Decision**: No change; the button stays the AcceptButton and only the final confirm sets `DialogResult.OK`.
- **Rationale**: Behaviour is unchanged (FR-004, FR-005); only the label differs.
