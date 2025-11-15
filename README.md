# TidyFile - File Organization and Classification System

A powerful .NET 8 console application for organizing and classifying files from multiple source folders into custom categories. Features an interactive menu-driven UI with async operations, comprehensive logging, and support for batch processing.

## Features

✨ **Core Features**:
- 📁 Scan multiple source folders for files (recursive discovery)
- 📊 Display files in flat hierarchy with metadata (name, size, dates)
- 🏷️ Create and manage custom categories
- 📋 Assign files to categories (single or bulk operations)
- 📤 Copy classified files to output folder maintaining category structure
- 🔄 Preserve original files (no modifications to source)
- ✅ Real-time file list updates after classification
- 📈 Progress indicators during copy operations
- 💾 Category persistence to JSON
- � Automatic tracking of copied files (prevents re-processing)
- 🔄 Folder path persistence across sessions
- �📝 Comprehensive logging with Serilog

✨ **UI Features**:
- 🎨 Interactive menu-driven console experience with Spectre.Console
- ⌨️ Table-based file display with paging
- 🎯 Numbered file selection with range support (e.g., "1-5, 7, 9")
- 🚀 Responsive async operations (non-blocking UI)
- ✔️ Confirmation dialogs for important operations

## Prerequisites

- .NET 8.0 SDK or later
- Linux, macOS, or Windows (cross-platform)

## Installation

### From Source

```bash
# Clone the repository
git clone https://github.com/yourusername/TidyFile.git
cd TidyFile

# Build the project
dotnet build

# Build in release mode
dotnet build -c Release
```

### Running the Application

```bash
# Run in interactive mode (default)
dotnet run --project src/TidyFile/TidyFile.csproj

# Or from the built executable (Release)
./src/TidyFile/bin/Release/net8.0/TidyFile
```

## Usage

### Interactive Mode (Default)

1. **Start the application**:
   ```bash
   dotnet run --project src/TidyFile/TidyFile.csproj
   ```

2. **Main Menu Options**:
   - `Select Source Folders` - Add one or more folders to scan
   - `Select Output Folder` - Set where classified files will be copied
   - `Discover Files` - Scan selected folders for files
   - `View Files` - Browse discovered files with pagination
   - `Manage Categories` - Create, rename, or delete categories
   - `Classify Files` - Assign files to categories
   - `Copy Classified Files` - Copy classified files to output folder
   - `Exit` - Close the application

### Step-by-Step Example

```
1. Choose "Select Source Folders"
   → Enter folder path: /home/user/Documents
   → Enter folder path: /home/user/Downloads
   → Choose "Done"

2. Choose "Select Output Folder"
   → Enter output folder path: /home/user/Organized
   
3. Choose "Discover Files"
   → Application scans all source folders
   → Reports: "Discovered 150 files"

4. Choose "Manage Categories"
   → Create Category: "Resume"
   → Create Category: "IDProof"
   → Create Category: "Photos"
   → Choose "Back"

5. Choose "View Files"
   → Browse through paginated list
   → See file names, sizes, modified dates
   → Choose "Back"

6. Choose "Classify Files"
   → Select files: 1,3,5,7
   → Assign to category: "Resume"
   → Classify more files: 10-15
   → Assign to category: "Photos"

7. Choose "Copy Classified Files"
   → Application copies files to:
      - /home/user/Organized/Resume/Files/
      - /home/user/Organized/Photos/Files/
   → Classified files removed from display
   → Success report: "Copied 19 files"
```

### File Selection Syntax

When classifying files, use intuitive selection syntax:

```
1,2,3         # Select files 1, 2, and 3
1-5           # Select files 1 through 5
1,3-5,7,9-10  # Mixed selection
```

### Output Folder Structure

```
OutputFolder/
├── Resume/
│   └── Files/
│       ├── resume_john.pdf
│       ├── cv_updated.docx
│       └── ...
├── IDProof/
│   └── Files/
│       ├── passport.jpg
│       ├── driver_license.pdf
│       └── ...
└── Photos/
    └── Files/
        ├── vacation_photo1.jpg
        ├── family_pic.png
        └── ...
```

## Configuration

### Category Storage

Categories are stored in JSON format at:
```
~/.config/TidyFile/categories.json
```

Example:
```json
[
   {
      "Name": "Resume",
      "Description": "Resume and CV files",
      "CreatedAt": "2025-11-15T10:30:00"
   },
   {
      "Name": "IDProof",
      "Description": "Identity and legal documents",
      "CreatedAt": "2025-11-15T10:32:00"
   }
]
```

### Source/Output Folder Path Persistence

Source folders and output folder are automatically saved and loaded at startup, similar to categories. These paths are stored in:
```
~/.config/TidyFile/appconfig.json
```

Example:
```json
{
   "SourceFolders": [
      "/home/user/Documents",
      "/home/user/Downloads"
   ],
   "OutputFolder": "/home/user/Organized"
}
```

You can change these paths via the interactive menu. Changes are persisted automatically and restored on next launch.

### Copied Files Tracking

Once files are copied to the output folder, they are automatically tracked and excluded from future file discoveries. This prevents already-processed files from reappearing in the file list after app restarts.

Tracking data is stored in:
```
~/.config/TidyFile/copiedfiles.json
```

