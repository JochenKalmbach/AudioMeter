# Contract: Workflows

## pr-build.yml
- **Trigger**: `pull_request` with `branches: [main]` (types: opened, synchronize, reopened).
- **Permissions**: `contents: read`.
- **Job** `build` on `windows-latest`: checkout, setup .NET 10, restore, build Release, run unit tests.
- **Result**: check named `build` is success only if build and all tests pass. Never publishes.

## release.yml
- **Trigger**: `push` with `branches: [main]`.
- **Permissions**: `contents: write`.
- **Concurrency**: group `release`, no cancel.
- **Job** `release` on `windows-latest`: checkout, setup .NET 10, restore, build Release, run unit tests, publish app, zip, upload artifact, create release.
- **Version**: `v1.0.${{ github.run_number }}`.
- **Outputs**: run artifact `AudioMeter-<version>-win` (ZIP); GitHub Release `<version>` with ZIP asset and generated notes.
- **Failure behavior**: build/test failure -> no artifact, no release; release creation failure -> artifact remains.
