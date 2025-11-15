using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TidyFile.Services;
using TidyFile.Models;

namespace TidyFile.Tests;

public class FileServiceTests
{
    private readonly Mock<ILogger<FileService>> _mockLogger;
    private readonly Mock<ILogger<CopiedFilesTrackerService>> _mockTrackerLogger;

    public FileServiceTests()
    {
        _mockLogger = new Mock<ILogger<FileService>>();
        _mockTrackerLogger = new Mock<ILogger<CopiedFilesTrackerService>>();
    }

    private CopiedFilesTrackerService CreateTrackerService()
    {
        return new CopiedFilesTrackerService(_mockTrackerLogger.Object);
    }

    [Fact]
    public async Task DiscoverFilesAsync_WithValidFolder_ReturnsFileList()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        // Create test files
        File.WriteAllText(Path.Combine(tempDir, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(tempDir, "file2.txt"), "content2");

        var trackerService = CreateTrackerService();
        var service = new FileService(_mockLogger.Object, trackerService);

        try
        {
            // Act
            var files = await service.DiscoverFilesAsync(new List<string> { tempDir });

            // Assert
            Assert.Equal(2, files.Count);
            Assert.Contains(files, f => f.FileName == "file1.txt");
            Assert.Contains(files, f => f.FileName == "file2.txt");
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task DiscoverFilesAsync_WithNonExistentFolder_ReturnsEmptyList()
    {
        // Arrange
        var nonExistentFolder = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}");
        var trackerService = CreateTrackerService();
        var service = new FileService(_mockLogger.Object, trackerService);

        // Act
        var files = await service.DiscoverFilesAsync(new List<string> { nonExistentFolder });

        // Assert
        Assert.Empty(files);
    }

    [Fact]
    public async Task DiscoverFilesAsync_WithMultipleFolders_ReturnsFilesFromAll()
    {
        // Arrange
        var tempDir1 = Path.Combine(Path.GetTempPath(), $"test1_{Guid.NewGuid()}");
        var tempDir2 = Path.Combine(Path.GetTempPath(), $"test2_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir1);
        Directory.CreateDirectory(tempDir2);

        File.WriteAllText(Path.Combine(tempDir1, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(tempDir2, "file2.txt"), "content2");

        var trackerService = CreateTrackerService();
        var service = new FileService(_mockLogger.Object, trackerService);

        try
        {
            // Act
            var files = await service.DiscoverFilesAsync(new List<string> { tempDir1, tempDir2 });

            // Assert
            Assert.Equal(2, files.Count);
        }
        finally
        {
            Directory.Delete(tempDir1, true);
            Directory.Delete(tempDir2, true);
        }
    }

    [Fact]
    public async Task GetFileMetadataAsync_WithValidFile_ReturnsFileMetadata()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.txt");
        File.WriteAllText(tempFile, "test content");

        var trackerService = CreateTrackerService();
        var service = new FileService(_mockLogger.Object, trackerService);

        try
        {
            // Act
            var metadata = await service.GetFileMetadataAsync(tempFile);

            // Assert
            Assert.NotNull(metadata);
            Assert.Equal(Path.GetFileName(tempFile), metadata.FileName);
            Assert.True(metadata.SizeBytes > 0);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetFileMetadataAsync_WithNonExistentFile_ReturnsNull()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.txt");
        var trackerService = CreateTrackerService();
        var service = new FileService(_mockLogger.Object, trackerService);

        // Act
        var metadata = await service.GetFileMetadataAsync(nonExistentFile);

        // Assert
        Assert.Null(metadata);
    }

    [Fact]
    public async Task CopyClassifiedFilesAsync_WithClassifiedFiles_CopiesFilesToOutputFolder()
    {
        // Arrange
        var sourceDir = Path.Combine(Path.GetTempPath(), $"source_{Guid.NewGuid()}");
        var outputDir = Path.Combine(Path.GetTempPath(), $"output_{Guid.NewGuid()}");
        Directory.CreateDirectory(sourceDir);
        Directory.CreateDirectory(outputDir);

        var testFile = Path.Combine(sourceDir, "test.txt");
        File.WriteAllText(testFile, "test content");

        var fileItem = new FileItem
        {
            FilePath = testFile,
            FileName = "test.txt",
            SizeBytes = 12,
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now,
            IsClassified = true,
            AssignedCategory = "TestCategory"
        };

        var trackerService = CreateTrackerService();
        var service = new FileService(_mockLogger.Object, trackerService);
        var progress = new Progress<(int, int, string)>();

        try
        {
            // Act
            var result = await service.CopyClassifiedFilesAsync(
                new List<FileItem> { fileItem },
                outputDir,
                progress);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.FilesCopied);
            var copiedFile = Path.Combine(outputDir, "TestCategory", "test.txt");
            Assert.True(File.Exists(copiedFile));
        }
        finally
        {
            Directory.Delete(sourceDir, true);
            Directory.Delete(outputDir, true);
        }
    }

    [Fact]
    public async Task CopyClassifiedFilesAsync_WithNoClassifiedFiles_ReturnsFalse()
    {
        // Arrange
        var outputDir = Path.Combine(Path.GetTempPath(), $"output_{Guid.NewGuid()}");
        Directory.CreateDirectory(outputDir);

        var trackerService = CreateTrackerService();
        var service = new FileService(_mockLogger.Object, trackerService);
        var progress = new Progress<(int, int, string)>();

        var unclassifiedFile = new FileItem
        {
            FilePath = "dummy.txt",
            IsClassified = false
        };

        try
        {
            // Act
            var result = await service.CopyClassifiedFilesAsync(
                new List<FileItem> { unclassifiedFile },
                outputDir,
                progress);

            // Assert
            Assert.False(result.Success);
        }
        finally
        {
            Directory.Delete(outputDir, true);
        }
    }

    [Fact]
    public async Task CopyClassifiedFilesAsync_WithFileConflict_HandlesRenaming()
    {
        // Arrange
        var sourceDir = Path.Combine(Path.GetTempPath(), $"source_{Guid.NewGuid()}");
        var outputDir = Path.Combine(Path.GetTempPath(), $"output_{Guid.NewGuid()}");
        Directory.CreateDirectory(sourceDir);
        Directory.CreateDirectory(outputDir);

        var testFile1 = Path.Combine(sourceDir, "test.txt");
        var testFile2 = Path.Combine(sourceDir, "test_copy.txt");
        File.WriteAllText(testFile1, "content1");
        File.WriteAllText(testFile2, "content2");

        // Pre-create conflicting file
        var categoryFolder = Path.Combine(outputDir, "TestCategory");
        Directory.CreateDirectory(categoryFolder);
        File.WriteAllText(Path.Combine(categoryFolder, "test.txt"), "existing content");

        var fileItem = new FileItem
        {
            FilePath = testFile1,
            FileName = "test.txt",
            SizeBytes = 8,
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now,
            IsClassified = true,
            AssignedCategory = "TestCategory"
        };

        var trackerService = CreateTrackerService();
        var service = new FileService(_mockLogger.Object, trackerService);
        var progress = new Progress<(int, int, string)>();

        try
        {
            // Act
            var result = await service.CopyClassifiedFilesAsync(
                new List<FileItem> { fileItem },
                outputDir,
                progress);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.FilesCopied);
            // Check that renamed file exists
            var renamedFile = Path.Combine(categoryFolder, "test_1.txt");
            Assert.True(File.Exists(renamedFile));
        }
        finally
        {
            Directory.Delete(sourceDir, true);
            Directory.Delete(outputDir, true);
        }
    }
}
