using System.Text.Json;
using TidyFile.Models;
using Microsoft.Extensions.Logging;

namespace TidyFile.Services;

public class AppConfigService
{
    private readonly ILogger<AppConfigService> _logger;
    private readonly string _configPath;
    private AppConfig _config = new();

    public AppConfigService(ILogger<AppConfigService> logger, string? configPath = null)
    {
        _logger = logger;
        _configPath = configPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TidyFile",
            "appconfig.json");
    }

    public AppConfig GetConfig() => _config;

    public void SetSourceFolders(List<string> folders)
    {
        _config.SourceFolders = folders;
    }

    public void SetOutputFolder(string folder)
    {
        _config.OutputFolder = folder;
    }

    public async Task SaveConfigAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                var directory = Path.GetDirectoryName(_configPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
                _logger.LogInformation("App config saved to {ConfigPath}", _configPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving app config to {ConfigPath}", _configPath);
                throw;
            }
        });
    }

    public async Task LoadConfigAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    var loadedConfig = JsonSerializer.Deserialize<AppConfig>(json);
                    _config = loadedConfig ?? new AppConfig();
                    _logger.LogInformation("App config loaded from {ConfigPath}", _configPath);
                }
                else
                {
                    _config = new AppConfig();
                    _logger.LogInformation("No existing app config file found at {ConfigPath}", _configPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading app config from {ConfigPath}", _configPath);
                _config = new AppConfig();
            }
        });
    }
}
