using AudioMeter.Core.Levels;

namespace AudioMeter.Core.Tests;

public class LevelZonesTests
{
    [Theory]
    [InlineData(40, Zone.Green)]
    [InlineData(70, Zone.Green)]
    [InlineData(70.1, Zone.Yellow)]
    [InlineData(85, Zone.Yellow)]
    [InlineData(85.1, Zone.Red)]
    [InlineData(105, Zone.Red)]
    public void DefaultBoundaries(double dba, Zone expected)
    {
        Assert.Equal(expected, LevelZones.Default.ZoneOf(dba));
    }

    [Theory]
    [InlineData(60, 80, true)]
    [InlineData(40, 105, true)]
    [InlineData(80, 80, false)]
    [InlineData(90, 80, false)]
    [InlineData(30, 100, false)]
    [InlineData(60, 110, false)]
    [InlineData(double.NaN, 80, false)]
    public void Validation(double gy, double yr, bool valid)
    {
        Assert.Equal(valid, LevelZones.TryCreate(gy, yr, out var z, out var error));
        Assert.Equal(valid, z is not null);
        Assert.Equal(!valid, error is not null);
    }
}
