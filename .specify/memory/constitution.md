<!--
Sync Impact Report
Version change: Uninitialized scaffold -> 1.0.0
Modified principles: Undefined -> I. Windows Desktop Platform; II. Separation of UI and Audio Processing; III. Measurement Correctness; IV. Responsive and Resilient Device Lifecycle; V. Automated Quality and Maintainability
Added sections: Technology and Runtime Constraints; Development Workflow and Quality Gates
Removed sections: None
Follow-up TODOs: Record the original ratification date when known.
-->

# AudioMeter Constitution

## Core Principles

### I. Windows Desktop Platform
The application MUST target .NET 10 for Windows using Windows Forms (`net10.0-windows`).
The user interface MUST use Windows Forms controls and supported Windows APIs; other UI
frameworks or target platforms require a constitution amendment. This keeps the delivery
focused on the requested Windows desktop application.

### II. Separation of UI and Audio Processing
Windows Forms event handlers MUST coordinate user interaction, not own measurement or device
processing logic. Audio capture, level calculations, and other application behavior MUST be
implemented in units that can be exercised independently of the form wherever practical.
This separation keeps behavior understandable and allows core logic to be tested without
opening the UI.

### III. Measurement Correctness
Level calculations MUST define their input assumptions, units, and output range. Changes to
measurement behavior MUST include tests for representative signal levels and boundary cases,
including silence and clipping where applicable. Results MUST NOT silently substitute
success-shaped values for unavailable or invalid audio data. Correct readings are the
application's central promise.

### IV. Responsive and Resilient Device Lifecycle
Audio capture and other potentially blocking work MUST NOT run synchronously on the Windows
Forms UI thread. Device start, stop, loss, and recovery paths MUST leave resources disposed
and the UI in an explicit, recoverable state. User-visible failures MUST be reported clearly.
This prevents audio-device conditions from freezing or misleading the user interface.

### V. Automated Quality and Maintainability
New or changed non-UI behavior MUST have automated tests. Windows- and device-dependent
behavior MUST be validated with focused integration or manual checks when it cannot be
reliably covered by unit tests. Implementations MUST prefer the simplest design that meets
the requirement and MUST avoid duplicating audio, device, or UI state logic.

## Technology and Runtime Constraints

The supported application target is Windows on .NET 10 with Windows Forms. Project and build
configuration MUST make the Windows target explicit. External packages and Windows APIs MUST
be selected for compatibility with that target, and dependencies MUST be limited to those
needed for a stated requirement. Platform-dependent code MUST fail visibly with a useful
message when the required capability is unavailable.

## Development Workflow and Quality Gates

Before a change is complete, developers MUST build the affected project and run relevant
automated tests. Changes to measurement or device behavior MUST include the tests or checks
required by the corresponding principles. Reviewers MUST verify that UI work remains
responsive, resources have a clear lifecycle, and user-visible errors are not suppressed.
Any intentional exception to a principle MUST be explained and approved as part of review.

## Governance

This constitution governs project implementation and review; feature plans and local
conventions MUST remain consistent with it. Amendments require a documented rationale and
review by the project owner. Each amendment MUST update the version, amendment date, and
sync impact report. Versioning follows semantic versioning: a MAJOR increment denotes
backward-incompatible governance changes, a MINOR increment adds or materially expands
principles or sections, and a PATCH increment clarifies existing rules without changing
their intent. Every change review MUST assess compliance with the applicable principles;
exceptions MUST be explicit rather than silently accepted.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): original adoption date not recorded | **Last Amended**: 2026-10-06
