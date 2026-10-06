# Research: Build and Release Pipeline

## Runner
- **Decision**: `windows-latest`. **Rationale**: App targets `net10.0-windows` with WinForms; building needs Windows. **Alternatives**: Linux runner with `EnableWindowsTargeting` (rejected: riskier, no run validation).

## .NET SDK
- **Decision**: `actions/setup-dotnet` with `dotnet-version: 10.0.x`. **Rationale**: matches target framework; no `global.json` exists.

## Build and test
- **Decision**: `dotnet restore`, `dotnet build AudioMeter.sln -c Release --no-restore`, `dotnet test AudioMeter.sln -c Release --no-build`. **Rationale**: one solution covers all projects, warnings-as-errors already configured. Running tests on PRs satisfies FR-001; the same steps gate releases (FR-004).

## Packaging
- **Decision**: `dotnet publish src/AudioMeter.App -c Release -o publish` (framework-dependent), then `Compress-Archive` to `AudioMeter-<version>-win.zip`. **Rationale**: matches spec assumption; small download. **Alternatives**: self-contained single-file (out of scope per spec).

## Versioning
- **Decision**: Release tag `v1.0.<github.run_number>` (tag created on the pushed commit). **Rationale**: unique, monotonic, automatic, collision-free across quick successive pushes. **Alternatives**: date+sha (less readable).

## Publishing
- **Decision**: `gh release create <tag> <zip> --target <sha> --generate-notes` using `GITHUB_TOKEN`, job-level `permissions: contents: write`. **Rationale**: no third-party action needed. Releases created with `GITHUB_TOKEN` do not trigger new workflow runs, so no loop (FR-008).
- ZIP also uploaded with `actions/upload-artifact` before release step so it survives publish failure (FR-007).

## Concurrency and forks
- **Decision**: release workflow `concurrency: group: release, cancel-in-progress: false` so releases are serialized. PR workflow uses `pull_request` (not `pull_request_target`), read-only `contents: read` permissions; fork PRs never access write tokens.

## Open items
- PR checks blocking merge requires a branch protection rule on `main` (repository setting, documented in quickstart; not controllable from workflow files).
