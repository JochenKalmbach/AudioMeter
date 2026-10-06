using AudioMeter.Core.Calibration;
using AudioMeter.App.Audio;
using System.Runtime.InteropServices;

namespace AudioMeter.App.Dialogs;

public sealed class CalibrationDialog : Form
{
    private readonly CalibrationSession _session = new();
    private readonly Label _target = new() { AutoSize = true, Font = new Font("Segoe UI", 36f, FontStyle.Bold) };
    private readonly Label _step = new() { AutoSize = true };
    private readonly Label _level = new() { AutoSize = true, Text = "Measured input: waiting for signal…" };
    private readonly Label _warning = new() { AutoSize = true, ForeColor = Color.Firebrick };
    private readonly Button _ok = new() { Text = "OK", AutoSize = true, MinimumSize = new Size(90, 0), Enabled = false };
    private readonly TrackBar _inputLevel = new() { Minimum = 0, Maximum = 100, TickFrequency = 10, Width = 320 };
    private readonly Label _inputLevelText = new() { AutoSize = true };
    private readonly InputLevelService _inputLevelService;
    private readonly int _sessionStartInputLevel;
    private double? _lastLevel;
    private bool _updatingInputLevel;

    public CalibrationDialog(InputLevelService inputLevelService)
    {
        _inputLevelService = inputLevelService;
        _sessionStartInputLevel = inputLevelService.CurrentLevel;
        Text = "Calibrate dBA mapping";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        TopMost = true;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(16);

        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true, MinimumSize = new Size(90, 0) };
        _ok.Click += (_, _) => OnOk();
        AcceptButton = _ok;
        CancelButton = cancel;

        var instruction = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(380, 0),
            Text = "Starting at 105 dBA, play each external reference level shown below and press OK to record the measured input level.",
        };
        _inputLevel.Value = _sessionStartInputLevel;
        _inputLevel.ValueChanged += (_, _) => SetInputLevel(_inputLevel.Value);
        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_ok);

        var layout = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        layout.Controls.Add(instruction);
        layout.Controls.Add(_step);
        layout.Controls.Add(_target);
        layout.Controls.Add(_level);
        layout.Controls.Add(new Label { AutoSize = true, Text = "Input level (capture volume)" });
        layout.Controls.Add(_inputLevel);
        layout.Controls.Add(_inputLevelText);
        layout.Controls.Add(_warning);
        layout.Controls.Add(buttons);
        Controls.Add(layout);
        FormClosing += (_, _) => RestoreInputLevelIfUncommitted();
        ShowStep();
        ShowInputLevel(_sessionStartInputLevel);
    }

    /// <summary>The calibrated table once all steps are confirmed; otherwise null.</summary>
    public CalibrationTable? Result => _session.Result;

    public int? CalibrationInputLevel => _session.InputLevelAtFirstPoint;

    /// <summary>Live level in dBFS of the latest 0.5 s window.</summary>
    public void UpdateLevel(double levelDbfs)
    {
        _lastLevel = levelDbfs;
        _level.Text = $"Measured input: {levelDbfs:0.0} dBFS";
        _ok.Enabled = !_session.IsInvalidated;
    }

    /// <summary>Called when the input is lost, so a stale level cannot be recorded.</summary>
    public void ClearLevel()
    {
        _lastLevel = null;
        _level.Text = "Measured input: no signal";
        _ok.Enabled = false;
    }

    public void UpdateInputLevel(int inputLevel)
    {
        _session.NotifyInputLevelChanged(inputLevel);
        ShowInputLevel(inputLevel);
        UpdateInputLevelState();
    }

    private void ShowStep()
    {
        _step.Text = $"Step {_session.StepIndex + 1} of {_session.StepCount}";
        _target.Text = $"{_session.TargetDba} dBA";
        UpdateInputLevelState();
    }

    private void OnOk()
    {
        if (_lastLevel is not double level)
        {
            return;
        }
        int inputLevel;
        try
        {
            inputLevel = _inputLevelService.CurrentLevel;
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException or ObjectDisposedException or NotSupportedException)
        {
            _warning.Text = $"The input level is unavailable: {ex.Message}";
            _ok.Enabled = false;
            return;
        }
        switch (_session.Confirm(level, inputLevel))
        {
            case ConfirmResult.NotDecreasing:
                _warning.Text = "This measured level is not lower than the previous step. Adjust the sound and try again.";
                break;
            case ConfirmResult.Invalidated:
                InvalidateSession();
                break;
            case ConfirmResult.InvalidInputLevel:
                _warning.Text = "The input level must be between 0 and 100.";
                break;
            case ConfirmResult.Advanced:
                _warning.Text = string.Empty;
                ShowStep();
                UpdateInputLevelState();
                break;
            case ConfirmResult.Completed:
                DialogResult = DialogResult.OK;
                break;
        }
    }

    private void SetInputLevel(int value)
    {
        if (_updatingInputLevel || !_session.CanAdjustInputLevel)
        {
            return;
        }
        try
        {
            int actualLevel = _inputLevelService.SetLevel(value);
            UpdateInputLevel(actualLevel);
            _warning.Text = string.Empty;
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException or ObjectDisposedException or NotSupportedException)
        {
            _warning.Text = $"The input level could not be changed: {ex.Message}";
            try
            {
                ShowInputLevel(_inputLevelService.CurrentLevel);
            }
            catch (Exception readException) when (readException is COMException or InvalidOperationException or ObjectDisposedException or NotSupportedException)
            {
                _inputLevelText.Text = "Input level: unavailable";
                _inputLevel.Enabled = false;
            }
        }
    }

    private void ShowInputLevel(int inputLevel)
    {
        _updatingInputLevel = true;
        _inputLevel.Value = Math.Clamp(inputLevel, _inputLevel.Minimum, _inputLevel.Maximum);
        _inputLevelText.Text = $"Input level: {inputLevel}";
        _updatingInputLevel = false;
    }

    private void UpdateInputLevelState()
    {
        _inputLevel.Enabled = _session.CanAdjustInputLevel;
        if (_session.IsInvalidated)
        {
            InvalidateSession();
        }
    }

    private void InvalidateSession()
    {
        _warning.Text = "The input level changed after 105 dBA was recorded. Cancel and restart calibration at 105 dBA.";
        _ok.Enabled = false;
        _inputLevel.Enabled = false;
    }

    private void RestoreInputLevelIfUncommitted()
    {
        if (_session.StepIndex != 0)
        {
            return;
        }
        try
        {
            _inputLevelService.SetLevel(_sessionStartInputLevel);
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException or ObjectDisposedException or NotSupportedException)
        {
            MessageBox.Show(this, $"The original input level could not be restored: {ex.Message}",
                "AudioMeter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
