namespace TidyFile.Models;

/// <summary>
/// Represents the result of a file classification and copy operation.
/// </summary>
public class ClassificationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int FilesCopied { get; set; }
    public int FilesSkipped { get; set; }
    public long TotalBytesCopied { get; set; }
    public List<string> Errors { get; set; } = new();
}
