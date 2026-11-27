using System.Drawing.Drawing2D;

namespace PilTakip;

static class BluetoothGlyph
{
    static readonly PointF[] Points =
    {
        new(5.5f, 7.5f), new(17.5f, 16.5f), new(11.5f, 21.5f),
        new(11.5f, 2.5f), new(17.5f, 7.5f), new(5.5f, 16.5f),
    };

    public static void Draw(Graphics g, Rectangle area, int alpha)
    {
        float size = Math.Min(area.Width, area.Height) * 0.75f;
        if (size < 40) return;

        float scale = size / 24f;
        float left = area.Left + (area.Width - 24f * scale) / 2f;
        float top = area.Top + (area.Height - 24f * scale) / 2f;
        var pts = Points.Select(p => new PointF(left + p.X * scale, top + p.Y * scale)).ToArray();

        var previous = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(Color.FromArgb(alpha, 0, 120, 212), scale * 2.2f)
        {
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
        };
        g.DrawLines(pen, pts);
        g.SmoothingMode = previous;
    }
}
