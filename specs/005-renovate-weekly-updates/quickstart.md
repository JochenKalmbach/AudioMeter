# Quickstart: Validate Weekly Renovate Updates

## Prerequisites
- Node.js available locally.
- Renovate GitHub App installed on JochenKalmbach/AudioMeter (repository settings, one-time, by the owner).

## Steps
1. Validate the config: `npx --yes --package renovate renovate-config-validator .github/renovate.json` — expect "Config validated successfully".
2. Merge the config to `main`. Renovate opens an onboarding/dependency dashboard issue listing detected NuGet and GitHub Actions dependencies (covers FR-006).
3. In the dashboard, tick the checkbox to force an immediate run of a pending update (or wait for the Monday window); expect a pull request to be opened automatically (FR-004).
4. Confirm the PR triggers "PR Build" and shows results (FR-005, SC-003).
5. Confirm minor/patch updates are in one grouped PR and majors are separate (FR-007), no more than 5 are open (FR-008), and nothing is auto-merged (SC-004).
6. Read the README "Dependency updates" section and confirm it explains changing/pausing the schedule (FR-009, SC-005).
