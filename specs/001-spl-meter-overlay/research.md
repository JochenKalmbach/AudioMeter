# Research: Always-on-Top SPL Meter

## Audio capture

- **Decision**: NAudio with WASAPI shared-mode capture (`WasapiCapture`, `MMDeviceEnumerator`).
- **Rationale**: Mature, MIT-licensed, supports .NET on Windows, enumerates capture endpoints, and raises device-change/stop events needed for FR-014.
- **Alternatives**: CSCore (unmaintained); raw Core Audio COM interop (more code, no benefit); `waveIn` (legacy).

## Level measurement

- **Decision**: Convert each buffer to float samples, mix channels to mono by averaging, accumulate over a 0.5 s window, and take the mean absolute amplitude. Convert to dBFS (`20·log10`), floor at -100 dBFS for silence.
- **Rationale**: Matches the spec ("average of the mono signal"). Working in dBFS makes the calibration table roughly linear in dB, so interpolation is accurate.
- **Alternatives**: RMS (closer to physical SPL but differs from the stated requirement; can be swapped in `LevelCalculator` later); peak (too jumpy).
- **Window timing**: Window length is counted in samples (sampleRate × 0.5), not by wall-clock timers, so the interval stays accurate. A UI timer is not used for measurement.

## Calibration mapping

- **Decision**: Table of (dBA, dBFS) pairs for 40–105 in 5 dBA steps. Conversion is piecewise-linear interpolation from dBFS to dBA, clamped to 40–105. Strictly increasing dBFS is required (FR-016).
- **Rationale**: Simple, deterministic, testable. Without calibration, a default mapping (dBFS -90 → 40, 0 → 105, linear) is used and flagged "uncalibrated".
- **Alternatives**: Regression fit (hides user's measured points); log-domain interpolation (unnecessary since dBFS is already logarithmic).

## "A-weighting"

- **Decision**: No A-weighting filter is applied; "dBA" is the calibrated approximation the user defines through the reference sound source.
- **Rationale**: Not requested; the calibration absorbs device response for the reference signal. Noted as an assumption in the spec ("approximate reading, not certified").
- **Alternatives**: IIR A-weighting filter in `LevelCalculator` as a future enhancement.

## UI and chart

- **Decision**: Custom-painted `MeterControl` and `HistoryChartControl` (double-buffered GDI+), `TopMost = true`, resizable with a minimum size, `MenuStrip` for settings.
- **Rationale**: .NET 10 has no built-in chart control; a small custom control avoids a heavy dependency and fully controls the linear 40–105 scale and gap rendering.
- **Alternatives**: ScottPlot/OxyPlot WinForms controls (extra dependency for a simple line chart).

## History

- **Decision**: In-memory ring of timestamped measurements, pruned to 90 minutes; `null` dBA entries represent gaps.
- **Rationale**: About 10,800 points at 2 Hz; trivially small. Spec states history is not persisted.

## Settings persistence

- **Decision**: `System.Text.Json` to `%APPDATA%\AudioMeter\settings.json`, written atomically (temp file then replace). Corrupt or missing file falls back to defaults with a notice.
- **Alternatives**: `Properties.Settings` / registry (less transparent, harder to test).

## Device identity

- **Decision**: Store the WASAPI endpoint ID (not display name); fall back to the default capture device if missing, and say so.

## Toolchain note

- The machine currently reports only .NET SDK 8.0.300. Building for `net10.0-windows` requires installing the .NET 10 SDK before implementation.
