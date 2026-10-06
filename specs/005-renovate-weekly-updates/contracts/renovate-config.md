# Contract: `.github/renovate.json`

The configuration is the only interface. It MUST satisfy:

| Key | Value | Spec |
|-----|-------|------|
| `$schema` | Renovate config schema URL | validation |
| `extends` | `["config:recommended"]` | FR-001, FR-006 |
| `schedule` | `["before 6am on monday"]` | FR-002, FR-003 |
| `timezone` | `Europe/Berlin` | FR-002 |
| `enabledManagers` | `["nuget", "github-actions"]` | FR-006 |
| `prConcurrentLimit` | `5` | FR-008 |
| `automerge` | `false` | FR-004 |
| `packageRules` | group `minor`+`patch` as "all non-major dependencies"; label major updates `major` and keep separate | FR-007 |
| `labels` | `["dependencies"]` | traceability |

Must NOT contain secrets or tokens.
