using AudioMeter.Core.Calibration;

namespace AudioMeter.App.Dialogs;

public sealed class CalibrationDialog : Form
{
    private readonly CalibrationSession _session = new();
    private readonly Label _target = new() { AutoSize = true, Font = new Font("Segoe UI", 36f, FontStyle.Bold) };
    private readonly Label _step = new() { AutoSize = true };
    private readonly Label _level = new() { AutoSize = true, Text = "Measured input: waiting for signal…" };
    private readonly Label _warning = new() { AutoSize = true, ForeColor = Color.Firebrick };
    private readonly Button _ok = new() { Text = "OK", AutoSize = true, MinimumSize = new Size(90, 0), Enabled = false };
    private double? _lastLevel;

    public CalibrationDialog()
    {
        Text = "Calibrate dBA mapping";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
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
            Text = "Play an external sound at the level shown below, then press OK to record the measured input level.",
        };
        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_ok);

        var layout = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        layout.Controls.Add(instruction);
        layout.Controls.Add(_step);
        layout.Controls.Add(_target);
        layout.Controls.Add(_level);
        layout.Controls.Add(_warning);
        layout.Controls.Add(buttons);
        Controls.Add(layout);
        ShowStep();
    }

    /// <summary>The calibrated table once all steps are confirmed; otherwise null.</summary>
    public CalibrationTable? Result => _session.Result;

    /// <summary>Live level in dBFS of the latest 0.5 s window.</summary>
    public void UpdateLevel(double levelDbfs)
    {
        _lastLevel = levelDbfs;
        _level.Text = $"Measured input: {levelDbfs:0.0} dBFS";
        _ok.Enabled = true;
    }

    /// <summary>Called when the input is lost, so a stale level cannot be recorded.</summary>
    public void ClearLevel()
    {
        _lastLevel = null;
        _level.Text = "Measured input: no signal";
        _ok.Enabled = false;
    }

    private void ShowStep()
    {
        _step.Text = $"Step {_session.StepIndex + 1} of {_session.StepCount}";
        _target.Text = $"{_session.TargetDba} dBA";
    }

    private void OnOk()
    {
        if (_lastLevel is not double level)
        {
            return;
        }
        switch (_session.Confirm(level))
        {
            case ConfirmResult.NotIncreasing:
                _warning.Text = "This level is not higher than the previous step. Adjust the sound and try again.";
                break;
            case ConfirmResult.Advanced:
                _warning.Text = string.Empty;
                ShowStep();
                break;
            case ConfirmResult.Completed:
                DialogResult = DialogResult.OK;
                break;
        }
    }
}
