using AudioMeter.Core.Levels;

namespace AudioMeter.Core.History;

/// <summary>Measurements of the last 90 minutes, oldest first. Null Dba entries are gaps.</summary>
public sealed class MeasurementHistory
{
    private readonly object _lock = new();
    private readonly List<Measurement> _items = new();

    public void Add(Measurement measurement)
    {
        lock (_lock)
        {
            _items.Add(measurement);
            var cutoff = measurement.Timestamp - Constants.HistoryDuration;
            int remove = 0;
            while (remove < _items.Count && _items[remove].Timestamp < cutoff)
            {
                remove++;
            }
            if (remove > 0)
            {
                _items.RemoveRange(0, remove);
            }
        }
    }

    public IReadOnlyList<Measurement> Snapshot()
    {
        lock (_lock)
        {
            return _items.ToArray();
        }
    }
}
