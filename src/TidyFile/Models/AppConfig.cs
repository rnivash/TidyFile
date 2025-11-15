namespace TidyFile.Models;

public class AppConfig
{
    public List<string> SourceFolders { get; set; } = new();
    public string OutputFolder { get; set; } = string.Empty;
}
