using Xunit;
using TidyFile.Models;

namespace TidyFile.Tests;

public class FileItemTests
{
    [Fact]
    public void FileItem_GetFormattedSize_ReturnsCorrectFormat()
    {
        // Arrange
        var fileItem = new FileItem { SizeBytes = 1024 };

        // Act
        var result = fileItem.GetFormattedSize();

        // Assert
        Assert.Equal("1 KB", result);
    }

    [Theory]
    [InlineData(512, "512 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1048576, "1 MB")]
    [InlineData(1073741824, "1 GB")]
    [InlineData(1536, "1.50 KB")]
    public void FileItem_GetFormattedSize_HandlesVariousSizes(long bytes, string expected)
    {
        // Arrange
        var fileItem = new FileItem { SizeBytes = bytes };

        // Act
        var result = fileItem.GetFormattedSize();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FileItem_ToString_ContainsFileName()
    {
        // Arrange
        var fileItem = new FileItem
        {
            FileName = "test.txt",
            SizeBytes = 1024,
            AssignedCategory = "Documents"
        };

        // Act
        var result = fileItem.ToString();

        // Assert
        Assert.Contains("test.txt", result);
        Assert.Contains("Documents", result);
    }

    [Fact]
    public void FileItem_IsClassified_DefaultsFalse()
    {
        // Arrange & Act
        var fileItem = new FileItem();

        // Assert
        Assert.False(fileItem.IsClassified);
    }
}
