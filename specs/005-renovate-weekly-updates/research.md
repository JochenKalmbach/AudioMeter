# Research: Weekly Renovate Updates

## Decision 1: Hosted Renovate GitHub App with a repo config file
- Rationale: No workflow, token or secret to maintain; Renovate runs on its own infrastructure and opens PRs automatically.
- Alternatives: Self-hosted `renovatebot/github-action` on a cron (needs a PAT/App token, uses Actions minutes); Dependabot (not requested).

## Decision 2: Config location `.github/renovate.json`
- Rationale: Recognised by Renovate; keeps repo root clean.
- Alternatives: `renovate.json` at root (equally valid).

## Decision 3: Weekly schedule via `schedule: ["before 6am on monday"]`, `timezone: "Europe/Berlin"`
- Rationale: Native Renovate schedule limits when branches/PRs are created, matching "once a week". Renovate itself polls frequently, but only acts in the window. The window is the only time PRs are created or updated.
- Alternatives: Cron-triggered self-hosted run (more infrastructure).

## Decision 4: Preset `config:recommended`, managers `nuget` and `github-actions` (both on by default)
- Rationale: Covers package references in the .csproj files and `actions/*` in workflows. Dependency Dashboard enabled by default gives visibility.

## Decision 5: Grouping and limits
- Minor/patch grouped via `packageRules` (`matchUpdateTypes: ["minor","patch"]`, `groupName: "all non-major dependencies"`); major updates remain separate PRs labelled `major`.
- `prConcurrentLimit: 5`, `prHourlyLimit` default.
- `automerge` left false (clarification: PR creation only).

## Decision 6: Verification
- PRs target `main`, which triggers the existing PR Build workflow (restore, build, test). `renovate-config-validator` (via `npx --package renovate renovate-config-validator`) checks syntax.
- Note: PR Build uses `pull_request`; PRs opened by the Renovate App trigger it normally (the App is not `GITHUB_TOKEN`). Workflow approval for first-time bot contributors may be required in repo settings.

All unknowns resolved.
