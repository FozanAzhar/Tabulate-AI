namespace TabulateAI.Services;

public interface IBackupService
{
    Task<string> CreateBackupAsync();

    Task RestoreBackupAsync(string zipPath);

    Task ShareBackupAsync(string zipPath);
}
