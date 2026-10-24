using System.Globalization;
using AudioMeter.Core.Calibration;

namespace AudioMeter.Core.Tests;

public class CalibrationTableDraftTests
{
    private static CalibrationTableDraft Default() => CalibrationTableDraft.FromTable(CalibrationTable.Default);

    [Fact]
    public void FromDefault_IsValidAndRoundTrips()
    {
        var draft = Default();
        Assert.True(draft.IsValid);
        Assert.True(draft.TryBuild(out var table));
        for (int i = 0; i < CalibrationTableDraft.RowCount; i++)
        {
            Assert.Equal(CalibrationTable.Default.Points[i].LevelDbfs, table!.Points[i].LevelDbfs, 0.01);
        }
    }

    [Fact]
    public void Boundaries_AreAccepted()
    {
        var draft = Default();
        draft.SetCell(0, "-120");
        draft.SetCell(13, "0");
        Assert.True(draft.IsValid);
    }

    [Theory]
    [InlineData("-120.1", 0)]
    [InlineData("0.1", 13)]
    [InlineData("", 3)]
    [InlineData("abc", 3)]
    [InlineData("NaN", 3)]
    [InlineData("Infinity", 3)]
    public void OutOfRangeOrNonNumeric_IsRejected(string text, int row)
    {
        var draft = Default();
        draft.SetCell(row, text);
        Assert.False(draft.IsValid);
        Assert.NotNull(draft.RowError(row));
        Assert.False(draft.TryBuild(out var table));
        Assert.Null(table);
        Assert.Null(draft.ToDba(-50));
    }

    [Fact]
    public void EqualOrDecreasingNeighbour_FlagsOffendingRow()
    {
        var draft = Default();
        draft.SetCell(5, draft.GetCell(4));
        Assert.NotNull(draft.RowError(5));
        Assert.Null(draft.RowError(4));
        draft.SetCell(5, "-200");
        Assert.NotNull(draft.RowError(5));
    }

    [Fact]
    public void ToDba_MatchesTableWhenValid()
    {
        var draft = Default();
        draft.SetCell(0, "-100");
        Assert.True(draft.TryBuild(out var table));
        Assert.Equal(table!.ToDba(-60), draft.ToDba(-60));
    }

    [Fact]
    public void DecimalSeparator_IsAcceptedForCurrentAndInvariantCulture()
    {
        var draft = Default();
        draft.SetCell(0, "-100.5");
        Assert.True(draft.IsValid);
        draft.SetCell(0, (-100.5).ToString(CultureInfo.CurrentCulture));
        Assert.True(draft.IsValid);
    }
}
