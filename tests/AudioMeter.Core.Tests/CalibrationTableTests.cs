using AudioMeter.Core.Calibration;

namespace AudioMeter.Core.Tests;

public class CalibrationTableTests
{
    private static List<CalibrationPoint> Points() =>
        Enumerable.Range(0, 14).Select(i => new CalibrationPoint(40 + i * 5, -80 + i * 6.0)).ToList();

    private static CalibrationTable Table()
    {
        Assert.True(CalibrationTable.TryCreate(Points(), out var t, out _));
        return t!;
    }

    [Fact]
    public void ExactPoints_ReturnTargetDba()
    {
        var t = Table();
        foreach (var p in Points())
        {
            Assert.Equal(p.Dba, t.ToDba(p.LevelDbfs), 6);
        }
    }

    [Fact]
    public void Midpoint_IsInterpolated()
    {
        Assert.Equal(42.5, Table().ToDba(-77), 6);
    }

    [Fact]
    public void OutOfRange_IsClamped()
    {
        var t = Table();
        Assert.Equal(40, t.ToDba(-100));
        Assert.Equal(105, t.ToDba(0));
    }

    [Fact]
    public void NonIncreasingLevels_AreRejected()
    {
        var pts = Points();
        pts[5] = pts[5] with { LevelDbfs = pts[4].LevelDbfs };
        Assert.False(CalibrationTable.TryCreate(pts, out _, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public void WrongPointCount_IsRejected()
    {
        Assert.False(CalibrationTable.TryCreate(Points().Take(13), out _, out _));
    }

    [Fact]
    public void Default_IsUncalibratedAndSpansScale()
    {
        var d = CalibrationTable.Default;
        Assert.False(d.IsCalibrated);
        Assert.Equal(40, d.ToDba(-90), 6);
        Assert.Equal(105, d.ToDba(0), 6);
        Assert.True(Table().IsCalibrated);
    }
}
