using AudioMeter.Core.Calibration;

namespace AudioMeter.Core.Tests;

public class CalibrationSessionTests
{
    [Fact]
    public void StepsRun105To40AndResultPointsRemainAscending()
    {
        var s = new CalibrationSession();
        for (int i = 0; i < 13; i++)
        {
            Assert.Equal(105 - i * 5, s.TargetDba);
            Assert.Equal(ConfirmResult.Advanced, s.Confirm(-i * 5));
        }
        Assert.Equal(40, s.TargetDba);
        Assert.Equal(ConfirmResult.Completed, s.Confirm(-65));
        Assert.NotNull(s.Result);
        Assert.True(s.Result!.IsCalibrated);
        Assert.Equal(Enumerable.Range(0, 14).Select(i => 40 + i * 5).ToArray(), s.Result.Points.Select(point => point.Dba).ToArray());
        Assert.Equal(Enumerable.Range(0, 14).Select(i => -65.0 + i * 5).ToArray(), s.Result.Points.Select(point => point.LevelDbfs).ToArray());
    }

    [Fact]
    public void NonDecreasingLevel_IsRejectedWithoutAdvancing()
    {
        var s = new CalibrationSession();
        s.Confirm(0);
        Assert.Equal(ConfirmResult.NotDecreasing, s.Confirm(0));
        Assert.Equal(ConfirmResult.NotDecreasing, s.Confirm(1));
        Assert.Equal(1, s.StepIndex);
        Assert.Equal(100, s.TargetDba);
        Assert.Equal(ConfirmResult.Advanced, s.Confirm(-1));
    }

    [Fact]
    public void Cancelled_SessionHasNoResult()
    {
        var s = new CalibrationSession();
        s.Confirm(0);
        Assert.Null(s.Result);
    }

    [Fact]
    public void InputLevelCanBeChangedOnlyBeforeFirstConfirmation()
    {
        var s = new CalibrationSession();

        Assert.True(s.CanAdjustInputLevel);
        Assert.Equal(ConfirmResult.Advanced, s.Confirm(0, 65));
        Assert.False(s.CanAdjustInputLevel);
        Assert.Equal(65, s.InputLevelAtFirstPoint);
    }

    [Fact]
    public void InputLevelChangeAfterFirstConfirmationInvalidatesSession()
    {
        var s = new CalibrationSession();
        s.Confirm(0, 65);

        s.NotifyInputLevelChanged(66);

        Assert.True(s.IsInvalidated);
        Assert.Equal(ConfirmResult.Invalidated, s.Confirm(-1, 66));
        Assert.Equal(1, s.StepIndex);
        Assert.Null(s.Result);
    }
}
