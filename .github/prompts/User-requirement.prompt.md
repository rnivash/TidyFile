---
mode: agent
---

# File Organization and Classification System

## Task Definition
Create a file organization and classification solution as a Console Application (.NET 8) that allows users to manually classify and categorize files from user-specified folders into custom categories and organize them into a dedicated output folder structure without modifying the original files. The application should be interactive (menu-driven or command-driven), cross-platform, and suitable for large file sets.

## Requirements

### Functional Requirements
1. **Folder Selection**: Allow users to select one or more source folders to scan for files
2. **File Discovery**: Scan selected folders to identify all files with a flat hierarchy display
3. **File Display & Metadata**: 
    - Display all discovered files in a flat list (non-hierarchical view)
    - Show file metadata (name, size, creation date, modification date, path)
    - Provide option to open individual files (invoke default program) when available from the console, and to preview text files in-console
4. **Category Management**:
    - Allow users to create custom categories (e.g., "Resume", "IDProof", "Photos", etc.)
    - Display all created categories in the application
5. **File Classification**:
    - Provide option to mark/classify files to custom categories
    - Support bulk classification of multiple files
6. **File Organization**:
    - Copy classified files to output folder maintaining category folder structure
    - Do NOT modify or delete files from original source location
    - Create output directory structure: `OutputFolder/CategoryName/Files`
7. **File List Management**:
    - Once files are copied to output folder, remove them from the file list display
    - Show progress of classification operation

### Console Application Requirements
1. **User Interface (Console/TUI)**: Provide an intuitive interactive console application (menu-driven or command-driven) with the following features:
    - Interactive source folder selection (browse by typing paths, or add multiple paths via commands)
    - Interactive output folder selection (set destination folder via command or prompt)
    - Flat file list presentation in the console with metadata columns (support paging and filtering)
    - Category management commands to create, list, rename, and delete categories
    - File actions available via keyboard commands or numbered selection (open, preview, assign category)
    - Category assignment commands that support single and bulk assignments
    - Progress indicators for file copying operations (console progress bars or status lines)
    - Automatic file list updates after successful classification and copy
2. **User Experience**:
    - Responsive and non-blocking console interactions: long-running I/O operations should run asynchronously and report progress
    - Clear confirmation prompts for destructive or important file operations (e.g., overwrites)
    - Error messages and status notifications printed to console and written to log
    - Real-time updates to file list as files are classified (reflect removal after copy)
    - Ability to select multiple files for batch classification using ranges, filters, or tags
    - Optional non-interactive mode: support command-line flags or a scriptable mode to run batch classification without interactive prompts

### Technology Stack
**Backend**:
- **Language**: .NET 8 with C#
- **File Operations**: `System.IO`, `System.IO.FileSystem`
- **Metadata Extraction**: File properties (name, size, creation/modification dates)
- **Logging**: `Serilog` or `Microsoft.Extensions.Logging`
- **Cross-platform**: Built-in cross-platform support

**Console UI / Frontend**:
- **Framework**: Console application (dotnet) using either built-in Console APIs or a rich console library such as `Spectre.Console` or `System.CommandLine` for commands and interactive prompts. The solution must remain .NET-only (C#) and cross-platform.
- **Threading**: `System.Threading.Tasks` for async operations and background workers
- **UI Pattern**: Command-handling and service separation (no MVVM required for console); keep code modular with interfaces for IO and file operations

**Additional Libraries**:
- `Spectre.Console` (optional, recommended) for richer console UI: tables, progress bars, prompts
- `System.CommandLine` (optional) to support a robust CLI and scriptable mode
- `xUnit` or `NUnit` for unit testing
- File system watcher (optional) for real-time updates when running in interactive mode

### Constraints
- Only scan and display files from user-selected source folders
- Do NOT modify, delete, or move files from original source location
- Copy files to output folder only after user classification
- Remove classified files from display list after successful copy
- Handle file name conflicts in output folder (e.g., rename if file exists)
- Preserve original file properties during copy operation
 - Console UI should remain responsive during file operations (use async/background tasks and progress reporting)
- Support multiple source folders selection
- Efficient handling of large file collections

### Success Criteria
- User can select one or more source folders to organize
- All files from selected folders are displayed in flat hierarchy with metadata
- User can create custom categories as needed
- User can classify files to custom categories
- Classified files are successfully copied to output folder with category structure
- Original files remain intact in source location
- Classified files are removed from display list after copying
- File list updates automatically after classification
- Application provides clear status and progress feedback
- No data loss during classification and copy operation

Additional console-specific success criteria:
- Interactive flow is usable from a terminal (Linux, macOS, Windows)
- Long-running operations show progress and do not block the interactive prompt
- Command-line flags enable automated/scripted usage for unattended batch jobs

## Scope
- Custom user-defined categories (user creates categories as needed)
- Single or multiple source folder selection
- Configurable output folder path
- Console/TUI application built with .NET 8 (C#)
- Flat file list display with metadata
- Manual file classification workflow
- No automatic classification (user-driven categorization)

Optional next-step suggestions (not required but recommended):
- Provide a `--script` or `--batch` mode to accept a CSV/JSON list of file->category mappings and run non-interactively
- Add a small test harness and example scripts in the repository README to demonstrate interactive and non-interactive usage