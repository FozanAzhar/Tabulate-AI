namespace SpendSmart.Services;

public interface IImageStorageService
{
    Task<string> SaveReceiptImageAsync(Stream imageStream, string extension = ".jpg");

    Task<string> SaveReceiptImageFromFileAsync(string sourcePath);

    bool ImageExists(string imagePath);

    Task DeleteImageAsync(string imagePath);
}
