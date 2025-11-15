namespace TidyFile.Services;

using System.Diagnostics;
using TidyFile.Interfaces;
using TidyFile.Models;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for file discovery, metadata extraction, and organization.
/// </summary>
public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;

    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
    }

    public async Task<List<FileItem>> DiscoverFilesAsync(List<string> sourceFolders)
    {
        var files = new List<FileItem>();

        await Task.Run(() =>
        {
            foreach (var folder in sourceFolders)
            {
                if (!Directory.Exists(folder))
                {
                    _logger.LogWarning("Source folder does not exist: {Folder}", folder);
                    continue;
                }

                try
                {
                    var dirInfo = new DirectoryInfo(folder);
                    var fileInfos = dirInfo.GetFiles("*", SearchOption.AllDirectories);

                    foreach (var fileInfo in fileInfos)
                    {
                        var fileItem = new FileItem
                        {
                            FilePath = fileInfo.FullName,
                            FileName = fileInfo.Name,
                            SizeBytes = fileInfo.Length,
                            CreatedAt = fileInfo.CreationTime,
                            ModifiedAt = fileInfo.LastWriteTime,
                            IsClassified = false,
                            AssignedCategory = null
                        };
                        files.Add(fileItem);
                    }

                    _logger.LogInformation("Discovered {Count} files in {Folder}", fileInfos.Length, folder);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error discovering files in folder: {Folder}", folder);
                }
            }
        });

        return files;
    }

    public async Task<FileItem?> GetFileMetadataAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                    return null;

                return new FileItem
                {
                    FilePath = fileInfo.FullName,
                    FileName = fileInfo.Name,
                    SizeBytes = fileInfo.Length,
                    CreatedAt = fileInfo.CreationTime,
                    ModifiedAt = fileInfo.LastWriteTime,
                    IsClassified = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metadata for file: {FilePath}", filePath);
                return null;
            }
        });
    }

    public async Task<ClassificationResult> CopyClassifiedFilesAsync(
        List<FileItem> files,
        string outputFolder,
        IProgress<(int Current, int Total, string Message)> progress)
    {
        var result = new ClassificationResult { Success = true };
        var classifiedFiles = files.Where(f => f.IsClassified && !string.IsNullOrEmpty(f.AssignedCategory)).ToList();

        if (!classifiedFiles.Any())
        {
            result.Success = false;
            result.Message = "No classified files to copy.";
            return result;
        }

        // Create output folder structure
        try
        {
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating output folder: {OutputFolder}", outputFolder);
            result.Success = false;
            result.Message = $"Failed to create output folder: {ex.Message}";
            result.Errors.Add(result.Message);
            return result;
        }

        int currentIndex = 0;
        await Task.Run(() =>
        {
            foreach (var file in classifiedFiles)
            {
                try
                {
                    var categoryFolder = Path.Combine(outputFolder, file.AssignedCategory!);
                    Directory.CreateDirectory(categoryFolder);

                    var destinationPath = Path.Combine(categoryFolder, file.FileName);

                    // Handle file name conflicts
                    if (File.Exists(destinationPath))
                    {
                        destinationPath = HandleFileConflict(categoryFolder, file.FileName);
                        result.FilesSkipped++;
                    }

                    // Copy file preserving metadata
                    File.Copy(file.FilePath, destinationPath, true);
                    File.SetCreationTime(destinationPath, file.CreatedAt);
                    File.SetLastWriteTime(destinationPath, file.ModifiedAt);

                    result.FilesCopied++;
                    result.TotalBytesCopied += file.SizeBytes;

                    currentIndex++;
                    progress?.Report((currentIndex, classifiedFiles.Count, $"Copied: {file.FileName}"));

                    _logger.LogInformation("Copied file: {FileName} to {Category}", file.FileName, file.AssignedCategory);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error copying file: {FilePath}", file.FilePath);
                    result.Errors.Add($"Error copying {file.FileName}: {ex.Message}");
                }
            }
        });

        result.Message = $"Successfully copied {result.FilesCopied} files. Skipped {result.FilesSkipped} files due to conflicts.";
        return result;
    }

    public async Task OpenFileAsync(string filePath)
    {
        await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File does not exist: {FilePath}", filePath);
                    return;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };

                Process.Start(psi);
                _logger.LogInformation("Opened file: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening file: {FilePath}", filePath);
            }
        });
    }

    public async Task<string> PreviewTextFileAsync(string filePath, int lineCount = 20)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(filePath))
                    return "File not found.";

                var lines = File.ReadLines(filePath).Take(lineCount).ToList();
                return string.Join(Environment.NewLine, lines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing file: {FilePath}", filePath);
                return $"Error reading file: {ex.Message}";
            }
        });
    }

    private string HandleFileConflict(string folder, string fileName)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        int counter = 1;

        string newPath;
        do
        {
            newPath = Path.Combine(folder, $"{fileNameWithoutExtension}_{counter}{extension}");
            counter++;
        } while (File.Exists(newPath));

        return newPath;
    }
}
