using AudioMeter.Core.Calibration;

namespace AudioMeter.App.Dialogs;

public sealed class CalibrationTableDialog : Form
{
    private readonly CalibrationTableDraft _draft;
    private readonly DataGridView _grid = new();
    private readonly Label _level = new() { AutoSize = true, Text = "Measured input: no signal" };
    private readonly Button _save = new() { Text = "Save", AutoSize = true, MinimumSize = new Size(90, 0) };
    private double? _lastLevel;

    public CalibrationTableDialog(CalibrationTable current)
    {
        _draft = CalibrationTableDraft.FromTable(current);

        Text = "Calibration table";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        TopMost = true;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;

        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible = false;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "dBA", ReadOnly = true, Width = 90, SortMode = DataGridViewColumnSortMode.NotSortable });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "dBFS", Width = 140, SortMode = DataGridViewColumnSortMode.NotSortable });
        for (int i = 0; i < CalibrationTableDraft.RowCount; i++)
        {
            _grid.Rows.Add(CalibrationTableDraft.DbaAt(i).ToString(), _draft.GetCell(i));
        }
        _grid.ClientSize = new Size(260, _grid.ColumnHeadersHeight + CalibrationTableDraft.RowCount * _grid.RowTemplate.Height + 2);
        _grid.CellEndEdit += (_, e) => OnCellEdited(e.RowIndex, e.ColumnIndex);

        _save.Click += (_, _) =>
        {
            _grid.EndEdit();
            if (_draft.TryBuild(out var table))
            {
                Result = table;
                DialogResult = DialogResult.OK;
            }
        };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true, MinimumSize = new Size(90, 0) };
        AcceptButton = _save;
        CancelButton = cancel;

        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_save);

        var layout = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Padding = new Padding(12) };
        layout.Controls.Add(_level);
        layout.Controls.Add(_grid);
        layout.Controls.Add(buttons);
        Controls.Add(layout);
        RefreshState();
    }

    public CalibrationTable? Result { get; private set; }

    /// <summary>Raised with the built table after each committed edit that leaves the draft valid.</summary>
    public event Action<CalibrationTable>? TableChanged;

    public void UpdateLevel(double levelDbfs)
    {
        _lastLevel = levelDbfs;
        RefreshLevelText();
    }

    public void ClearLevel()
    {
        _lastLevel = null;
        RefreshLevelText();
    }

    private void OnCellEdited(int row, int column)
    {
        if (column != 1 || row < 0)
        {
            return;
        }
        _draft.SetCell(row, _grid.Rows[row].Cells[1].Value as string);
        RefreshState();
        if (_draft.TryBuild(out var table))
        {
            TableChanged?.Invoke(table!);
        }
    }

    private void RefreshState()
    {
        for (int i = 0; i < CalibrationTableDraft.RowCount; i++)
        {
            _grid.Rows[i].Cells[1].ErrorText = _draft.RowError(i) ?? string.Empty;
        }
        _save.Enabled = _draft.IsValid;
        RefreshLevelText();
    }

    private void RefreshLevelText()
    {
        if (_lastLevel is not { } level)
        {
            _level.Text = "Measured input: no signal";
        }
        else if (_draft.ToDba(level) is { } dba)
        {
            _level.Text = $"Measured input: {level:0.0} dBFS  →  {dba:0.0} dBA";
        }
        else
        {
            _level.Text = $"Measured input: {level:0.0} dBFS  →  n/a (table invalid)";
        }
    }
}
