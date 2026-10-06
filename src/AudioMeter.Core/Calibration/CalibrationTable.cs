namespace AudioMeter.Core.Calibration;

public sealed record CalibrationPoint(int Dba, double LevelDbfs);

/// <summary>
/// Maps dBFS to dBA by piecewise-linear interpolation over 14 points (40…105 dBA in 5 dBA steps).
/// LevelDbfs must be strictly increasing with Dba.
/// </summary>
public sealed class CalibrationTable
{
    private CalibrationTable(IReadOnlyList<CalibrationPoint> points, bool isCalibrated)
    {
        Points = points;
        IsCalibrated = isCalibrated;
    }

    public IReadOnlyList<CalibrationPoint> Points { get; }

    public bool IsCalibrated { get; }

    /// <summary>Uncalibrated mapping: -90 dBFS → 40 dBA, 0 dBFS → 105 dBA, linear.</summary>
    public static CalibrationTable Default { get; } = BuildDefault();

    public static bool TryCreate(IEnumerable<CalibrationPoint> points, out CalibrationTable? table, out string? error)
    {
        table = null;
        var list = points.ToList();
        if (list.Count != Constants.CalibrationStepCount)
        {
            error = $"Exactly {Constants.CalibrationStepCount} calibration points are required.";
            return false;
        }
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Dba != Constants.ScaleMinDba + i * Constants.CalibrationStepDba)
            {
                error = "Calibration points must be 40…105 dBA in 5 dBA steps.";
                return false;
            }
            if (double.IsNaN(list[i].LevelDbfs) || (i > 0 && list[i].LevelDbfs <= list[i - 1].LevelDbfs))
            {
                error = "Calibration levels must be strictly increasing.";
                return false;
            }
        }
        error = null;
        table = new CalibrationTable(list, true);
        return true;
    }

    public double ToDba(double levelDbfs)
    {
        var p = Points;
        if (levelDbfs <= p[0].LevelDbfs)
        {
            return p[0].Dba;
        }
        if (levelDbfs >= p[^1].LevelDbfs)
        {
            return p[^1].Dba;
        }
        for (int i = 1; i < p.Count; i++)
        {
            if (levelDbfs <= p[i].LevelDbfs)
            {
                double t = (levelDbfs - p[i - 1].LevelDbfs) / (p[i].LevelDbfs - p[i - 1].LevelDbfs);
                return p[i - 1].Dba + t * (p[i].Dba - p[i - 1].Dba);
            }
        }
        return p[^1].Dba;
    }

    private static CalibrationTable BuildDefault()
    {
        double span = Constants.ScaleMaxDba - Constants.ScaleMinDba;
        var points = Enumerable.Range(0, Constants.CalibrationStepCount)
            .Select(i =>
            {
                int dba = Constants.CalibrationStepDba * i + (int)Constants.ScaleMinDba;
                return new CalibrationPoint(dba, -90 + (dba - Constants.ScaleMinDba) / span * 90);
            })
            .ToList();
        return new CalibrationTable(points, false);
    }
}
