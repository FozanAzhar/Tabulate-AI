namespace TabulateAI.Services;

public class ImageStorageService : IImageStorageService
{
    private readonly string _receiptsDirectory;

    public ImageStorageService()
    {
        _receiptsDirectory = Path.Combine(FileSystem.AppDataDirectory, "receipts");
        Directory.CreateDirectory(_receiptsDirectory);
    }

    public async Task<string> SaveReceiptImageAsync(Stream imageStream, string extension = ".jpg")
    {
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(_receiptsDirectory, fileName);

        await using var fileStream = File.Create(destinationPath);
        await imageStream.CopyToAsync(fileStream);

        return destinationPath;
    }

    public async Task<string> SaveReceiptImageFromFileAsync(string sourcePath)
    {
        var extension = Path.GetExtension(sourcePath);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".jpg";
        }

        await using var sourceStream = File.OpenRead(sourcePath);
        return await SaveReceiptImageAsync(sourceStream, extension);
    }

    public bool ImageExists(string imagePath) =>
        !string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath);

    public Task DeleteImageAsync(string imagePath)
    {
        if (ImageExists(imagePath))
        {
            File.Delete(imagePath);
        }

        return Task.CompletedTask;
    }

    public Task ClearAllReceiptImagesAsync()
    {
        if (Directory.Exists(_receiptsDirectory))
        {
            foreach (var file in Directory.EnumerateFiles(_receiptsDirectory))
            {
                File.Delete(file);
            }
        }

        return Task.CompletedTask;
    }
}
