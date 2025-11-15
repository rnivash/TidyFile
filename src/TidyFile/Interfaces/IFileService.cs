namespace TidyFile.Interfaces;

using TidyFile.Models;

/// <summary>
/// Interface for file discovery and operations.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Discovers all files in the specified source folders.
    /// </summary>
    Task<List<FileItem>> DiscoverFilesAsync(List<string> sourceFolders);

    /// <summary>
    /// Gets metadata for a specific file.
    /// </summary>
    Task<FileItem?> GetFileMetadataAsync(string filePath);

    /// <summary>
    /// Copies classified files to the output folder organized by category.
    /// </summary>
    Task<ClassificationResult> CopyClassifiedFilesAsync(
        List<FileItem> files,
        string outputFolder,
        IProgress<(int Current, int Total, string Message)> progress);

    /// <summary>
    /// Opens a file with the default program.
    /// </summary>
    Task OpenFileAsync(string filePath);

    /// <summary>
    /// Reads first N lines of a text file for preview.
    /// </summary>
    Task<string> PreviewTextFileAsync(string filePath, int lineCount = 20);
}
