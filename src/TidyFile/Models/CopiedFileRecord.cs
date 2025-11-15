namespace TidyFile.Models;

using System;

/// <summary>
/// Record of a file that has been copied to the output folder.
/// Used to track and exclude already-processed files from future discoveries.
/// </summary>
public class CopiedFileRecord
{
    /// <summary>
    /// Full path of the source file.
    /// </summary>
    public string SourceFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Destination path where the file was copied.
    /// </summary>
    public string DestinationFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Category the file was assigned to.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the file was copied.
    /// </summary>
    public DateTime CopiedAt { get; set; }

    /// <summary>
    /// File hash for future duplicate detection.
    /// </summary>
    public string? FileHash { get; set; }
}
