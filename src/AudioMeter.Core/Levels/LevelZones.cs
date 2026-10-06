namespace AudioMeter.Core.Levels;

public enum Zone
{
    Green,
    Yellow,
    Red,
}

public sealed record LevelZones(double GreenYellowLimit, double YellowRedLimit)
{
    public static LevelZones Default { get; } =
        new(Constants.DefaultGreenYellowLimit, Constants.DefaultYellowRedLimit);

    /// <summary>Requires 40 ≤ GreenYellow &lt; YellowRed ≤ 105.</summary>
    public static bool TryCreate(double greenYellow, double yellowRed, out LevelZones? zones, out string? error)
    {
        zones = null;
        error = null;
        if (double.IsNaN(greenYellow) || double.IsNaN(yellowRed)
            || greenYellow < Constants.ScaleMinDba || greenYellow > Constants.ScaleMaxDba
            || yellowRed < Constants.ScaleMinDba || yellowRed > Constants.ScaleMaxDba)
        {
            error = $"Limits must be within {Constants.ScaleMinDba}–{Constants.ScaleMaxDba} dBA.";
            return false;
        }
        if (yellowRed <= greenYellow)
        {
            error = "The yellow/red limit must be higher than the green/yellow limit.";
            return false;
        }
        zones = new LevelZones(greenYellow, yellowRed);
        return true;
    }

    public Zone ZoneOf(double dba) =>
        dba <= GreenYellowLimit ? Zone.Green : dba <= YellowRedLimit ? Zone.Yellow : Zone.Red;
}
