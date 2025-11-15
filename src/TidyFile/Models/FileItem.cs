namespace TidyFile.Models;

/// <summary>
/// Represents a file discovered in source folders with metadata.
/// </summary>
public class FileItem
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? AssignedCategory { get; set; }
    public bool IsClassified { get; set; }

    /// <summary>
    /// Gets the file size as a human-readable string.
    /// </summary>
    public string GetFormattedSize()
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = SizeBytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return len == (long)len 
            ? $"{len:0} {sizes[order]}" 
            : $"{len:0.00} {sizes[order]}";
    }

    public override string ToString()
    {
        return $"{FileName} ({GetFormattedSize()}) - Category: {AssignedCategory ?? "Unassigned"}";
    }
}
