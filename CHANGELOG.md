# Changelog

## Version 1.1.0 - November 15, 2025

### Changed
- **Output Structure**: Removed the `Files` subfolder from the output directory structure
  - Old: `OutputFolder/CategoryName/Files/file.txt`
  - New: `OutputFolder/CategoryName/file.txt`
  - This provides a cleaner, flatter organization structure

### Modified Files
- `src/TidyFile/Services/FileService.cs` - Updated `CopyClassifiedFilesAsync()` to copy directly to category folder
- `tests/TidyFile.Tests/FileServiceTests.cs` - Updated test paths to reflect new structure
- `IMPLEMENTATION_SUMMARY.md` - Updated documentation to reflect new output structure

### Test Status
- ✅ All 30 unit tests passing
- ✅ Application builds successfully
- ✅ Interactive UI functional

## Version 1.0.0 - Initial Release

### Features
- ✅ Multi-folder file discovery with recursive scanning
- ✅ Custom category management with JSON persistence
- ✅ Interactive menu-driven console UI with Spectre.Console
- ✅ File classification and bulk operations
- ✅ Automatic file organization and copying
- ✅ Progress tracking and status reporting
- ✅ Comprehensive logging with Serilog
- ✅ Full unit test coverage (30 tests)
- ✅ Cross-platform .NET 8 support

### Technology Stack
- .NET 8.0 with C# 12
- Spectre.Console 0.54.0
- System.CommandLine 2.0.0
- Serilog 4.3.0
- xUnit 2.6.x + Moq 4.20.72

### Test Coverage
- FileItem: 6 tests
- Category: 6 tests  
- CategoryService: 8 tests
- FileService: 10 tests
- **Total: 30/30 passing**
