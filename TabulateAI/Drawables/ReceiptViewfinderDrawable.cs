namespace TabulateAI.Drawables;

public class ReceiptViewfinderDrawable : IDrawable
{
    public float ScanLineY { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        const float viewfinderWidth = 190f;
        const float viewfinderHeight = 240f;
        const float cornerArm = 20f;
        const float strokeWidth = 2.5f;

        var left = (dirtyRect.Width - viewfinderWidth) / 2f;
        var top = (dirtyRect.Height - viewfinderHeight) / 2f;
        var right = left + viewfinderWidth;
        var bottom = top + viewfinderHeight;

        canvas.StrokeColor = Color.FromArgb("#C8922A");
        canvas.StrokeSize = strokeWidth;
        canvas.StrokeLineCap = LineCap.Square;

        DrawCorner(canvas, left, top, cornerArm, true, true);
        DrawCorner(canvas, right, top, cornerArm, false, true);
        DrawCorner(canvas, left, bottom, cornerArm, true, false);
        DrawCorner(canvas, right, bottom, cornerArm, false, false);

        var scanY = top + ScanLineY;
        if (scanY >= top + 8 && scanY <= bottom - 8)
        {
            canvas.StrokeColor = Color.FromArgb("#80C8922A");
            canvas.StrokeSize = 1.5f;
            canvas.DrawLine(left + 8, scanY, right - 8, scanY);
        }
    }

    private static void DrawCorner(ICanvas canvas, float x, float y, float arm, bool isLeft, bool isTop)
    {
        if (isLeft && isTop)
        {
            canvas.DrawLine(x, y, x + arm, y);
            canvas.DrawLine(x, y, x, y + arm);
        }
        else if (!isLeft && isTop)
        {
            canvas.DrawLine(x, y, x - arm, y);
            canvas.DrawLine(x, y, x, y + arm);
        }
        else if (isLeft && !isTop)
        {
            canvas.DrawLine(x, y, x + arm, y);
            canvas.DrawLine(x, y, x, y - arm);
        }
        else
        {
            canvas.DrawLine(x, y, x - arm, y);
            canvas.DrawLine(x, y, x, y - arm);
        }
    }
}
