namespace TidyFile.UI;

using Spectre.Console;
using TidyFile.Interfaces;
using TidyFile.Models;
using Microsoft.Extensions.Logging;
using TidyFile.Services;

/// <summary>
/// Main application UI controller using Spectre.Console for interactive experience.
/// </summary>
public class ApplicationUI
{
    private readonly IFileService _fileService;
    private readonly ICategoryService _categoryService;
    private readonly AppConfigService _appConfigService;
    private readonly ILogger<ApplicationUI> _logger;
    private List<FileItem> _discoveredFiles = new();
    private List<string> _sourceFolders = new();
    private string _outputFolder = string.Empty;

    public ApplicationUI(IFileService fileService, ICategoryService categoryService, AppConfigService appConfigService, ILogger<ApplicationUI> logger)
    {
        _fileService = fileService;
        _categoryService = categoryService;
        _appConfigService = appConfigService;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        AnsiConsole.MarkupLine("[bold blue]Welcome to TidyFile - File Organization System[/]");
        AnsiConsole.MarkupLine("[dim]Organize your files into custom categories[/]\n");

    // Load existing categories
    await _categoryService.LoadCategoriesAsync();
    // Load persisted source/output folders
    await _appConfigService.LoadConfigAsync();
    var config = _appConfigService.GetConfig();
    _sourceFolders = config.SourceFolders ?? new List<string>();
    _outputFolder = config.OutputFolder ?? string.Empty;

        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Main Menu[/]")
                    .AddChoices(
                        "Select Source Folders",
                        "Select Output Folder",
                        "Discover Files",
                        "View Files",
                        "Manage Categories",
                        "Classify Files",
                        "Copy Classified Files",
                        "Exit"));

