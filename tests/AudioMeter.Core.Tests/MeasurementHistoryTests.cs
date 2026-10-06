using AudioMeter.Core.History;
using AudioMeter.Core.Levels;

namespace AudioMeter.Core.Tests;

public class MeasurementHistoryTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Prunes_EntriesOlderThan90Minutes()
    {
        var h = new MeasurementHistory();
        h.Add(new Measurement(Start, -50, 60));
        h.Add(new Measurement(Start.AddMinutes(90), -50, 61));
        Assert.Equal(2, h.Snapshot().Count);
        h.Add(new Measurement(Start.AddMinutes(90).AddSeconds(1), -50, 62));
        var items = h.Snapshot();
        Assert.Equal(2, items.Count);
        Assert.Equal(61, items[0].Dba);
    }

    [Fact]
    public void KeepsOrderAndGaps()
    {
        var h = new MeasurementHistory();
        h.Add(new Measurement(Start, -50, 60));
        h.Add(new Measurement(Start.AddSeconds(0.5), -100, null));
        h.Add(new Measurement(Start.AddSeconds(1), -50, 62));
        var items = h.Snapshot();
        Assert.Null(items[1].Dba);
        Assert.Equal(new double?[] { 60, null, 62 }, items.Select(i => i.Dba).ToArray());
    }

    [Fact]
    public void Count_IsBoundedAtTwoHertz()
    {
        var h = new MeasurementHistory();
        for (int i = 0; i < 20000; i++)
        {
            h.Add(new Measurement(Start.AddSeconds(i * 0.5), -50, 60));
        }
        Assert.InRange(h.Snapshot().Count, 10800, 10801);
    }
}