Example:
```json
[
  {
    "SourceFilePath": "/home/user/Documents/resume.pdf",
    "DestinationFilePath": "/home/user/Organized/Resume/resume.pdf",
    "Category": "Resume",
    "CopiedAt": "2025-11-15T10:45:00Z"
  },
  {
    "SourceFilePath": "/home/user/Downloads/photo.jpg",
    "DestinationFilePath": "/home/user/Organized/Photos/photo.jpg",
    "Category": "Photos",
    "CopiedAt": "2025-11-15T10:46:00Z"
  }
]
```

**Benefits**:
- Files are never re-processed in subsequent sessions
- Clean file list showing only remaining files
- Automatic exclusion based on source file path
- Full audit trail of copied files

### Logs

Logs are saved to:
```
~/.config/TidyFile/logs/tidyfile-YYYY-MM-DD.txt
```

Log levels: Information, Warning, Error

## Project Structure

```
TidyFile/
├── src/
│   └── TidyFile/
│       ├── Models/           # Domain models (FileItem, Category, ClassificationResult)
│       ├── Services/         # Business logic (FileService, CategoryService)
│       ├── Interfaces/       # Service interfaces
│       ├── UI/               # Console UI (ApplicationUI)
│       ├── Utilities/        # Helper utilities
│       └── Program.cs        # Application entry point
├── tests/
│   └── TidyFile.Tests/       # xUnit tests
├── docs/                     # Documentation
├── README.md                 # This file
├── .gitignore
└── TidyFile.sln              # Solution file
```

## Technology Stack

**Framework**: .NET 8 with C#

**Key Dependencies**:
- **Spectre.Console** - Rich console UI (tables, progress bars, prompts)
- **System.CommandLine** - Command-line argument parsing
- **Serilog** - Structured logging
- **Microsoft.Extensions.Logging** - Logging abstractions
- **Microsoft.Extensions.DependencyInjection** - Dependency injection
- **xUnit** - Unit testing framework

## API Reference

### FileService

```csharp
// Discover files from source folders
Task<List<FileItem>> DiscoverFilesAsync(List<string> sourceFolders);

// Copy classified files to output folder
Task<ClassificationResult> CopyClassifiedFilesAsync(
    List<FileItem> files,
    string outputFolder,
    IProgress<(int Current, int Total, string Message)> progress);

// Open file with default program
Task OpenFileAsync(string filePath);

// Preview first N lines of text file
Task<string> PreviewTextFileAsync(string filePath, int lineCount = 20);
```

### CategoryService

```csharp
// Get all categories
Task<List<Category>> GetCategoriesAsync();

// Create new category
Task<bool> CreateCategoryAsync(string name, string description = "");

// Delete category
Task<bool> DeleteCategoryAsync(string name);

// Rename category
Task<bool> RenameCategoryAsync(string oldName, string newName);

// Save/Load categories
Task SaveCategoriesAsync();
Task LoadCategoriesAsync();
```

## Testing

Run unit tests:

```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter NamespaceName.TestClassName
```

## Advanced Usage

### Batch Mode (Future Implementation)

Planned batch mode will support JSON/CSV file input for automated classification without interactive prompts.

Example batch file (`classifications.json`):
```json
[
  {
    "filePath": "/home/user/documents/resume.pdf",
    "category": "Resume"
  },
  {
    "filePath": "/home/user/documents/passport.jpg",
    "category": "IDProof"
  }
]
```

## Troubleshooting

### Issue: Application fails to discover files

**Solution**:
- Ensure folder paths are correctly entered
- Check folder permissions (read access required)
- Verify folders exist and contain files

### Issue: Category file not found

**Solution**:
- Categories are auto-created on first run at `~/.config/TidyFile/categories.json`
- Ensure write permissions for the config directory
- Delete the file to reset categories

### Issue: Files not copied

**Solution**:
- Ensure output folder is selected
- Verify files are classified (marked with category)
- Check write permissions on output folder
- Check available disk space

## Development

### Build and Run

```bash
# Debug build and run
dotnet build
dotnet run --project src/TidyFile/TidyFile.csproj

# Release build
dotnet build -c Release
```

### Code Organization

- **Models**: Immutable data structures
- **Services**: Business logic with async operations
- **Interfaces**: Abstraction for dependency injection
- **UI**: Console-based user interaction
- **Tests**: Comprehensive xUnit test coverage

### Adding New Features

1. Create models in `Models/`
2. Define service interface in `Interfaces/`
3. Implement service in `Services/`
4. Add UI commands in `UI/ApplicationUI.cs`
5. Write tests in `tests/TidyFile.Tests/`
6. Update documentation

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests
5. Submit a pull request

## Known Limitations

- Batch mode not yet implemented
- File preview limited to first 20 lines for text files
- Maximum filename length depends on OS (typically 255 characters)
- Very large file collections (100,000+) may require pagination optimization

## Future Roadmap

- [ ] Batch mode with JSON/CSV input
- [ ] Automatic file type categorization
- [ ] Duplicate file detection
- [ ] Search and filter within file list
- [ ] Undo classification operations
- [ ] Config file for default folders
- [ ] Web UI alternative
- [ ] Docker containerization

## License

MIT License - See LICENSE file for details

## Support

For issues, questions, or suggestions:
- Open an issue on GitHub
- Check existing documentation
- Review logs in `~/.config/TidyFile/logs/`

## Changelog

### Version 1.0.0 (Initial Release)
- Interactive menu-driven console application
- File discovery and metadata display
- Category management
- File classification and organization
- Async operations with progress reporting
- Comprehensive logging
- Cross-platform support (Linux, macOS, Windows)

---

**Built with ❤️ using .NET 8**
