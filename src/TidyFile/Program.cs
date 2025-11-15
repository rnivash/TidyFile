using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TidyFile.Interfaces;
using TidyFile.Services;
using TidyFile.UI;

// Setup Serilog
var logPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "TidyFile",
    "logs",
    "tidyfile-.txt");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Setup dependency injection
var services = new ServiceCollection();

services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddSerilog();
});

services.AddScoped<IFileService, FileService>();
services.AddScoped<ICategoryService, CategoryService>();
services.AddScoped<AppConfigService>();
services.AddScoped<ApplicationUI>(sp =>
    new ApplicationUI(
        sp.GetRequiredService<IFileService>(),
        sp.GetRequiredService<ICategoryService>(),
        sp.GetRequiredService<AppConfigService>(),
        sp.GetRequiredService<ILogger<ApplicationUI>>()));

var serviceProvider = services.BuildServiceProvider();

try
{
    // For now, run in interactive mode (batch mode can be added later)
    var ui = serviceProvider.GetRequiredService<ApplicationUI>();
    await ui.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
