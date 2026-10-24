using System.Globalization;

namespace AudioMeter.Core.Calibration;

/// <summary>
/// Editable, unsaved state of a calibration table: one text cell of dBFS per calibration point.
/// Cells must be finite numbers in [MinDbfs, MaxDbfs] and strictly increasing.
/// </summary>
public sealed class CalibrationTableDraft
{
    public const double MinDbfs = -120;
    public const double MaxDbfs = 0;

    private readonly string[] _texts = new string[Constants.CalibrationStepCount];
    private readonly double?[] _values = new double?[Constants.CalibrationStepCount];
    private readonly string?[] _errors = new string?[Constants.CalibrationStepCount];

    private CalibrationTableDraft()
    {
    }

    public static int RowCount => Constants.CalibrationStepCount;

    public bool IsValid => _errors.All(e => e is null);

    public static int DbaAt(int index) => (int)Constants.ScaleMinDba + index * Constants.CalibrationStepDba;

    public static CalibrationTableDraft FromTable(CalibrationTable table)
    {
        var draft = new CalibrationTableDraft();
        for (int i = 0; i < RowCount; i++)
        {
            draft._texts[i] = table.Points[i].LevelDbfs.ToString("0.0#", CultureInfo.CurrentCulture);
        }
        draft.Revalidate();
        return draft;
    }

    public string GetCell(int index) => _texts[index];

    public void SetCell(int index, string? text)
    {
        _texts[index] = text ?? string.Empty;
        Revalidate();
    }

    public string? RowError(int index) => _errors[index];

    public bool TryBuild(out CalibrationTable? table)
    {
        table = null;
        if (!IsValid)
        {
            return false;
        }
        var points = Enumerable.Range(0, RowCount).Select(i => new CalibrationPoint(DbaAt(i), _values[i]!.Value));
        return CalibrationTable.TryCreate(points, out table, out _);
    }

    public double? ToDba(double levelDbfs) => TryBuild(out var table) ? table!.ToDba(levelDbfs) : null;

    private void Revalidate()
    {
        double? previous = null;
        for (int i = 0; i < RowCount; i++)
        {
            _values[i] = null;
            _errors[i] = null;
            if (!TryParse(_texts[i], out double value))
            {
                _errors[i] = "Enter a number.";
            }
            else if (value < MinDbfs || value > MaxDbfs)
            {
                _errors[i] = $"Value must be between {MinDbfs:0} and {MaxDbfs:0} dBFS.";
            }
            else if (previous is { } prev && value <= prev)
            {
                _errors[i] = "Value must be greater than the row above.";
            }
            else
            {
                _values[i] = value;
                previous = value;
            }
        }
    }

    private static bool TryParse(string text, out double value)
    {
        string t = text.Trim();
        return (double.TryParse(t, NumberStyles.Float, CultureInfo.CurrentCulture, out value) ||
                double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) &&
            double.IsFinite(value);
    }
}
