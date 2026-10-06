# Research: Calibration Input Level

## Capture endpoint and level control

- **Decision**: Use NAudio's `MMDevice.AudioEndpointVolume.MasterVolumeLevelScalar` to read and set Windows capture-endpoint input level. Convert between the API's normalized 0.0–1.0 float and the displayed integer 0–100 by rounding on read and dividing by 100 on write. Subscribe to `AudioEndpointVolume.OnVolumeNotification` so changes made by the calibration slider or Windows Sound settings update the shared displayed value.
- **Rationale**: The application already uses NAudio and stable WASAPI endpoint IDs. NAudio 3.1.0 exposes the capture endpoint's scalar volume and native change notifications; its own volume-mixer sample uses this percentage conversion. No separate app input-level dialog or audio dependency is needed: the calibration slider controls the same device setting users can adjust in Windows Sound settings.
- **Alternatives considered**: Store an application-only pre-amplifier multiplier (would not change the device input setting requested); create a second independent application slider (unnecessary and could diverge); use the dB-based endpoint property (not equivalent to the Windows 0–100 slider); add a new audio library (unnecessary for existing NAudio integration).
- **Threading and lifetime**: Notifications may arrive on a COM worker thread unless NAudio captured a synchronization context when the endpoint volume object was created. Marshal UI updates to the WinForms thread. Dispose the owning `MMDevice`/volume object when changing devices or shutting down to unregister its native callback.
- **Failure behavior**: Accessing endpoint volume or setting the scalar can fail for unavailable/disconnected endpoints or unsupported controls. Surface the failure and do not show the requested value as applied. Device removal must release the old endpoint and its notification callback.

## Calibration target order and table representation

- **Decision**: Change the session's next target to `105 - StepIndex × 5`, capture points high-to-low, and reverse the completed points before constructing the calibration table. Keep `CalibrationTable`'s canonical representation ascending from 40 through 105 dBA with strictly increasing dBFS values.
- **Rationale**: This preserves table interpolation, serialization, and level ordering rules already used by the meter while letting the user set gain at the maximum reference level first.
- **Alternatives considered**: Store the table in descending order (would require changing table validation and interpolation); sort captured points after capture (unnecessarily obscures the session's guaranteed target sequence).

## Calibration metadata and saved settings

- **Decision**: Keep the existing `calibration` array and existing `inputDeviceId` setting for backward compatibility. Add the selected input-level value plus separate calibration-associated device ID and input-level fields. On a successful calibration, capture the device and level at confirmation of the 105 dBA point and persist that metadata with the completed mapping. When valid metadata exists, startup selects that calibration device and restores its level before using the mapping.
- **Rationale**: Existing settings with a bare calibration array continue to deserialize and be used without attempting a destructive schema migration. Separate associated metadata also distinguishes the level currently configured in settings from the gain under which the saved calibration was recorded, allowing stale mappings to be identified.
- **Alternatives considered**: Replace the array with a calibration object (requires custom legacy JSON migration); store only the current input level (loses the gain at which the saved mapping was recorded after subsequent edits).
- **Legacy behavior**: When metadata is absent, preserve the existing selected input device and current system endpoint level and use the historical calibration as before. Do not manufacture calibration metadata.

## Endpoint state and calibration lifecycle

- **Decision**: Keep one endpoint-volume service at the application level and route its value notifications to the calibration dialog. The calibration slider is editable only before confirmation of the first (105 dBA) point. Any gain change after that point invalidates the in-progress session and requires restarting calibration at 105 dBA; changing the level after a saved calibration marks that calibration as requiring recalibration. Cancel before the first point restores the pre-session value.
- **Rationale**: The gain is an input to every captured calibration point. Requiring a restart prevents a mixed-gain mapping. Tracking the calibration's saved gain makes later changes visible as stale rather than silently miscalibrated.
- **Alternatives considered**: Add a separate in-app input-level dialog (duplicates the calibration slider and is not needed for synchronization with Windows Sound settings); allow gain changes midway and save points together (produces an internally inconsistent mapping); silently retain the old mapping as valid after gain changes (violates calibration assumptions).

## Validation and runtime

- **Decision**: Keep pure session and persistence rules in xUnit tests; use a Windows manual scenario for NAudio endpoint read/write/notification and startup restore.
- **Rationale**: Core ordering and old-settings compatibility are deterministic and portable within the existing test project. Real endpoint-volume support and device-removal behavior depend on Windows audio hardware and must be checked against actual devices.
- **Alternatives considered**: Mocking endpoint APIs for all validation (cannot prove synchronization with Windows sound settings); introducing a new integration-test framework (adds scope without improving this targeted check).
