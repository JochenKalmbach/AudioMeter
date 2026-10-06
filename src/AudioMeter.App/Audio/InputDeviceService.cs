using NAudio.CoreAudioApi;

namespace AudioMeter.App.Audio;

public sealed record InputDevice(string Id, string Name, bool IsDefault);

public static class InputDeviceService
{
    public static IReadOnlyList<InputDevice> List()
    {
        using var enumerator = new MMDeviceEnumerator();
        string? defaultId = null;
        try
        {
            defaultId = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console).ID;
        }
        catch (Exception)
        {
            // No default capture device present.
        }
        return enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
            .Select(d => new InputDevice(d.ID, d.FriendlyName, d.ID == defaultId))
            .ToList();
    }
}
