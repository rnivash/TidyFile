using System.Text.Json;
using TidyFile.Models;
using Microsoft.Extensions.Logging;

namespace TidyFile.Services;

/// <summary>
/// Service for tracking and managing copied files to avoid re-processing them.
/// </summary>
public class CopiedFilesTrackerService
{
    private readonly ILogger<CopiedFilesTrackerService> _logger;
    private readonly string _trackingFilePath;
    private List<CopiedFileRecord> _copiedFiles = new();

    public CopiedFilesTrackerService(ILogger<CopiedFilesTrackerService> logger, string? trackingFilePath = null)
    {
        _logger = logger;
        _trackingFilePath = trackingFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TidyFile",
            "copiedfiles.json");
    }

    /// <summary>
    /// Get all tracked copied files.
    /// </summary>
    public List<CopiedFileRecord> GetCopiedFiles() => new List<CopiedFileRecord>(_copiedFiles);

    /// <summary>
    /// Check if a file has already been copied based on its source path.
    /// </summary>
    public bool IsFileCopied(string sourceFilePath)
    {
        return _copiedFiles.Any(cf => 
            cf.SourceFilePath.Equals(sourceFilePath, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Add a file record as copied.
    /// </summary>
    public void AddCopiedFile(string sourceFilePath, string destinationFilePath, string category)
    {
        var record = new CopiedFileRecord
        {
            SourceFilePath = sourceFilePath,
            DestinationFilePath = destinationFilePath,
            Category = category,
            CopiedAt = DateTime.UtcNow
        };

        if (!_copiedFiles.Any(cf => cf.SourceFilePath.Equals(sourceFilePath, StringComparison.OrdinalIgnoreCase)))
        {
            _copiedFiles.Add(record);
            _logger.LogInformation("Added copied file record: {SourcePath}", sourceFilePath);
        }
    }

    /// <summary>
    /// Remove a file record from tracking (if it needs to be re-processed).
    /// </summary>
    public bool RemoveCopiedFile(string sourceFilePath)
    {
        var record = _copiedFiles.FirstOrDefault(cf => 
            cf.SourceFilePath.Equals(sourceFilePath, StringComparison.OrdinalIgnoreCase));

        if (record != null)
        {
            _copiedFiles.Remove(record);
            _logger.LogInformation("Removed copied file record: {SourcePath}", sourceFilePath);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Clear all tracking records.
    /// </summary>
    public void ClearAllRecords()
    {
        _copiedFiles.Clear();
        _logger.LogInformation("Cleared all copied file records");
    }

    /// <summary>
    /// Load tracking data from persistent storage.
    /// </summary>
    public async Task LoadTrackingDataAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                if (File.Exists(_trackingFilePath))
                {
                    var json = File.ReadAllText(_trackingFilePath);
                    var loadedRecords = JsonSerializer.Deserialize<List<CopiedFileRecord>>(json);
                    _copiedFiles = loadedRecords ?? new List<CopiedFileRecord>();
                    _logger.LogInformation("Loaded {Count} copied file records from {TrackingPath}", 
                        _copiedFiles.Count, _trackingFilePath);
                }
                else
                {
                    _copiedFiles = new List<CopiedFileRecord>();
                    _logger.LogInformation("No existing tracking file found at {TrackingPath}", _trackingFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tracking data from {TrackingPath}", _trackingFilePath);
                _copiedFiles = new List<CopiedFileRecord>();
            }
        });
    }

    /// <summary>
    /// Save tracking data to persistent storage.
    /// </summary>
    public async Task SaveTrackingDataAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                var directory = Path.GetDirectoryName(_trackingFilePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(_copiedFiles, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_trackingFilePath, json);
                _logger.LogInformation("Saved {Count} copied file records to {TrackingPath}", 
                    _copiedFiles.Count, _trackingFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving tracking data to {TrackingPath}", _trackingFilePath);
                throw;
            }
        });
    }
}
