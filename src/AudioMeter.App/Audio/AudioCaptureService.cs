using AudioMeter.Core;
using AudioMeter.Core.Levels;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace AudioMeter.App.Audio;

/// <summary>
/// WASAPI capture of one input. Events are raised on the capture thread; callers must marshal to the UI thread.
/// </summary>
public sealed class AudioCaptureService : IDisposable
{
    private WasapiCapture? _capture;

    /// <summary>Mean absolute mono level in dBFS of one 0.5 s window.</summary>
    public event Action<double>? LevelMeasured;

    public event Action<string>? Failed;

    public bool IsRunning => _capture is not null;

    /// <summary>Starts capture. Returns a notice when the requested device was unavailable and the default was used.</summary>
    public string? Start(string? deviceId)
    {
        Stop();
        using var enumerator = new MMDeviceEnumerator();
        MMDevice? device = null;
        string? notice = null;
        if (!string.IsNullOrEmpty(deviceId))
        {
            try
            {
                device = enumerator.GetDevice(deviceId);
                if (device.State != DeviceState.Active)
                {
                    device = null;
                }
            }
            catch (Exception)
            {
                device = null;
            }
            if (device is null)
            {
                notice = "Selected input is unavailable; using the default input.";
            }
        }
        device ??= enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);

        var capture = new WasapiCapture(device);
        var format = capture.WaveFormat is WaveFormatExtensible ext ? ext.ToStandardWaveFormat() : capture.WaveFormat;
        bool isFloat = format.Encoding == WaveFormatEncoding.IeeeFloat && format.BitsPerSample == 32;
        bool isPcm16 = format.Encoding == WaveFormatEncoding.Pcm && format.BitsPerSample == 16;
        if (!isFloat && !isPcm16)
        {
            capture.Dispose();
            throw new NotSupportedException($"Unsupported input format: {capture.WaveFormat}.");
        }

        var calculator = new LevelCalculator(format.SampleRate);
        int channels = format.Channels;
        float[] scratch = Array.Empty<float>();

        capture.DataAvailable += (sender, e) =>
        {
            if (!ReferenceEquals(sender, _capture))
            {
                return;
            }
            ReadOnlySpan<float> samples;
            if (isFloat)
            {
                samples = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, float>(e.Buffer.AsSpan(0, e.BytesRecorded));
            }
            else
            {
                int count = e.BytesRecorded / 2;
                if (scratch.Length < count)
                {
                    scratch = new float[count];
                }
                for (int i = 0; i < count; i++)
                {
                    scratch[i] = BitConverter.ToInt16(e.Buffer, i * 2) / 32768f;
                }
                samples = scratch.AsSpan(0, count);
            }
            calculator.Add(samples, channels, level => LevelMeasured?.Invoke(level));
        };
        capture.RecordingStopped += (sender, e) =>
        {
            if (!ReferenceEquals(sender, _capture))
            {
                return;
            }
            Failed?.Invoke(e.Exception is null ? "The audio input stopped." : $"Audio input failed: {e.Exception.Message}");
        };

        _capture = capture;
        try
        {
            capture.StartRecording();
        }
        catch
        {
            _capture = null;
            capture.Dispose();
            throw;
        }
        return notice;
    }

    public void Stop()
    {
        var capture = _capture;
        _capture = null;
        if (capture is null)
        {
            return;
        }
        try
        {
            capture.StopRecording();
        }
        catch (Exception)
        {
            // The device may already be gone; disposal below still releases it.
        }
        capture.Dispose();
    }

    public void Dispose() => Stop();
}
