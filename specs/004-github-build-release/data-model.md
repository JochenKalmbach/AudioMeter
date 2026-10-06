# Data Model

No persistent data. Conceptual entities from the spec:

- **Pull request build**: trigger = PR to `main` (opened, synchronize, reopened); outputs = pass/fail check, logs. Fails if build or any test fails.
- **Release package**: `AudioMeter-<version>-win.zip` produced from `dotnet publish -c Release`; stored as run artifact (and release asset when published).
- **Release**: tag `v1.0.<run_number>` targeting the triggering commit; one per successful `main` run; contains the Release package.

State transitions (main run): triggered -> built -> tested -> packaged -> artifact uploaded -> release published. Any failure before "packaged" stops the run with no release; failure at "published" leaves the artifact available.
