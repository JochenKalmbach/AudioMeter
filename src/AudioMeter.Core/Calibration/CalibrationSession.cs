namespace AudioMeter.Core.Calibration;

public enum ConfirmResult
{
    Advanced,
    Completed,
    NotIncreasing,
}

/// <summary>Interactive calibration: one confirmed level per target dBA, 40…105 in 5 dBA steps.</summary>
public sealed class CalibrationSession
{
    private readonly List<CalibrationPoint> _points = new();

    public int StepIndex => _points.Count;

    public int StepCount => Constants.CalibrationStepCount;

    public int TargetDba => (int)Constants.ScaleMinDba + StepIndex * Constants.CalibrationStepDba;

    public CalibrationTable? Result { get; private set; }

    public ConfirmResult Confirm(double levelDbfs)
    {
        if (Result is not null)
        {
            throw new InvalidOperationException("Calibration is already complete.");
        }
        if (_points.Count > 0 && !(levelDbfs > _points[^1].LevelDbfs))
        {
            return ConfirmResult.NotIncreasing;
        }
        _points.Add(new CalibrationPoint(TargetDba, levelDbfs));
        if (_points.Count < StepCount)
        {
            return ConfirmResult.Advanced;
        }
        CalibrationTable.TryCreate(_points, out var table, out _);
        Result = table;
        return ConfirmResult.Completed;
    }
}
