using NAudio.CoreAudioApi;

namespace AudioMeter.App.Audio;

/// <summary>Controls the normalized capture volume for one Windows endpoint.</summary>
public sealed class InputLevelService : IDisposable
{
    private readonly MMDevice _device;
    private readonly string _deviceId;
    private bool _disposed;

    public InputLevelService(string deviceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        using var enumerator = new MMDeviceEnumerator();
        _device = enumerator.GetDevice(deviceId);
        _deviceId = _device.ID;
        try
        {
            if (_device.State != DeviceState.Active)
            {
                throw new InvalidOperationException("The selected audio input is unavailable.");
            }
            _ = _device.AudioEndpointVolume.MasterVolumeLevelScalar;
            _device.AudioEndpointVolume.OnVolumeNotification += OnVolumeNotification;
        }
        catch
        {
            _device.Dispose();
            throw;
        }
    }

    public event Action<string, int>? LevelChanged;

    public string DeviceId => _deviceId;

    public int CurrentLevel
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return ToInputLevel(_device.AudioEndpointVolume.MasterVolumeLevelScalar);
        }
    }

    public int SetLevel(int inputLevel)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (inputLevel is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(inputLevel), "Input level must be between 0 and 100.");
        }
        _device.AudioEndpointVolume.MasterVolumeLevelScalar = inputLevel / 100f;
        return CurrentLevel;
    }

    private void OnVolumeNotification(AudioVolumeNotificationData notification)
    {
        LevelChanged?.Invoke(_deviceId, ToInputLevel(notification.MasterVolume));
    }

    private static int ToInputLevel(float scalar) =>
        Math.Clamp((int)Math.Round(scalar * 100, MidpointRounding.AwayFromZero), 0, 100);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        try
        {
            _device.AudioEndpointVolume.OnVolumeNotification -= OnVolumeNotification;
        }
        finally
        {
            _device.Dispose();
        }
    }
}
