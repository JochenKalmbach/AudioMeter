using AudioMeter.App.Audio;
using AudioMeter.App.Controls;
using AudioMeter.App.Dialogs;
using AudioMeter.Core;
using AudioMeter.Core.Calibration;
using AudioMeter.Core.History;
using AudioMeter.Core.Levels;
using AudioMeter.Core.Settings;

namespace AudioMeter.App;

public sealed class MainForm : Form
{
    private static readonly TimeSpan StaleAfter = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan RetryEvery = TimeSpan.FromSeconds(5);

    private readonly SettingsStore _store = new();
    private readonly AppSettings _settings;
    private readonly AudioCaptureService _capture = new();
    private readonly MeasurementHistory _history = new();
    private readonly MeterControl _meter = new() { Dock = DockStyle.Fill };
    private readonly HistoryChartControl _chart = new() { Dock = DockStyle.Fill };
    private readonly System.Windows.Forms.Timer _watchdog = new() { Interval = 500 };
    private readonly ToolStripMenuItem _inputMenu = new("&Audio input");
    private CalibrationTable _table;
    private LevelZones _zones;
    private CalibrationDialog? _calibrationDialog;
    private DateTime _lastLevelAt = DateTime.MinValue;
    private DateTime _lastStartAttempt = DateTime.MinValue;
    private bool _noSignal = true;

