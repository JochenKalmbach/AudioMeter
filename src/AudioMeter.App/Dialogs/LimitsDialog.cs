using AudioMeter.Core;
using AudioMeter.Core.Levels;

namespace AudioMeter.App.Dialogs;

public sealed class LimitsDialog : Form
{
    private readonly NumericUpDown _greenYellow = Create();
    private readonly NumericUpDown _yellowRed = Create();

    public LimitsDialog(LevelZones current)
    {
        Text = "Color limits";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        _greenYellow.Value = (decimal)current.GreenYellowLimit;
        _yellowRed.Value = (decimal)current.YellowRedLimit;

        var ok = new Button { Text = "OK", AutoSize = true, MinimumSize = new Size(80, 0) };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true, MinimumSize = new Size(80, 0) };
        ok.Click += (_, _) => OnOk();
        AcceptButton = ok;
        CancelButton = cancel;

        var layout = new TableLayoutPanel { AutoSize = true, ColumnCount = 3, RowCount = 3 };
        layout.Controls.Add(new Label { Text = "Green / yellow limit:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        layout.Controls.Add(_greenYellow, 1, 0);
        layout.Controls.Add(new Label { Text = "dBA", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 0);
        layout.Controls.Add(new Label { Text = "Yellow / red limit:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        layout.Controls.Add(_yellowRed, 1, 1);
        layout.Controls.Add(new Label { Text = "dBA", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 1);
        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(ok);
        layout.Controls.Add(buttons, 0, 2);
        layout.SetColumnSpan(buttons, 3);
        Controls.Add(layout);
    }

    public LevelZones? Result { get; private set; }

    private static NumericUpDown Create() => new()
    {
        Minimum = (decimal)Constants.ScaleMinDba,
        Maximum = (decimal)Constants.ScaleMaxDba,
        DecimalPlaces = 1,
        Increment = 1,
        Width = 80,
    };

    private void OnOk()
    {
        if (!LevelZones.TryCreate((double)_greenYellow.Value, (double)_yellowRed.Value, out var zones, out var error))
        {
            MessageBox.Show(this, error, "Invalid limits", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Result = zones;
        DialogResult = DialogResult.OK;
    }
}
