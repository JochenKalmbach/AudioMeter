namespace AudioMeter.Core.Calibration;

public enum ConfirmResult
{
    Advanced,
    Completed,
    NotDecreasing,
    Invalidated,
    InvalidInputLevel,
}

/// <summary>Interactive calibration descends from 105 to 40 dBA; the result is ordered from 40 to 105.</summary>
public sealed class CalibrationSession
{
    private readonly List<CalibrationPoint> _points = new();

    public int StepIndex => _points.Count;

    public int StepCount => Constants.CalibrationStepCount;

    /// <summary>True when the current step is the final calibration point.</summary>
    public bool IsLastStep => StepIndex == StepCount - 1;

    public int TargetDba => (int)Constants.ScaleMaxDba - StepIndex * Constants.CalibrationStepDba;

    public int? InputLevelAtFirstPoint { get; private set; }

    public bool CanAdjustInputLevel => Result is null && !IsInvalidated && StepIndex == 0;

    public bool IsInvalidated { get; private set; }

    public CalibrationTable? Result { get; private set; }

    public ConfirmResult Confirm(double levelDbfs, int? inputLevel = null)
    {
        if (Result is not null)
        {
            throw new InvalidOperationException("Calibration is already complete.");
        }
        if (IsInvalidated)
        {
            return ConfirmResult.Invalidated;
        }
        if (inputLevel is < 0 or > 100)
        {
            return ConfirmResult.InvalidInputLevel;
        }
        if (StepIndex > 0 && inputLevel != InputLevelAtFirstPoint)
        {
            IsInvalidated = true;
            return ConfirmResult.Invalidated;
        }
        if (_points.Count > 0 && !(levelDbfs < _points[^1].LevelDbfs))
        {
            return ConfirmResult.NotDecreasing;
        }
        if (StepIndex == 0)
        {
            InputLevelAtFirstPoint = inputLevel;
        }
        _points.Add(new CalibrationPoint(TargetDba, levelDbfs));
        if (_points.Count < StepCount)
        {
            return ConfirmResult.Advanced;
        }
        CalibrationTable.TryCreate(_points.AsEnumerable().Reverse(), out var table, out _);
        Result = table;
        return ConfirmResult.Completed;
    }

    public void NotifyInputLevelChanged(int inputLevel)
    {
        if (inputLevel is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(inputLevel), "Input level must be between 0 and 100.");
        }
        if (StepIndex > 0 && inputLevel != InputLevelAtFirstPoint)
        {
            IsInvalidated = true;
        }
    }
}
