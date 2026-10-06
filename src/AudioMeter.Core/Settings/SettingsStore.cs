using System.Text.Json;
using AudioMeter.Core.Calibration;
using AudioMeter.Core.Levels;

namespace AudioMeter.Core.Settings;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public SettingsStore(string? path = null)
    {
        Path = path ?? System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AudioMeter", "settings.json");
    }

    public string Path { get; }

    /// <summary>Set when the file existed but could not be used; null otherwise.</summary>
    public string? LoadWarning { get; private set; }

    public AppSettings Load()
    {
        LoadWarning = null;
        if (!File.Exists(Path))
        {
            return new AppSettings();
        }
        AppSettings? settings;
        try
        {
            settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(Path), Options);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            LoadWarning = $"Settings could not be read ({ex.Message}); defaults are used.";
            return new AppSettings();
        }
        settings ??= new AppSettings();

        bool invalidInputLevel = IsOutOfRange(settings.InputLevel) || IsOutOfRange(settings.CalibrationInputLevel);
        if (IsOutOfRange(settings.InputLevel))
        {
            settings.InputLevel = null;
        }
        if (IsOutOfRange(settings.CalibrationInputLevel))
        {
            settings.CalibrationInputLevel = null;
        }
        if (invalidInputLevel)
        {
            LoadWarning = "An input level outside 0–100 was ignored.";
        }

        if (!LevelZones.TryCreate(settings.GreenYellowLimit, settings.YellowRedLimit, out _, out _))
        {
            settings.GreenYellowLimit = Constants.DefaultGreenYellowLimit;
            settings.YellowRedLimit = Constants.DefaultYellowRedLimit;
        }
        settings.Calibration ??= new List<CalibrationPoint>();
        if (!CalibrationTable.TryCreate(settings.Calibration, out _, out _))
        {
            settings.Calibration = new List<CalibrationPoint>();
        }
        return settings;
    }

    private static bool IsOutOfRange(int? value) => value is < 0 or > 100;

    public void Save(AppSettings settings)
    {
        var dir = System.IO.Path.GetDirectoryName(Path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        var temp = Path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(settings, Options));
        if (File.Exists(Path))
        {
            File.Replace(temp, Path, null);
        }
        else
        {
            File.Move(temp, Path);
        }
    }
}
