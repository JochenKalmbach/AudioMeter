namespace AudioMeter.Core.Levels;

/// <summary>
/// Mixes interleaved float samples (-1..1) to mono and reports the mean absolute level in dBFS
/// for each window of exactly sampleRate * 0.5 frames.
/// </summary>
public sealed class LevelCalculator
{
    private readonly int _windowFrames;
    private double _sum;
    private int _frames;

    public LevelCalculator(int sampleRate)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sampleRate, 0);
        _windowFrames = (int)Math.Round(sampleRate * Constants.MeasurementIntervalSeconds);
    }

    public void Add(ReadOnlySpan<float> interleaved, int channels, Action<double> onWindowCompleted)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(channels, 1);
        int frameCount = interleaved.Length / channels;
        for (int f = 0; f < frameCount; f++)
        {
            double mono = 0;
            for (int c = 0; c < channels; c++)
            {
                mono += interleaved[f * channels + c];
            }
            _sum += Math.Abs(mono / channels);
            if (++_frames == _windowFrames)
            {
                onWindowCompleted(ToDbfs(_sum / _frames));
                _sum = 0;
                _frames = 0;
            }
        }
    }

    public void Reset()
    {
        _sum = 0;
        _frames = 0;
    }

    public static double ToDbfs(double meanAbsolute)
    {
        if (meanAbsolute <= 0 || double.IsNaN(meanAbsolute))
        {
            return Constants.SilenceFloorDbfs;
        }
        double db = 20 * Math.Log10(Math.Min(meanAbsolute, 1.0));
        return Math.Max(db, Constants.SilenceFloorDbfs);
    }
}
