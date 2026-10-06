namespace AudioMeter.Core;

public static class Constants
{
    public const double ScaleMinDba = 40;
    public const double ScaleMaxDba = 105;
    public const int CalibrationStepDba = 5;
    public const int CalibrationStepCount = 14;
    public const double MeasurementIntervalSeconds = 0.5;
    public const double SilenceFloorDbfs = -100;
    public const double DefaultGreenYellowLimit = 70;
    public const double DefaultYellowRedLimit = 85;
    public static readonly TimeSpan HistoryDuration = TimeSpan.FromMinutes(90);
}
