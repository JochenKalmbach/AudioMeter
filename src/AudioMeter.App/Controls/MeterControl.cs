using AudioMeter.Core;
using AudioMeter.Core.Levels;

namespace AudioMeter.App.Controls;

public sealed class MeterControl : Control
{
    private static readonly Color Green = Color.FromArgb(46, 184, 76);
    private static readonly Color Yellow = Color.FromArgb(240, 192, 32);
    private static readonly Color Red = Color.FromArgb(224, 52, 52);

    private double? _dba;
    private LevelZones _zones = LevelZones.Default;
    private bool _isCalibrated;
    private string? _status;

    public MeterControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        BackColor = Color.FromArgb(32, 32, 32);
        ForeColor = Color.White;
    }

    public double? Dba
    {
        get => _dba;
        set { _dba = value; Invalidate(); }
    }

    public LevelZones Zones
    {
        get => _zones;
        set { _zones = value; Invalidate(); }
    }

    public bool IsCalibrated
    {
        get => _isCalibrated;
        set { _isCalibrated = value; Invalidate(); }
    }

    /// <summary>Message shown under the bar, e.g. why there is no signal.</summary>
    public string? StatusText
    {
        get => _status;
        set { _status = value; Invalidate(); }
    }

    private static Color ColorOf(Zone zone) => zone switch
    {
        Zone.Green => Green,
        Zone.Yellow => Yellow,
        _ => Red,
    };

    private static float Fraction(double dba) =>
        (float)((Math.Clamp(dba, Constants.ScaleMinDba, Constants.ScaleMaxDba) - Constants.ScaleMinDba)
            / (Constants.ScaleMaxDba - Constants.ScaleMinDba));

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        const int margin = 8;
        int footer = Math.Max(16, Font.Height + 4);
        var bar = new RectangleF(margin, margin, Width - 2 * margin, Height - 2 * margin - footer);
        if (bar.Width < 10 || bar.Height < 10)
        {
            return;
        }

        float xGy = bar.Left + bar.Width * Fraction(_zones.GreenYellowLimit);
        float xYr = bar.Left + bar.Width * Fraction(_zones.YellowRedLimit);
        DrawSegment(g, bar.Left, xGy, bar, Dim(Green));
        DrawSegment(g, xGy, xYr, bar, Dim(Yellow));
        DrawSegment(g, xYr, bar.Right, bar, Dim(Red));

        if (_dba is double value)
        {
            float xValue = bar.Left + bar.Width * Fraction(value);
            DrawSegment(g, bar.Left, xValue, bar, ColorOf(_zones.ZoneOf(value)));
        }

        string text = _dba is double v ? $"{Math.Round(v):0} dBA" : "No signal";
        using var big = new Font(Font.FontFamily, Math.Max(8f, Math.Min(bar.Height * 0.38f, bar.Width * 0.12f)), FontStyle.Bold, GraphicsUnit.Pixel);
        var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using (var shadow = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
        {
            g.DrawString(text, big, shadow, new RectangleF(bar.X + 2, bar.Y + 2, bar.Width, bar.Height), format);
        }
        using (var brush = new SolidBrush(Color.White))
        {
            g.DrawString(text, big, brush, bar, format);
        }

        using var small = new SolidBrush(Color.FromArgb(190, 190, 190));
        float labelY = bar.Bottom + 2;
        DrawTick(g, small, bar.Left, Constants.ScaleMinDba, labelY, StringAlignment.Near);
        DrawTick(g, small, xGy, _zones.GreenYellowLimit, labelY, StringAlignment.Center);
        DrawTick(g, small, xYr, _zones.YellowRedLimit, labelY, StringAlignment.Center);
        DrawTick(g, small, bar.Right, Constants.ScaleMaxDba, labelY, StringAlignment.Far);

        string? status = _status ?? (_isCalibrated ? null : "Uncalibrated – values are approximate");
        if (status is not null)
        {
            using var warn = new SolidBrush(_status is null ? Color.FromArgb(240, 192, 32) : Color.FromArgb(255, 140, 120));
            var rect = new RectangleF(margin, bar.Bottom + 2, Width - 2 * margin, footer);
            var f = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
            g.DrawString(status, Font, warn, new RectangleF(rect.X, rect.Y + Font.Height - 2, rect.Width, rect.Height), f);
        }
    }

    private void DrawTick(Graphics g, Brush brush, float x, double dba, float y, StringAlignment align)
    {
        var format = new StringFormat { Alignment = align };
        const float w = 60;
        float left = align switch
        {
            StringAlignment.Near => x,
            StringAlignment.Far => x - w,
            _ => x - w / 2,
        };
        g.DrawString(dba.ToString("0"), Font, brush, new RectangleF(left, y, w, Font.Height + 2), format);
    }

    private static Color Dim(Color c) => Color.FromArgb(c.R / 3, c.G / 3, c.B / 3);

    private static void DrawSegment(Graphics g, float x1, float x2, RectangleF bar, Color color)
    {
        if (x2 - x1 < 0.5f)
        {
            return;
        }
        using var brush = new SolidBrush(color);
        g.FillRectangle(brush, x1, bar.Top, x2 - x1, bar.Height);
    }
}
