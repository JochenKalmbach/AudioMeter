# Quickstart: Validating the Pipeline

Prerequisites: workflows merged to `main`; repository setting Actions > Workflow permissions allows read/write (or the release job's explicit `contents: write` is honored).

1. **PR validation**: create a branch, change any file, open a PR to `main`. Expect the `build` check to run and pass. Push a commit that breaks a unit test; expect the check to fail with test names in the log (SC-002).
2. **Merge gate (optional)**: add a branch protection rule on `main` requiring the `build` check.
3. **Release**: merge the PR. Expect the "Release" workflow to run, upload artifact `AudioMeter-v1.0.N-win`, and create release `v1.0.N` with the ZIP.
4. **Verify ZIP**: download, extract on Windows with the .NET 10 Desktop Runtime installed, run `AudioMeter.App.exe`.
5. **Uniqueness**: merge two commits quickly; expect two distinct releases.
6. **Local dry run**: `dotnet build AudioMeter.sln -c Release; dotnet test AudioMeter.sln -c Release --no-build; dotnet publish src/AudioMeter.App -c Release -o publish`.
