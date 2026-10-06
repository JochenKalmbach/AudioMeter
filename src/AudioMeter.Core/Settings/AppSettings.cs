using AudioMeter.Core.Calibration;

namespace AudioMeter.Core.Settings;

public sealed class WindowSettings
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public sealed class AppSettings
{
    public string? InputDeviceId { get; set; }
    public int? InputLevel { get; set; }
    public string? CalibrationInputDeviceId { get; set; }
    public int? CalibrationInputLevel { get; set; }
    public double GreenYellowLimit { get; set; } = Constants.DefaultGreenYellowLimit;
    public double YellowRedLimit { get; set; } = Constants.DefaultYellowRedLimit;
    public List<CalibrationPoint> Calibration { get; set; } = new();
    public WindowSettings? Window { get; set; }
}
