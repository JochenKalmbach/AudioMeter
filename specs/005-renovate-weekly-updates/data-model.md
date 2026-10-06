# Data Model

No persistent data. Conceptual entities from the spec map to configuration:

| Entity | Representation | Rules |
|--------|----------------|-------|
| Update schedule | `schedule` + `timezone` in `.github/renovate.json` | Weekly window: Monday before 06:00 Europe/Berlin |
| Update proposal | Pull request opened by Renovate against `main` | Created automatically; never auto-merged; minor/patch grouped; major separate; max 5 open |
