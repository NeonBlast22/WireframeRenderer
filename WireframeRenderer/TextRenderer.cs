using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;

namespace WireframeRenderer;

public class TextRenderer : IDisposable
{
    private readonly Font _font;
    private readonly TextOptions _textOptions;

    public TextRenderer(string fontPath, float size = 16f)
    {
        var collection = new FontCollection();
        var family = collection.Add(fontPath);
        _font = family.CreateFont(size, FontStyle.Regular);
        _textOptions = new TextOptions(_font);
    }

    public void DrawText(string text, int x, int y, Color color)
    {
        var rect = TextMeasurer.MeasureBounds(text, _textOptions); // use MeasureBounds instead
        int w = (int)MathF.Ceiling(rect.Width + rect.Left) + 2;   // account for offset + padding
        int h = (int)MathF.Ceiling(rect.Height + rect.Top) + 2;
        if (w <= 0 || h <= 0) return;

        using var image = new Image<Rgba32>(w, h);
        var drawColor = SixLabors.ImageSharp.Color.FromRgba(
            (byte)(color.R * 255),
            (byte)(color.G * 255),
            (byte)(color.B * 255),
            (byte)(color.A * 255));

        image.Mutate(ctx => ctx.DrawText(text, _font, drawColor, new PointF(0, 0)));

        for (int row = 0; row < h; row++)
        {
            for (int col = 0; col < w; col++)
            {
                var p = image[col, row];
                if (p.A == 0) continue;
                Screen.SetPixel(x + col, y + row, new Color(
                    p.R / 255f,
                    p.G / 255f,
                    p.B / 255f,
                    p.A / 255f));
            }
        }
    }

    public void Dispose() { }
}