    public MainForm()
    {
        _settings = _store.Load();
        _table = CalibrationTable.TryCreate(_settings.Calibration, out var table, out _) ? table! : CalibrationTable.Default;
        _zones = LevelZones.TryCreate(_settings.GreenYellowLimit, _settings.YellowRedLimit, out var zones, out _)
            ? zones! : LevelZones.Default;

        Text = "AudioMeter";
        TopMost = true;
        MinimumSize = new Size(320, 240);
        Size = new Size(480, 360);
        StartPosition = FormStartPosition.WindowsDefaultLocation;
        RestoreWindow();

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        layout.Controls.Add(_meter, 0, 0);
        layout.Controls.Add(_chart, 0, 1);

        var menu = new MenuStrip();
        var settingsMenu = new ToolStripMenuItem("&Settings");
        var limits = new ToolStripMenuItem("&Color limits…");
        var calibrate = new ToolStripMenuItem("C&alibrate…");
        _inputMenu.DropDownOpening += (_, _) => PopulateInputMenu();
        _inputMenu.DropDownItems.Add(new ToolStripMenuItem("(loading)") { Enabled = false });
        limits.Click += (_, _) => EditLimits();
        calibrate.Click += (_, _) => RunCalibration();
        settingsMenu.DropDownItems.AddRange(new ToolStripItem[] { _inputMenu, limits, calibrate });
        menu.Items.Add(settingsMenu);
        MainMenuStrip = menu;

        Controls.Add(layout);
        Controls.Add(menu);

        ApplyZonesAndCalibration();
        _meter.Dba = null;
        _capture.LevelMeasured += level => BeginInvokeSafe(() => OnLevel(level));
        _capture.Failed += message => BeginInvokeSafe(() => OnFailed(message));
        _watchdog.Tick += (_, _) => OnWatchdog();

        Shown += (_, _) =>
        {
            if (_store.LoadWarning is { } warning)
            {
                _meter.StatusText = warning;
            }
            StartCapture();
            _watchdog.Start();
        };
        FormClosing += (_, _) => SaveWindow();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _watchdog.Dispose();
            _capture.Dispose();
        }
        base.Dispose(disposing);
    }

    private void BeginInvokeSafe(Action action)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }
        try
        {
            BeginInvoke(action);
        }
        catch (InvalidOperationException)
        {
            // Form closed while a capture callback was in flight.
        }
    }

    private void ApplyZonesAndCalibration()
    {
        _meter.Zones = _zones;
        _chart.Zones = _zones;
        _meter.IsCalibrated = _table.IsCalibrated;
    }

    private void StartCapture()
    {
        _lastStartAttempt = DateTime.UtcNow;
        try
        {
            string? notice = _capture.Start(_settings.InputDeviceId);
            _lastLevelAt = DateTime.UtcNow;
            _meter.StatusText = notice;
        }
        catch (Exception ex)
        {
            SetNoSignal($"Cannot open audio input: {ex.Message}");
        }
    }

    private void OnLevel(double levelDbfs)
    {
        _lastLevelAt = DateTime.UtcNow;
        _noSignal = false;
        double dba = _table.ToDba(levelDbfs);
        _history.Add(new Measurement(DateTimeOffset.Now, levelDbfs, dba));
        _meter.Dba = dba;
        if (_meter.StatusText is not null && _meter.StatusText.StartsWith("Cannot", StringComparison.Ordinal))
        {
            _meter.StatusText = null;
        }
        _chart.SetHistory(_history.Snapshot());
        _calibrationDialog?.UpdateLevel(levelDbfs);
    }

    private void OnFailed(string message)
    {
        _capture.Stop();
        SetNoSignal(message);
    }

    private void SetNoSignal(string message)
    {
        _noSignal = true;
        _meter.Dba = null;
        _meter.StatusText = message;
        _calibrationDialog?.ClearLevel();
    }

    private void OnWatchdog()
    {
        var now = DateTime.UtcNow;
        if (_capture.IsRunning && !_noSignal && now - _lastLevelAt > StaleAfter)
        {
            _capture.Stop();
            SetNoSignal("No audio data received from the input.");
        }
        if (_noSignal)
        {
            _history.Add(new Measurement(DateTimeOffset.Now, Constants.SilenceFloorDbfs, null));
            _chart.SetHistory(_history.Snapshot());
            if (!_capture.IsRunning && now - _lastStartAttempt > RetryEvery)
            {
                StartCapture();
            }
        }
    }

    private void PopulateInputMenu()
    {
        _inputMenu.DropDownItems.Clear();
        IReadOnlyList<InputDevice> devices;
        try
        {
            devices = InputDeviceService.List();
        }
        catch (Exception ex)
        {
            _inputMenu.DropDownItems.Add(new ToolStripMenuItem($"Cannot list inputs: {ex.Message}") { Enabled = false });
            return;
        }
        if (devices.Count == 0)
        {
            _inputMenu.DropDownItems.Add(new ToolStripMenuItem("No audio inputs found") { Enabled = false });
            return;
        }
        bool hasSelection = devices.Any(d => d.Id == _settings.InputDeviceId);
        foreach (var device in devices)
        {
            var item = new ToolStripMenuItem(device.IsDefault ? $"{device.Name} (default)" : device.Name)
            {
                Checked = hasSelection ? device.Id == _settings.InputDeviceId : device.IsDefault,
                Tag = device.Id,
            };
            item.Click += (_, _) => SelectInput(device.Id);
            _inputMenu.DropDownItems.Add(item);
        }
    }

    private void SelectInput(string deviceId)
    {
        _settings.InputDeviceId = deviceId;
        SaveSettings();
        _capture.Stop();
        _noSignal = true;
        StartCapture();
    }

    private void EditLimits()
    {
        using var dialog = new LimitsDialog(_zones);
        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.Result is { } zones)
        {
            _zones = zones;
            _settings.GreenYellowLimit = zones.GreenYellowLimit;
            _settings.YellowRedLimit = zones.YellowRedLimit;
            SaveSettings();
            ApplyZonesAndCalibration();
        }
    }

    private void RunCalibration()
    {
        using var dialog = new CalibrationDialog();
        _calibrationDialog = dialog;
        try
        {
            if (dialog.ShowDialog(this) == DialogResult.OK && dialog.Result is { } table)
            {
                _table = table;
                _settings.Calibration = table.Points.ToList();
                SaveSettings();
                ApplyZonesAndCalibration();
            }
        }
        finally
        {
            _calibrationDialog = null;
        }
    }

    private void SaveSettings()
    {
        try
        {
            _store.Save(_settings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"Settings could not be saved: {ex.Message}", "AudioMeter",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RestoreWindow()
    {
        if (_settings.Window is not { Width: > 0, Height: > 0 } w)
        {
            return;
        }
        var bounds = new Rectangle(w.X, w.Y, Math.Max(w.Width, MinimumSize.Width), Math.Max(w.Height, MinimumSize.Height));
        if (Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(bounds)))
        {
            StartPosition = FormStartPosition.Manual;
            Bounds = bounds;
        }
    }

    private void SaveWindow()
    {
        var b = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        _settings.Window = new WindowSettings { X = b.X, Y = b.Y, Width = b.Width, Height = b.Height };
        try
        {
            _store.Save(_settings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Window bounds are not worth interrupting shutdown for.
        }
    }
}
