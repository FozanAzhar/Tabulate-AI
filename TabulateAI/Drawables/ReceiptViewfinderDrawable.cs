namespace TabulateAI.Drawables;

public class ReceiptViewfinderDrawable : IDrawable
{
    public float ScanLineY { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var cx = dirtyRect.Width / 2f;
        var cy = dirtyRect.Height / 2f;
        const float w = 188f;
        const float h = 238f;
        var left = cx - w / 2f;
        var top = cy - h / 2f;
        var right = cx + w / 2f;
        var bottom = cy + h / 2f;
        const float arm = 22f;
        const float stroke = 2.5f;

        canvas.StrokeColor = Color.FromArgb("#30A78BFA");
        canvas.StrokeSize = 1f;
        canvas.DrawRectangle(left, top, w, h);

        canvas.StrokeColor = Color.FromArgb("#A78BFA");
        canvas.StrokeSize = stroke;
        canvas.StrokeLineCap = LineCap.Square;

        canvas.DrawLine(left, top + arm, left, top);
        canvas.DrawLine(left, top, left + arm, top);

        canvas.DrawLine(right - arm, top, right, top);
        canvas.DrawLine(right, top, right, top + arm);

        canvas.DrawLine(left, bottom - arm, left, bottom);
        canvas.DrawLine(left, bottom, left + arm, bottom);

        canvas.DrawLine(right - arm, bottom, right, bottom);
        canvas.DrawLine(right, bottom, right, bottom - arm);

        if (ScanLineY >= top + 8f && ScanLineY <= bottom - 8f)
        {
            canvas.StrokeColor = Color.FromArgb("#80A78BFA");
            canvas.StrokeSize = 1.5f;
            canvas.DrawLine(left + 8f, ScanLineY, right - 8f, ScanLineY);
        }
    }
}
