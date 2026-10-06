# Feature Specification: Weekly Automated Dependency Updates (Renovate)

**Feature Branch**: `005-renovate-weekly-updates`

**Created**: 2026-10-06

**Status**: Draft

**Input**: User description: "Can you add Renovate to the project and trigger that once a week..."

## Clarifications

### Session 2026-10-06

- Q: Besides automatically opening the pull request, should updates also be merged automatically? → A: No. Pull requests are created automatically; merging stays manual.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Receive weekly dependency update proposals (Priority: P1)

As the maintainer, I want outdated dependencies of the project to be checked automatically once a week and proposed as reviewable change requests, so the project stays current and secure without manual tracking.

**Why this priority**: This is the core value of the feature.

**Independent Test**: Enable the automation, wait for (or manually trigger) a weekly run against a repository with an outdated dependency, and confirm a change request is opened for it.

**Acceptance Scenarios**:

1. **Given** the project has an outdated dependency, **When** the weekly run occurs, **Then** a change request proposing the newer version is opened.
2. **Given** all dependencies are current, **When** the weekly run occurs, **Then** no change requests are opened.
3. **Given** a weekly run already opened a change request that is still unmerged, **When** the next weekly run occurs, **Then** the existing request is updated rather than duplicated.

---

### User Story 2 - Existing build checks validate update proposals (Priority: P2)

As the maintainer, I want every proposed update to go through the project's existing build verification, so I can merge with confidence.

**Why this priority**: Updates are only safe to merge if verified.

**Independent Test**: Open an automated update proposal and confirm the existing build check runs on it.

**Acceptance Scenarios**:

1. **Given** an automated update proposal is opened, **When** it is created, **Then** the standard pull-request build verification runs on it.

---

### User Story 3 - Cover all kinds of dependencies in the project (Priority: P3)

As the maintainer, I want package dependencies as well as the automation workflow actions to be kept current.

**Why this priority**: Broadens coverage, but the weekly mechanism works without it.

**Independent Test**: Verify that an outdated package dependency and an outdated workflow action each produce a proposal.

**Acceptance Scenarios**:

1. **Given** an outdated workflow action version, **When** the weekly run occurs, **Then** an update proposal is opened for it.

### Edge Cases

- A weekly run finds many updates at once: proposals are limited/grouped so the maintainer is not flooded.
- A proposed update fails the build: it stays open, visibly failing, and is never merged automatically.
- The automation is paused or the service is unavailable for a week: the next run catches up on all pending updates.
- A major-version update is available: it is proposed separately and clearly labelled.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The repository MUST include configuration enabling Renovate dependency updates.
- **FR-002**: Dependency checks MUST run once per week, at a defined time window (default: early Monday morning, Europe/Berlin time).
- **FR-003**: Update proposals MUST be created only within the weekly schedule window.
- **FR-004**: Each update MUST be proposed by automatically creating a pull request (no manual step to open it); pull requests MUST NOT be merged automatically.
- **FR-005**: Proposals MUST trigger the project's existing pull-request build verification.
- **FR-006**: The configuration MUST cover package dependencies and GitHub workflow actions used by the project.
- **FR-007**: Minor/patch updates SHOULD be grouped into a single weekly proposal; major updates MUST be proposed separately.
- **FR-008**: The number of concurrently open automated proposals MUST be limited to a reasonable cap (default: 5).
- **FR-009**: Documentation MUST explain how the schedule works and how to change or disable it.

### Key Entities

- **Update schedule**: The weekly window in which proposals may be created.
- **Update proposal**: A change request proposing a newer version of one or more dependencies.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Within 7 days of a new dependency release, a proposal for it exists (or is updated) without manual action.
- **SC-002**: At most one batch of grouped minor/patch updates is opened per week.
- **SC-003**: 100% of automated proposals have build verification results before review.
- **SC-004**: Zero updates are merged without explicit maintainer approval.
- **SC-005**: A maintainer can change or pause the schedule in under 5 minutes using the documentation.

## Assumptions

- The repository is hosted on GitHub, and the maintainer will install/authorize the hosted Renovate GitHub App (or equivalent) for it; this cannot be done from within the repository.
- "Once a week" means a fixed weekday and time; Monday before 06:00 Europe/Berlin is assumed.
- The existing pull-request build workflow is the verification gate.
- Automatic merging is out of scope (confirmed in clarification); only pull request creation is automatic.

