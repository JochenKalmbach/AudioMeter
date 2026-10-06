using AudioMeter.Core.Calibration;
using AudioMeter.Core.Settings;

namespace AudioMeter.Core.Tests;

public sealed class SettingsStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "AudioMeterTests-" + Guid.NewGuid());

    private SettingsStore Store() => new(Path.Combine(_dir, "settings.json"));

    public void Dispose()
    {
        if (Directory.Exists(_dir)) { Directory.Delete(_dir, true); }
    }

    private static List<CalibrationPoint> Points() =>
        Enumerable.Range(0, 14).Select(i => new CalibrationPoint(40 + i * 5, -80 + i * 6.0)).ToList();

    [Fact]
    public void MissingFile_GivesDefaults()
    {
        var s = Store().Load();
        Assert.Equal(70, s.GreenYellowLimit);
        Assert.Equal(85, s.YellowRedLimit);
        Assert.Empty(s.Calibration);
    }

    [Fact]
    public void RoundTrip_PreservesValues()
    {
        var store = Store();
        store.Save(new AppSettings
        {
            InputDeviceId = "dev1",
            InputLevel = 72,
            CalibrationInputDeviceId = "dev1",
            CalibrationInputLevel = 72,
            GreenYellowLimit = 60,
            YellowRedLimit = 90,
            Calibration = Points(),
            Window = new WindowSettings { X = 1, Y = 2, Width = 300, Height = 400 },
        });
        var s = store.Load();
        Assert.Equal("dev1", s.InputDeviceId);
        Assert.Equal(72, s.InputLevel);
        Assert.Equal("dev1", s.CalibrationInputDeviceId);
        Assert.Equal(72, s.CalibrationInputLevel);
        Assert.Equal(60, s.GreenYellowLimit);
        Assert.Equal(90, s.YellowRedLimit);
        Assert.Equal(14, s.Calibration.Count);
        Assert.Equal(400, s.Window!.Height);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void InputLevelBounds_AreAccepted(int value)
    {
        var store = Store();
        store.Save(new AppSettings { InputLevel = value, CalibrationInputLevel = value });

        var settings = store.Load();

        Assert.Equal(value, settings.InputLevel);
        Assert.Equal(value, settings.CalibrationInputLevel);
        Assert.Null(store.LoadWarning);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void OutOfRangeInputLevels_AreIgnoredWithWarning(int value)
    {
        var store = Store();
        store.Save(new AppSettings { InputLevel = value, CalibrationInputLevel = value });

        var settings = store.Load();

        Assert.Null(settings.InputLevel);
        Assert.Null(settings.CalibrationInputLevel);
        Assert.Contains("input level", store.LoadWarning, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LegacySettings_PreserveCalibrationWithoutInventingMetadata()
    {
        var store = Store();
        Directory.CreateDirectory(_dir);
        File.WriteAllText(store.Path, $$"""
            {
              "inputDeviceId": "legacy-device",
              "calibration": {{System.Text.Json.JsonSerializer.Serialize(Points())}}
            }
            """);

        var settings = store.Load();

        Assert.Equal("legacy-device", settings.InputDeviceId);
        Assert.Equal(14, settings.Calibration.Count);
        Assert.Null(settings.InputLevel);
        Assert.Null(settings.CalibrationInputDeviceId);
        Assert.Null(settings.CalibrationInputLevel);
    }

    [Fact]
    public void CorruptJson_GivesDefaultsAndWarning()
    {
        var store = Store();
        Directory.CreateDirectory(_dir);
        File.WriteAllText(store.Path, "{ not json");
        var s = store.Load();
        Assert.Equal(70, s.GreenYellowLimit);
        Assert.NotNull(store.LoadWarning);
    }

    [Fact]
    public void InvalidCalibration_IsIgnored()
    {
        var store = Store();
        var pts = Points();
        pts[3] = pts[3] with { LevelDbfs = -200 };
        store.Save(new AppSettings { Calibration = pts });
        Assert.Empty(store.Load().Calibration);
    }

    [Fact]
    public void InvalidLimits_FallBackToDefaults()
    {
        var store = Store();
        store.Save(new AppSettings { GreenYellowLimit = 90, YellowRedLimit = 50 });
        var s = store.Load();
        Assert.Equal(70, s.GreenYellowLimit);
        Assert.Equal(85, s.YellowRedLimit);
    }
}
