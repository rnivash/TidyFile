namespace TidyFile.Services;

using System.Text.Json;
using TidyFile.Interfaces;
using TidyFile.Models;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for category management and persistence.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ILogger<CategoryService> _logger;
    private readonly string _configPath;
    private List<Category> _categories = new();

    public CategoryService(ILogger<CategoryService> logger, string? configPath = null)
    {
        _logger = logger;
        _configPath = configPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TidyFile",
            "categories.json");
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await Task.FromResult(new List<Category>(_categories));
    }

    public async Task<bool> CreateCategoryAsync(string name, string description = "")
    {
        return await Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Cannot create category with empty name");
                return false;
            }

            if (_categories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Category already exists: {CategoryName}", name);
                return false;
            }

            var category = new Category(name, description);
            _categories.Add(category);
            _logger.LogInformation("Category created: {CategoryName}", name);
            return true;
        });
    }

    public async Task<bool> DeleteCategoryAsync(string name)
    {
        return await Task.Run(() =>
        {
            var category = _categories.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (category == null)
            {
                _logger.LogWarning("Category not found: {CategoryName}", name);
                return false;
            }

            _categories.Remove(category);
            _logger.LogInformation("Category deleted: {CategoryName}", name);
            return true;
        });
    }

    public async Task<bool> RenameCategoryAsync(string oldName, string newName)
    {
        return await Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                _logger.LogWarning("Cannot rename category to empty name");
                return false;
            }

            var category = _categories.FirstOrDefault(c => c.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase));
            if (category == null)
            {
                _logger.LogWarning("Category not found: {CategoryName}", oldName);
                return false;
            }

            if (_categories.Any(c => c.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Category with new name already exists: {CategoryName}", newName);
                return false;
            }

            category.Name = newName;
            _logger.LogInformation("Category renamed from {OldName} to {NewName}", oldName, newName);
            return true;
        });
    }

    public async Task SaveCategoriesAsync()
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

                var json = JsonSerializer.Serialize(_categories, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
                _logger.LogInformation("Categories saved to {ConfigPath}", _configPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving categories to {ConfigPath}", _configPath);
                throw;
            }
        });
    }

    public async Task LoadCategoriesAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    var loadedCategories = JsonSerializer.Deserialize<List<Category>>(json);
                    _categories = loadedCategories ?? new List<Category>();
                    _logger.LogInformation("Categories loaded from {ConfigPath}. Count: {Count}", _configPath, _categories.Count);
                }
                else
                {
                    _categories = new List<Category>();
                    _logger.LogInformation("No existing categories file found at {ConfigPath}", _configPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories from {ConfigPath}", _configPath);
                _categories = new List<Category>();
            }
        });
    }
}
