using System.Globalization;
using SkiaSharp;
using TabulateAI.Models;

namespace TabulateAI.Helpers;

public static class PdfReportBuilder
{
    private const float PageWidth = 595f;
    private const float PageHeight = 842f;
    private const float Margin = 48f;
    private const float RowHeight = 22f;
    private const float ThumbWidth = 108f;
    private const float ThumbHeight = 136f;
    private const float ThumbCaptionHeight = 34f;
    private const float ThumbColumnGap = 18f;

    public static byte[] Build(IReadOnlyList<Receipt> receipts, string periodLabel)
    {
        using var stream = new MemoryStream();
        using var document = SKDocument.CreatePdf(stream);

        var titleTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Bold);
        var bodyTypeface = SKTypeface.FromFamilyName("sans-serif");
        var boldTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Bold);

        using var titlePaint = CreatePaint(titleTypeface, 20, "#111827");
        using var subtitlePaint = CreatePaint(bodyTypeface, 11, "#6B7280");
        using var bodyPaint = CreatePaint(bodyTypeface, 10, "#111827");
        using var boldPaint = CreatePaint(boldTypeface, 10, "#111827");
        using var mutedPaint = CreatePaint(bodyTypeface, 9, "#6B7280");
        using var headerBgPaint = new SKPaint { Color = SKColor.Parse("#7C3AED"), IsAntialias = true };
        using var headerTextPaint = CreatePaint(boldTypeface, 9, "#FFFFFF");
        using var linePaint = new SKPaint { Color = SKColor.Parse("#E5E7EB"), StrokeWidth = 1, IsAntialias = true };
        using var accentPaint = CreatePaint(boldTypeface, 16, "#7C3AED");
        using var thumbBorderPaint = new SKPaint
        {
            Color = SKColor.Parse("#D1D5DB"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true
        };
        using var placeholderPaint = new SKPaint { Color = SKColor.Parse("#F3F4F6"), IsAntialias = true };

        SKCanvas? canvas = null;
        var y = Margin;

        void BeginPage()
        {
            canvas = document.BeginPage(PageWidth, PageHeight);
            y = Margin;
        }

        void EndPage()
        {
            if (canvas is not null)
            {
                document.EndPage();
                canvas = null;
            }
        }

        void EnsureSpace(float requiredHeight)
        {
            if (canvas is null || y + requiredHeight > PageHeight - Margin)
            {
                EndPage();
                BeginPage();
            }
        }

        BeginPage();

        var total = receipts.Sum(r => r.Amount);
        var generated = DateTime.Now.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);

        canvas!.DrawText("Expensely Expense Report", Margin, y + titlePaint.TextSize, titlePaint);
        y += 30;
        canvas.DrawText($"Period: {periodLabel}", Margin, y, subtitlePaint);
        y += 16;
        canvas.DrawText($"Generated: {generated}", Margin, y, subtitlePaint);
        y += 16;
        canvas.DrawText($"Total: {total:C2}  ·  {receipts.Count} receipt{(receipts.Count == 1 ? string.Empty : "s")}", Margin, y, accentPaint);
        y += 28;

        EnsureSpace(RowHeight * 2);
        DrawTableHeader(canvas, ref y, headerBgPaint, headerTextPaint);

        foreach (var receipt in receipts.OrderByDescending(r => r.Date))
        {
            EnsureSpace(RowHeight);
            DrawReceiptRow(canvas, ref y, receipt, bodyPaint, mutedPaint, linePaint);
        }

        y += 16;
        EnsureSpace(RowHeight * 3);
        canvas.DrawText("Category summary", Margin, y + boldPaint.TextSize, boldPaint);
        y += 22;

        DrawCategorySummaryHeader(canvas, ref y, headerBgPaint, headerTextPaint);

        foreach (var group in receipts.GroupBy(r => r.Category).OrderByDescending(g => g.Sum(r => r.Amount)))
        {
            EnsureSpace(RowHeight);
            DrawSummaryRow(canvas, ref y, group.Key, group.Count().ToString(CultureInfo.InvariantCulture), group.Sum(r => r.Amount).ToString("C2", CultureInfo.CurrentCulture), bodyPaint, linePaint);
        }

        y += 24;
        EnsureSpace(ThumbHeight + ThumbCaptionHeight + 24);
        canvas.DrawText("Receipt images", Margin, y + boldPaint.TextSize, boldPaint);
        y += 22;

        DrawReceiptThumbnails(
            canvas,
            ref y,
            receipts,
            EnsureSpace,
            bodyPaint,
            mutedPaint,
            thumbBorderPaint,
            placeholderPaint);

        EndPage();
        document.Close();
        return stream.ToArray();
    }

    private static void DrawReceiptThumbnails(
        SKCanvas canvas,
        ref float y,
        IReadOnlyList<Receipt> receipts,
        Action<float> ensureSpace,
        SKPaint bodyPaint,
        SKPaint mutedPaint,
        SKPaint borderPaint,
        SKPaint placeholderPaint)
    {
        var ordered = receipts.OrderByDescending(r => r.Date).ToList();
        var column = 0;
        var rowY = y;

        foreach (var receipt in ordered)
        {
            var blockHeight = ThumbHeight + ThumbCaptionHeight + 12f;
            if (column == 0)
            {
                ensureSpace(blockHeight);
                rowY = y;
            }

            var x = Margin + column * (ThumbWidth + ThumbColumnGap);
            var thumbRect = new SKRect(x, rowY, x + ThumbWidth, rowY + ThumbHeight);

            canvas.DrawRoundRect(thumbRect, 6, 6, placeholderPaint);
            canvas.DrawRoundRect(thumbRect, 6, 6, borderPaint);

            using var thumbnail = LoadThumbnail(receipt.ImagePath, (int)ThumbWidth, (int)ThumbHeight);
            if (thumbnail is not null)
            {
                var dest = FitRect(thumbRect, thumbnail.Width, thumbnail.Height, inset: 4f);
                canvas.DrawBitmap(thumbnail, dest);
                canvas.DrawRoundRect(thumbRect, 6, 6, borderPaint);
            }
            else
            {
                var noImage = "No image";
                var noImageWidth = mutedPaint.MeasureText(noImage);
                canvas.DrawText(
                    noImage,
                    thumbRect.MidX - noImageWidth / 2f,
                    thumbRect.MidY,
                    mutedPaint);
            }

            var captionY = rowY + ThumbHeight + 12f;
            canvas.DrawText(Truncate(receipt.Merchant, 18), x, captionY, bodyPaint);

            var meta = $"{receipt.Date:dd MMM yyyy} · {receipt.Amount:C2}";
            canvas.DrawText(meta, x, captionY + 12f, mutedPaint);

            column++;
            if (column >= 2)
            {
                column = 0;
                y = rowY + blockHeight;
            }
        }

        if (column > 0)
        {
            y = rowY + ThumbHeight + ThumbCaptionHeight + 12f;
        }
    }

    private static SKRect FitRect(SKRect bounds, int imageWidth, int imageHeight, float inset)
    {
        var inner = new SKRect(
            bounds.Left + inset,
            bounds.Top + inset,
            bounds.Right - inset,
            bounds.Bottom - inset);

        var scale = Math.Min(inner.Width / imageWidth, inner.Height / imageHeight);
        var width = imageWidth * scale;
        var height = imageHeight * scale;

        return new SKRect(
            inner.MidX - width / 2f,
            inner.MidY - height / 2f,
            inner.MidX + width / 2f,
            inner.MidY + height / 2f);
    }

    private static SKBitmap? LoadThumbnail(string imagePath, int maxWidth, int maxHeight)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            return null;
        }

        try
        {
            using var stream = File.OpenRead(imagePath);
            using var bitmap = SKBitmap.Decode(stream);
            if (bitmap is null)
            {
                return null;
            }

            var scale = Math.Min((float)maxWidth / bitmap.Width, (float)maxHeight / bitmap.Height);
            var targetWidth = Math.Max(1, (int)(bitmap.Width * scale));
            var targetHeight = Math.Max(1, (int)(bitmap.Height * scale));

            if (targetWidth == bitmap.Width && targetHeight == bitmap.Height)
            {
                return bitmap.Copy();
            }

            return bitmap.Resize(new SKImageInfo(targetWidth, targetHeight), SKFilterQuality.Medium);
        }
        catch
        {
            return null;
        }
    }

    private static void DrawTableHeader(SKCanvas canvas, ref float y, SKPaint bgPaint, SKPaint textPaint)
    {
        var headerRect = new SKRect(Margin, y, PageWidth - Margin, y + RowHeight);
        canvas.DrawRect(headerRect, bgPaint);

        canvas.DrawText("Date", Margin + 8, y + 15, textPaint);
        canvas.DrawText("Merchant", Margin + 88, y + 15, textPaint);
        canvas.DrawText("Category", Margin + 280, y + 15, textPaint);

        var amountLabel = "Amount";
        var width = textPaint.MeasureText(amountLabel);
        canvas.DrawText(amountLabel, PageWidth - Margin - 8 - width, y + 15, textPaint);
        y += RowHeight;
    }

    private static void DrawCategorySummaryHeader(SKCanvas canvas, ref float y, SKPaint bgPaint, SKPaint textPaint)
    {
        var headerRect = new SKRect(Margin, y, PageWidth - Margin, y + RowHeight);
        canvas.DrawRect(headerRect, bgPaint);

        canvas.DrawText("Category", Margin + 8, y + 15, textPaint);
        canvas.DrawText("Count", Margin + 280, y + 15, textPaint);

        var totalLabel = "Total";
        var width = textPaint.MeasureText(totalLabel);
        canvas.DrawText(totalLabel, PageWidth - Margin - 8 - width, y + 15, textPaint);
        y += RowHeight;
    }

    private static void DrawReceiptRow(SKCanvas canvas, ref float y, Receipt receipt, SKPaint bodyPaint, SKPaint mutedPaint, SKPaint linePaint)
    {
        var date = receipt.Date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        var amount = receipt.Amount.ToString("C2", CultureInfo.CurrentCulture);

        canvas.DrawText(date, Margin + 8, y + 15, mutedPaint);
        canvas.DrawText(Truncate(receipt.Merchant, 26), Margin + 88, y + 15, bodyPaint);
        canvas.DrawText(Truncate(receipt.Category, 16), Margin + 280, y + 15, bodyPaint);

        var amountWidth = bodyPaint.MeasureText(amount);
        canvas.DrawText(amount, PageWidth - Margin - 8 - amountWidth, y + 15, bodyPaint);
        canvas.DrawLine(Margin, y + RowHeight, PageWidth - Margin, y + RowHeight, linePaint);
        y += RowHeight;
    }

    private static void DrawSummaryRow(SKCanvas canvas, ref float y, string category, string count, string total, SKPaint bodyPaint, SKPaint linePaint)
    {
        canvas.DrawText(Truncate(category, 28), Margin + 8, y + 15, bodyPaint);
        canvas.DrawText(count, Margin + 280, y + 15, bodyPaint);

        var totalWidth = bodyPaint.MeasureText(total);
        canvas.DrawText(total, PageWidth - Margin - 8 - totalWidth, y + 15, bodyPaint);
        canvas.DrawLine(Margin, y + RowHeight, PageWidth - Margin, y + RowHeight, linePaint);
        y += RowHeight;
    }

    private static SKPaint CreatePaint(SKTypeface typeface, float size, string hexColor)
    {
        return new SKPaint
        {
            Color = SKColor.Parse(hexColor),
            TextSize = size,
            IsAntialias = true,
            Typeface = typeface
        };
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";
    }
}
