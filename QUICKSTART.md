# Quick Start Guide - TidyFile

Get up and running with TidyFile in 5 minutes!

## Installation

### Prerequisites
- .NET 8.0 SDK or later
- Any OS (Linux, macOS, Windows)

### Clone and Build

```bash
# Clone the repository
git clone https://github.com/yourusername/TidyFile.git
cd TidyFile

# Build the project
dotnet build -c Release

# Run the application
./src/TidyFile/bin/Release/net8.0/TidyFile
```

Or run directly without building:
```bash
dotnet run --project src/TidyFile/TidyFile.csproj
```

## Your First Organization

### Step 1: Create Categories
1. Select **"Manage Categories"** from the main menu
2. Choose **"Create Category"**
3. Enter category name: `Resume`
4. Add description (optional): `Resume and CV files`
5. Repeat for other categories like `Photos`, `Documents`, etc.

### Step 2: Select Source Folders
1. Select **"Select Source Folders"** from the main menu
2. Enter a folder path: `/home/user/Documents`
3. Add more folders if needed
4. Choose **"Done"**

### Step 3: Choose Output Folder
1. Select **"Select Output Folder"**
2. Enter output path: `/home/user/Organized`
3. The folder will be created if it doesn't exist

### Step 4: Discover Files
1. Select **"Discover Files"**
2. Wait for the scan to complete
3. It will report how many files were found

### Step 5: Classify Files
1. Select **"Classify Files"**
2. View the list of files
3. Select files: `1,3,5` or use ranges `1-5`
4. Choose a category to assign
5. Repeat for different categories

### Step 6: Organize
1. Select **"Copy Classified Files"**
2. Watch the progress as files are copied
3. Original files remain untouched
4. Classified files are removed from the list

### Done!
Your files are now organized in `/home/user/Organized/`:
```
Organized/
├── Resume/
│   └── Files/
│       ├── resume.pdf
│       └── cv.docx
├── Photos/
│   └── Files/
│       ├── vacation.jpg
│       └── family.png
└── Documents/
    └── Files/
        └── contract.pdf
```

## File Selection Tips

When asked to select files, you can use:
- Single files: `1` (selects file #1)
- Multiple files: `1,3,5` (selects files 1, 3, and 5)
- Ranges: `1-5` (selects files 1 through 5)
- Mixed: `1-3,5,7-9` (combines ranges and singles)

## Keyboard Shortcuts in Menus

- **Arrow keys**: Navigate menu options
- **Enter**: Select an option
- **Ctrl+C**: Cancel current operation or exit

## Common Tasks

### View All Files
1. Select **"View Files"**
2. Use Previous/Next to navigate pages
3. Select "Back" to return

### Rename a Category
1. Select **"Manage Categories"**
2. Choose **"Rename Category"**
3. Select the category to rename
4. Enter new name

### Delete a Category
1. Select **"Manage Categories"**
2. Choose **"Delete Category"**
3. Select the category
4. Confirm deletion

## Troubleshooting

### Can't find my files?
- Make sure you selected source folders first
- Check that the folders exist and are readable
- Try discovering files again

### Categories not saving?
- Categories are auto-saved after each change
- They're stored in: `~/.config/TidyFile/categories.json`
- Make sure you have write permissions in `~/.config/`

### File not copied?
- Ensure the file is classified (assigned to a category)
- Check that output folder is selected and writable
- Look for error messages in the log file: `~/.config/TidyFile/logs/`

## Getting Help

- Check the full [README.md](README.md) for detailed documentation
- Run tests to verify installation: `dotnet test`
- View application logs: `~/.config/TidyFile/logs/`

## Next Steps

- Explore the **"Manage Categories"** menu to create more categories
- Try different file selections and bulk operations
- Organize files from multiple source folders
- Check the logs to understand what the app is doing

---

**Enjoy organizing your files! 🎉**
