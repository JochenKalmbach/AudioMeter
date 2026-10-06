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
            GreenYellowLimit = 60,
            YellowRedLimit = 90,
            Calibration = Points(),
            Window = new WindowSettings { X = 1, Y = 2, Width = 300, Height = 400 },
        });
        var s = store.Load();
        Assert.Equal("dev1", s.InputDeviceId);
        Assert.Equal(60, s.GreenYellowLimit);
        Assert.Equal(90, s.YellowRedLimit);
        Assert.Equal(14, s.Calibration.Count);
        Assert.Equal(400, s.Window!.Height);
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
