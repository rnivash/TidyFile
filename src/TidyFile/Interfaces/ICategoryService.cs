namespace TidyFile.Interfaces;

using TidyFile.Models;

/// <summary>
/// Interface for category management and persistence.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets all categories.
    /// </summary>
    Task<List<Category>> GetCategoriesAsync();

    /// <summary>
    /// Creates a new category.
    /// </summary>
    Task<bool> CreateCategoryAsync(string name, string description = "");

    /// <summary>
    /// Deletes a category by name.
    /// </summary>
    Task<bool> DeleteCategoryAsync(string name);

    /// <summary>
    /// Renames an existing category.
    /// </summary>
    Task<bool> RenameCategoryAsync(string oldName, string newName);

    /// <summary>
    /// Saves categories to persistence layer.
    /// </summary>
    Task SaveCategoriesAsync();

    /// <summary>
    /// Loads categories from persistence layer.
    /// </summary>
    Task LoadCategoriesAsync();
}
