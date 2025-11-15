using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TidyFile.Services;
using TidyFile.Models;

namespace TidyFile.Tests;

public class CategoryServiceTests
{
    private readonly Mock<ILogger<CategoryService>> _mockLogger;
    private readonly string _testCategoryPath;

    public CategoryServiceTests()
    {
        _mockLogger = new Mock<ILogger<CategoryService>>();
        _testCategoryPath = Path.Combine(Path.GetTempPath(), $"test_categories_{Guid.NewGuid()}.json");
    }

    [Fact]
    public async Task CreateCategoryAsync_WithValidName_ReturnsTrueAndAddsCategory()
    {
        // Arrange
        var service = new CategoryService(_mockLogger.Object, _testCategoryPath);
        await service.LoadCategoriesAsync();

        // Act
        var result = await service.CreateCategoryAsync("Documents", "Important docs");
        var categories = await service.GetCategoriesAsync();

        // Assert
        Assert.True(result);
        Assert.Single(categories);
        Assert.Equal("Documents", categories[0].Name);
    }

    [Fact]
    public async Task CreateCategoryAsync_WithEmptyName_ReturnsFalse()
    {
        // Arrange
        var service = new CategoryService(_mockLogger.Object, _testCategoryPath);
        await service.LoadCategoriesAsync();

        // Act
        var result = await service.CreateCategoryAsync("", "Description");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateCategoryAsync_WithDuplicateName_ReturnsFalse()
    {
        // Arrange
        var service = new CategoryService(_mockLogger.Object, _testCategoryPath);
        await service.LoadCategoriesAsync();
        await service.CreateCategoryAsync("Documents");

        // Act
        var result = await service.CreateCategoryAsync("Documents");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithValidName_ReturnsTrueAndRemovesCategory()
    {
        // Arrange
        var service = new CategoryService(_mockLogger.Object, _testCategoryPath);
        await service.LoadCategoriesAsync();
        await service.CreateCategoryAsync("Documents");

        // Act
        var result = await service.DeleteCategoryAsync("Documents");
        var categories = await service.GetCategoriesAsync();

        // Assert
        Assert.True(result);
        Assert.Empty(categories);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithNonExistentName_ReturnsFalse()
    {
        // Arrange
        var service = new CategoryService(_mockLogger.Object, _testCategoryPath);
        await service.LoadCategoriesAsync();

        // Act
        var result = await service.DeleteCategoryAsync("NonExistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RenameCategoryAsync_WithValidNewName_ReturnsTrueAndRenamesCategory()
    {
        // Arrange
        var service = new CategoryService(_mockLogger.Object, _testCategoryPath);
        await service.LoadCategoriesAsync();
        await service.CreateCategoryAsync("OldName");

        // Act
        var result = await service.RenameCategoryAsync("OldName", "NewName");
        var categories = await service.GetCategoriesAsync();

        // Assert
        Assert.True(result);
        Assert.Single(categories);
        Assert.Equal("NewName", categories[0].Name);
    }

    [Fact]
    public async Task SaveAndLoadCategoriesAsync_PersistsCategoriesToFile()
    {
        // Arrange
        var service1 = new CategoryService(_mockLogger.Object, _testCategoryPath);
        await service1.LoadCategoriesAsync();
        await service1.CreateCategoryAsync("Documents", "Doc files");
        await service1.CreateCategoryAsync("Photos", "Photo files");
        await service1.SaveCategoriesAsync();

        // Act
        var service2 = new CategoryService(_mockLogger.Object, _testCategoryPath);
        await service2.LoadCategoriesAsync();
        var categories = await service2.GetCategoriesAsync();

        // Assert
        Assert.Equal(2, categories.Count);
        Assert.Contains(categories, c => c.Name == "Documents");
        Assert.Contains(categories, c => c.Name == "Photos");

        // Cleanup
        if (File.Exists(_testCategoryPath))
            File.Delete(_testCategoryPath);
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsCopyOfList()
    {
        // Arrange
        var service = new CategoryService(_mockLogger.Object, _testCategoryPath);
        await service.LoadCategoriesAsync();
        await service.CreateCategoryAsync("Documents");

        // Act
        var categories1 = await service.GetCategoriesAsync();
        var categories2 = await service.GetCategoriesAsync();

        // Assert
        Assert.NotSame(categories1, categories2);
        Assert.Equal(categories1.Count, categories2.Count);
    }
}
