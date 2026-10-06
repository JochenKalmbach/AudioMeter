using AudioMeter.Core.Levels;

namespace AudioMeter.Core.Tests;

public class LevelCalculatorTests
{
    private static List<double> Run(int sampleRate, int channels, float[] samples)
    {
        var results = new List<double>();
        new LevelCalculator(sampleRate).Add(samples, channels, results.Add);
        return results;
    }

    [Fact]
    public void Silence_ReturnsFloor()
    {
        var r = Run(100, 1, new float[50]);
        Assert.Equal(Constants.SilenceFloorDbfs, Assert.Single(r));
    }

    [Fact]
    public void FullScale_ReturnsZeroDbfs()
    {
        var samples = Enumerable.Repeat(1f, 50).ToArray();
        Assert.Equal(0, Assert.Single(Run(100, 1, samples)), 6);
    }

    [Fact]
    public void Clipping_AboveFullScale_IsCappedAtZero()
    {
        var samples = Enumerable.Repeat(2f, 50).ToArray();
        Assert.Equal(0, Assert.Single(Run(100, 1, samples)), 6);
    }

    [Fact]
    public void Stereo_IsMixedToMonoByAveraging()
    {
        // L = 1, R = 0 → mono 0.5 → about -6.02 dBFS
        var samples = new float[100];
        for (int i = 0; i < 50; i++) { samples[i * 2] = 1f; }
        Assert.Equal(20 * Math.Log10(0.5), Assert.Single(Run(100, 2, samples)), 6);
    }

    [Fact]
    public void Window_CompletesAfterExactlyHalfSecondOfFrames()
    {
        var calc = new LevelCalculator(100);
        int count = 0;
        calc.Add(new float[49], 1, _ => count++);
        Assert.Equal(0, count);
        calc.Add(new float[1], 1, _ => count++);
        Assert.Equal(1, count);
        calc.Add(new float[100], 1, _ => count++);
        Assert.Equal(3, count);
    }
}