            switch (choice)
            {
                case "Select Source Folders":
                    await SelectSourceFoldersAsync();
                    break;
                case "Select Output Folder":
                    SelectOutputFolder();
                    break;
                case "Discover Files":
                    await DiscoverFilesAsync();
                    break;
                case "View Files":
                    await ViewFilesAsync();
                    break;
                case "Manage Categories":
                    await ManageCategoriesAsync();
                    break;
                case "Classify Files":
                    await ClassifyFilesAsync();
                    break;
                case "Copy Classified Files":
                    await CopyClassifiedFilesAsync();
                    break;
                case "Exit":
                    AnsiConsole.MarkupLine("[bold green]Thank you for using TidyFile![/]");
                    return;
            }
        }
    }

    private async Task SelectSourceFoldersAsync()
    {
        await Task.CompletedTask;
        AnsiConsole.MarkupLine("[bold]Select Source Folders[/]\n");

        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Folder Selection Options")
                    .AddChoices("Add Folder", "View Selected Folders", "Clear All", "Done"));

            if (choice == "Add Folder")
            {
                AnsiConsole.MarkupLine("\n[yellow]Enter folder path (or 'cancel' to go back):[/]");
                var folderPath = Console.ReadLine();

                if (folderPath?.Equals("cancel", StringComparison.OrdinalIgnoreCase) == true)
                    continue;

                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    AnsiConsole.MarkupLine("[red]Invalid path![/]");
                    continue;
                }

                if (!Directory.Exists(folderPath))
                {
                    AnsiConsole.MarkupLine("[red]Directory does not exist![/]");
                    continue;
                }

                if (!_sourceFolders.Contains(folderPath))
                {
                    _sourceFolders.Add(folderPath);
                    AnsiConsole.MarkupLine($"[green]✓ Added: {folderPath}[/]\n");
                    _appConfigService.SetSourceFolders(_sourceFolders);
                    await _appConfigService.SaveConfigAsync();
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Folder already added![/]\n");
                }
            }
            else if (choice == "View Selected Folders")
            {
                if (!_sourceFolders.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No folders selected yet.[/]\n");
                }
                else
                {
                    var table = new Table();
                    table.AddColumn("[bold]Index[/]");
                    table.AddColumn("[bold]Folder Path[/]");
                    for (int i = 0; i < _sourceFolders.Count; i++)
                    {
                        table.AddRow((i + 1).ToString(), _sourceFolders[i]);
                    }
                    AnsiConsole.Write(table);
                    Console.WriteLine();
                }
            }
            else if (choice == "Clear All")
            {
                _sourceFolders.Clear();
                _appConfigService.SetSourceFolders(_sourceFolders);
                await _appConfigService.SaveConfigAsync();
                AnsiConsole.MarkupLine("[green]✓ All folders cleared![/]\n");
            }
            else if (choice == "Done")
            {
                break;
            }
        }
    }

    private void SelectOutputFolder()
    {
        AnsiConsole.MarkupLine("[bold]Select Output Folder[/]\n");
        AnsiConsole.MarkupLine("[yellow]Enter output folder path:[/]");
        var folderPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            AnsiConsole.MarkupLine("[red]Invalid path![/]");
            return;
        }

        // Create folder if it doesn't exist
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            _outputFolder = folderPath;
            _appConfigService.SetOutputFolder(_outputFolder);
            _appConfigService.SaveConfigAsync().Wait();
            AnsiConsole.MarkupLine($"[green]✓ Output folder set to: {_outputFolder}[/]\n");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error setting output folder: {ex.Message}[/]\n");
        }
    }

    private async Task DiscoverFilesAsync()
    {
        if (!_sourceFolders.Any())
        {
            AnsiConsole.MarkupLine("[red]No source folders selected. Please select folders first.[/]\n");
            return;
        }

        AnsiConsole.MarkupLine("[bold]Discovering files...[/]");
        await AnsiConsole.Status()
            .Start("[bold blue]Scanning folders...[/]", async ctx =>
            {
                _discoveredFiles = await _fileService.DiscoverFilesAsync(_sourceFolders);
            });

        AnsiConsole.MarkupLine($"[green]✓ Discovered {_discoveredFiles.Count} files.[/]\n");
    }

    private async Task ViewFilesAsync()
    {
        if (!_discoveredFiles.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No files discovered yet. Please discover files first.[/]\n");
            return;
        }

        int pageSize = 10;
        int currentPage = 0;
        int totalPages = (int)Math.Ceiling(_discoveredFiles.Count / (double)pageSize);

        while (true)
        {
            var pageFiles = _discoveredFiles
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToList();

            var table = new Table();
            table.AddColumn("[bold]#[/]");
            table.AddColumn("[bold]File Name[/]");
            table.AddColumn("[bold]Size[/]");
            table.AddColumn("[bold]Modified[/]");
            table.AddColumn("[bold]Category[/]");

            int startIndex = currentPage * pageSize;
            for (int i = 0; i < pageFiles.Count; i++)
            {
                var file = pageFiles[i];
                var categoryDisplay = file.IsClassified && !string.IsNullOrEmpty(file.AssignedCategory)
                    ? $"[green]{file.AssignedCategory}[/]"
                    : "[yellow]Unassigned[/]";
                
                table.AddRow(
                    (startIndex + i + 1).ToString(),
                    file.FileName,
                    file.GetFormattedSize(),
                    file.ModifiedAt.ToString("yyyy-MM-dd"),
                    categoryDisplay);
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"\n[dim]Page {currentPage + 1} of {totalPages} | Total Files: {_discoveredFiles.Count}[/]");

            var navOptions = new List<string>();
            if (currentPage > 0) navOptions.Add("Previous");
            if (currentPage < totalPages - 1) navOptions.Add("Next");
            navOptions.Add("Back");

            var navChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Navigation")
                    .AddChoices(navOptions));

            if (navChoice == "Previous")
                currentPage--;
            else if (navChoice == "Next")
                currentPage++;
            else if (navChoice == "Back")
                break;
        }
    }

    private async Task ManageCategoriesAsync()
    {
        await Task.CompletedTask;
        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Category Management[/]")
                    .AddChoices("Create Category", "View Categories", "Rename Category", "Delete Category", "Back"));

            switch (choice)
            {
                case "Create Category":
                    await CreateCategoryAsync();
                    break;
                case "View Categories":
                    await ViewCategoriesAsync();
                    break;
                case "Rename Category":
                    await RenameCategoryAsync();
                    break;
                case "Delete Category":
                    await DeleteCategoryAsync();
                    break;
                case "Back":
                    return;
            }
        }
    }

    private async Task CreateCategoryAsync()
    {
        AnsiConsole.MarkupLine("\n[yellow]Enter category name:[/]");
        var name = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name))
        {
            AnsiConsole.MarkupLine("[red]Category name cannot be empty.[/]\n");
            return;
        }

        AnsiConsole.MarkupLine("[yellow]Enter category description (optional, press Enter to skip):[/]");
        var description = Console.ReadLine() ?? "";

        var success = await _categoryService.CreateCategoryAsync(name, description);
        if (success)
        {
            AnsiConsole.MarkupLine($"[green]✓ Category '{name}' created successfully![/]\n");
            await _categoryService.SaveCategoriesAsync();
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]✗ Failed to create category '{name}'.[/]\n");
        }
    }

    private async Task ViewCategoriesAsync()
    {
        var categories = await _categoryService.GetCategoriesAsync();

        if (!categories.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No categories created yet.[/]\n");
            return;
        }

        var table = new Table();
        table.AddColumn("[bold]Category Name[/]");
        table.AddColumn("[bold]Description[/]");
        table.AddColumn("[bold]Created[/]");

        foreach (var category in categories)
        {
            table.AddRow(
                category.Name,
                category.Description ?? "-",
                category.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
        }

        AnsiConsole.Write(table);
        Console.WriteLine();
    }

    private async Task RenameCategoryAsync()
    {
        var categories = await _categoryService.GetCategoriesAsync();

        if (!categories.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No categories to rename.[/]\n");
            return;
        }

        var categoryName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select category to rename")
                .AddChoices(categories.Select(c => c.Name)));

        AnsiConsole.MarkupLine("[yellow]Enter new name:[/]");
        var newName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(newName))
        {
            AnsiConsole.MarkupLine("[red]New name cannot be empty.[/]\n");
            return;
        }

        var success = await _categoryService.RenameCategoryAsync(categoryName, newName);
        if (success)
        {
            AnsiConsole.MarkupLine($"[green]✓ Category renamed to '{newName}'![/]\n");
            await _categoryService.SaveCategoriesAsync();
        }
        else
        {
            AnsiConsole.MarkupLine("[red]✗ Failed to rename category.[/]\n");
        }
    }

    private async Task DeleteCategoryAsync()
    {
        var categories = await _categoryService.GetCategoriesAsync();

        if (!categories.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No categories to delete.[/]\n");
            return;
        }

        var categoryName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select category to delete")
                .AddChoices(categories.Select(c => c.Name)));

        if (AnsiConsole.Confirm($"[yellow]Are you sure you want to delete '{categoryName}'?[/]"))
        {
            var success = await _categoryService.DeleteCategoryAsync(categoryName);
            if (success)
            {
                AnsiConsole.MarkupLine($"[green]✓ Category '{categoryName}' deleted![/]\n");
                await _categoryService.SaveCategoriesAsync();
            }
            else
            {
                AnsiConsole.MarkupLine("[red]✗ Failed to delete category.[/]\n");
            }
        }
    }

    private async Task ClassifyFilesAsync()
    {
        if (!_discoveredFiles.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No files discovered yet.[/]\n");
            return;
        }

        var categories = await _categoryService.GetCategoriesAsync();
        if (!categories.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No categories available. Please create categories first.[/]\n");
            return;
        }

        AnsiConsole.MarkupLine("[bold]File Classification[/]\n");
        AnsiConsole.MarkupLine("[dim]Select files to classify (Enter index number, comma-separated for multiple)[/]");

        var unclassifiedFiles = _discoveredFiles.Where(f => !f.IsClassified).ToList();
        if (!unclassifiedFiles.Any())
        {
            AnsiConsole.MarkupLine("[yellow]All files are already classified.[/]\n");
            return;
        }

        // Show unclassified files
        var table = new Table();
        table.AddColumn("[bold]#[/]");
        table.AddColumn("[bold]File Name[/]");
        table.AddColumn("[bold]Size[/]");

        for (int i = 0; i < unclassifiedFiles.Count; i++)
        {
            table.AddRow((i + 1).ToString(), unclassifiedFiles[i].FileName, unclassifiedFiles[i].GetFormattedSize());
        }

        AnsiConsole.Write(table);
        Console.WriteLine();

        AnsiConsole.MarkupLine("[yellow]Enter file indices (e.g., 1,2,3 or range 1-5):[/]");
        var input = Console.ReadLine();

        var selectedIndices = ParseFileSelection(input, unclassifiedFiles.Count);
        if (!selectedIndices.Any())
        {
            AnsiConsole.MarkupLine("[red]Invalid selection.[/]\n");
            return;
        }

        var selectedFiles = selectedIndices.Select(i => unclassifiedFiles[i]).ToList();

        var categoryName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select category to assign")
                .AddChoices(categories.Select(c => c.Name)));

        foreach (var file in selectedFiles)
        {
            file.IsClassified = true;
            file.AssignedCategory = categoryName;
        }

        AnsiConsole.MarkupLine($"[green]✓ {selectedFiles.Count} file(s) classified to '{categoryName}'[/]\n");
    }

    private async Task CopyClassifiedFilesAsync()
    {
        if (string.IsNullOrEmpty(_outputFolder))
        {
            AnsiConsole.MarkupLine("[red]Output folder not set. Please select an output folder first.[/]\n");
            return;
        }

        var classifiedFiles = _discoveredFiles.Where(f => f.IsClassified).ToList();
        if (!classifiedFiles.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No classified files to copy.[/]\n");
            return;
        }

        AnsiConsole.MarkupLine($"[bold]Copying {classifiedFiles.Count} classified files to {_outputFolder}[/]\n");

        var progress = new Progress<(int Current, int Total, string Message)>(report =>
        {
            var escaped = Markup.Escape($"[{report.Current}/{report.Total}] {report.Message}");
            AnsiConsole.MarkupLine($"[dim]{escaped}[/]");
        });

        var result = await _fileService.CopyClassifiedFilesAsync(classifiedFiles, _outputFolder, progress);

        if (result.Success)
        {
            AnsiConsole.MarkupLine($"[green]✓ {result.Message}[/]\n");

            // Remove copied files from the list
            var copiedFiles = classifiedFiles.Where(f => f.IsClassified).ToList();
            foreach (var file in copiedFiles)
            {
                _discoveredFiles.Remove(file);
            }

            AnsiConsole.MarkupLine($"[dim]Remaining files in list: {_discoveredFiles.Count}[/]\n");
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]✗ Error: {result.Message}[/]\n");
            foreach (var error in result.Errors)
            {
                AnsiConsole.MarkupLine($"[red]  - {error}[/]");
            }
            Console.WriteLine();
        }
    }

    private List<int> ParseFileSelection(string? input, int maxCount)
    {
        var indices = new List<int>();

        if (string.IsNullOrWhiteSpace(input))
            return indices;

        try
        {
            var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmed = part.Trim();

                // Check if it's a range (e.g., "1-5")
                if (trimmed.Contains('-'))
                {
                    var rangeParts = trimmed.Split('-');
                    if (rangeParts.Length == 2 && 
                        int.TryParse(rangeParts[0].Trim(), out int start) &&
                        int.TryParse(rangeParts[1].Trim(), out int end))
                    {
                        for (int i = start - 1; i < end && i < maxCount; i++)
                        {
                            if (i >= 0 && !indices.Contains(i))
                                indices.Add(i);
                        }
                    }
                }
                else if (int.TryParse(trimmed, out int index))
                {
                    index--; // Convert to 0-based index
                    if (index >= 0 && index < maxCount && !indices.Contains(index))
                        indices.Add(index);
                }
            }
        }
        catch { }

        return indices;
    }
}
