using AudioMeter.Core;
using AudioMeter.Core.Levels;

namespace AudioMeter.App.Controls;

public sealed class HistoryChartControl : Control
{
    private IReadOnlyList<Measurement> _items = Array.Empty<Measurement>();
    private LevelZones _zones = LevelZones.Default;

    public HistoryChartControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        BackColor = Color.FromArgb(24, 24, 24);
        ForeColor = Color.White;
    }

    public LevelZones Zones
    {
        get => _zones;
        set { _zones = value; Invalidate(); }
    }

    public void SetHistory(IReadOnlyList<Measurement> items)
    {
        _items = items;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        int left = TextRenderer.MeasureText("105", Font).Width + 8;
        int bottom = Font.Height + 6;
        var plot = new RectangleF(left, 6, Width - left - 8, Height - bottom - 6);
        if (plot.Width < 20 || plot.Height < 20)
        {
            return;
        }

        float YOf(double dba) =>
            plot.Bottom - (float)((Math.Clamp(dba, Constants.ScaleMinDba, Constants.ScaleMaxDba) - Constants.ScaleMinDba)
                / (Constants.ScaleMaxDba - Constants.ScaleMinDba)) * plot.Height;

        using var border = new Pen(Color.FromArgb(70, 70, 70));
        g.DrawRectangle(border, plot.X, plot.Y, plot.Width, plot.Height);

        using var textBrush = new SolidBrush(Color.FromArgb(190, 190, 190));
        var right = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
        foreach (var (dba, color) in new[]
        {
            ((double)Constants.ScaleMinDba, Color.Gray),
            (_zones.GreenYellowLimit, Color.FromArgb(240, 192, 32)),
            (_zones.YellowRedLimit, Color.FromArgb(224, 52, 52)),
            ((double)Constants.ScaleMaxDba, Color.Gray),
        })
        {
            float y = YOf(dba);
            using var pen = new Pen(Color.FromArgb(110, color)) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            g.DrawLine(pen, plot.Left, y, plot.Right, y);
            g.DrawString(dba.ToString("0"), Font, textBrush, new RectangleF(0, y - Font.Height / 2f, left - 4, Font.Height), right);
        }

        var center = new StringFormat { Alignment = StringAlignment.Center };
        for (int minutes = 90; minutes >= 0; minutes -= 30)
        {
            float x = plot.Right - plot.Width * (minutes / 90f);
            string label = minutes == 0 ? "now" : $"-{minutes} min";
            var format = minutes == 90 ? new StringFormat { Alignment = StringAlignment.Near }
                : minutes == 0 ? new StringFormat { Alignment = StringAlignment.Far } : center;
            float w = 70;
            float lx = minutes == 90 ? x : minutes == 0 ? x - w : x - w / 2;
            g.DrawString(label, Font, textBrush, new RectangleF(lx, plot.Bottom + 3, w, Font.Height), format);
        }

        var now = DateTimeOffset.Now;
        double span = Constants.HistoryDuration.TotalSeconds;
        g.SetClip(plot);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var linePen = new Pen(Color.FromArgb(90, 180, 255), 1.5f);
        var run = new List<PointF>();
        void Flush()
        {
            if (run.Count >= 2)
            {
                g.DrawLines(linePen, run.ToArray());
            }
            else if (run.Count == 1)
            {
                g.DrawLine(linePen, run[0].X - 0.5f, run[0].Y, run[0].X + 0.5f, run[0].Y);
            }
            run.Clear();
        }
        foreach (var m in _items)
        {
            if (m.Dba is not double dba)
            {
                Flush();
                continue;
            }
            double age = (now - m.Timestamp).TotalSeconds;
            if (age > span)
            {
                continue;
            }
            run.Add(new PointF(plot.Right - plot.Width * (float)(Math.Max(age, 0) / span), YOf(dba)));
        }
        Flush();
        g.ResetClip();
    }
}
