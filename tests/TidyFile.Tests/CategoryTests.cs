using Xunit;
using TidyFile.Models;

namespace TidyFile.Tests;

public class CategoryTests
{
    [Fact]
    public void Category_Constructor_InitializesProperties()
    {
        // Act
        var category = new Category("Documents", "Important documents");

        // Assert
        Assert.Equal("Documents", category.Name);
        Assert.Equal("Important documents", category.Description);
        Assert.NotEqual(default(DateTime), category.CreatedAt);
    }

    [Fact]
    public void Category_DefaultConstructor_CreatesEmptyCategory()
    {
        // Act
        var category = new Category();

        // Assert
        Assert.Empty(category.Name);
        Assert.Empty(category.Description);
    }

    [Fact]
    public void Category_Equals_ReturnsTrueForSameName()
    {
        // Arrange
        var category1 = new Category { Name = "Documents" };
        var category2 = new Category { Name = "Documents" };

        // Act & Assert
        Assert.Equal(category1, category2);
    }

    [Fact]
    public void Category_Equals_ReturnsFalseForDifferentNames()
    {
        // Arrange
        var category1 = new Category { Name = "Documents" };
        var category2 = new Category { Name = "Photos" };

        // Act & Assert
        Assert.NotEqual(category1, category2);
    }

    [Fact]
    public void Category_GetHashCode_SameForSameName()
    {
        // Arrange
        var category1 = new Category { Name = "Documents" };
        var category2 = new Category { Name = "Documents" };

        // Act & Assert
        Assert.Equal(category1.GetHashCode(), category2.GetHashCode());
    }

    [Fact]
    public void Category_ToString_ContainsNameAndDescription()
    {
        // Arrange
        var category = new Category("Documents", "Important docs");

        // Act
        var result = category.ToString();

        // Assert
        Assert.Contains("Documents", result);
        Assert.Contains("Important docs", result);
    }
}
