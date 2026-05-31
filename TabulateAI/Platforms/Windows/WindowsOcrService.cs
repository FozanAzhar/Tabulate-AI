#if WINDOWS
using TabulateAI.Models;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;

namespace TabulateAI.Services;

public class WindowsOcrService : IOcrService, ILocalOcrService
{
    public async Task<OcrExtractionResult> ExtractReceiptDataAsync(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            return new OcrExtractionResult();
        }

        try
        {
            var engine = OcrEngine.TryCreateFromUserProfileLanguages();
            if (engine is null)
            {
                return new OcrExtractionResult();
            }

            var file = await StorageFile.GetFileFromPathAsync(imagePath);
            using IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);

            var decoder = await BitmapDecoder.CreateAsync(stream);
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied);

            var ocrResult = await engine.RecognizeAsync(softwareBitmap);
            var rawText = ocrResult?.Text ?? string.Empty;

            return ReceiptParser.Parse(rawText);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Windows OCR failed: {ex.Message}");
            return new OcrExtractionResult();
        }
    }
}
#endif
