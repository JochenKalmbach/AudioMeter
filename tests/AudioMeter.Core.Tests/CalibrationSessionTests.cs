using AudioMeter.Core.Calibration;

namespace AudioMeter.Core.Tests;

public class CalibrationSessionTests
{
    [Fact]
    public void StepsRun40To105In14Steps()
    {
        var s = new CalibrationSession();
        for (int i = 0; i < 13; i++)
        {
            Assert.Equal(40 + i * 5, s.TargetDba);
            Assert.Equal(ConfirmResult.Advanced, s.Confirm(-80 + i * 5));
        }
        Assert.Equal(105, s.TargetDba);
        Assert.Equal(ConfirmResult.Completed, s.Confirm(0));
        Assert.NotNull(s.Result);
        Assert.True(s.Result!.IsCalibrated);
    }

    [Fact]
    public void NonIncreasingLevel_IsRejectedWithoutAdvancing()
    {
        var s = new CalibrationSession();
        s.Confirm(-60);
        Assert.Equal(ConfirmResult.NotIncreasing, s.Confirm(-60));
        Assert.Equal(ConfirmResult.NotIncreasing, s.Confirm(-70));
        Assert.Equal(1, s.StepIndex);
        Assert.Equal(45, s.TargetDba);
        Assert.Equal(ConfirmResult.Advanced, s.Confirm(-59));
    }

    [Fact]
    public void Cancelled_SessionHasNoResult()
    {
        var s = new CalibrationSession();
        s.Confirm(-60);
        Assert.Null(s.Result);
    }
}
