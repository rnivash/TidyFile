# TidyFile Persistence Architecture

## Why Categories Persist But Paths Don't

### The Key Difference

**Categories** are persisted to disk (JSON file) and automatically loaded on restart.  
**Source/Output paths** are stored only in memory for the current session and are not persisted.

---

## 1. Categories Persistence (✅ Remembered)

### How It Works

#### 1a. Storage Location
```csharp
// CategoryService.cs
_configPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "TidyFile",
    "categories.json");
```

**On different operating systems:**
- **Linux**: `~/.config/TidyFile/categories.json`
- **macOS**: `~/Library/Application Support/TidyFile/categories.json`
- **Windows**: `%APPDATA%/TidyFile/categories.json`

#### 1b. Automatic Loading on Startup
```csharp
// Program.cs
var ui = serviceProvider.GetRequiredService<ApplicationUI>();
await ui.RunAsync();

// ApplicationUI.cs - In RunAsync()
await _categoryService.LoadCategoriesAsync();  // ← Loads from JSON file
```

#### 1c. JSON File Structure
```json
[
  {
    "Name": "Resume",
    "Description": "Resume and cover letters",
    "CreatedAt": "2025-11-15T12:30:00"
  },
  {
    "Name": "IDProof",
    "Description": "Identity documents",
    "CreatedAt": "2025-11-15T12:31:00"
  },
  {
    "Name": "Photos",
    "Description": "Personal photographs",
    "CreatedAt": "2025-11-15T12:32:00"
  }
]
```

#### 1d. Save Mechanism
When user creates/modifies/deletes a category:
```csharp
// CategoryService.cs
public async Task SaveCategoriesAsync()
{
    var json = JsonSerializer.Serialize(_categories, 
        new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(_configPath, json);
}

// ApplicationUI.cs - After category operation
await _categoryService.SaveCategoriesAsync();  // ← Persists to disk
```

---

## 2. Source & Output Paths (❌ Not Remembered)

### How They Work (In-Memory Only)

#### 2a. Storage Location
```csharp
// ApplicationUI.cs - Class-level fields
private List<string> _sourceFolders = new();      // In memory only
private string _outputFolder = string.Empty;      // In memory only
```

**These are initialized as empty on every restart!**

#### 2b. Lifespan
- Created when application starts
- Populated when user selects folders via UI
- **Lost when application closes** (no persistence)
- Reset to empty on next restart

#### 2c. User Flow
```
Session 1:
├─ Start app
├─ _sourceFolders = []           (empty)
├─ Select Source Folders
│  └─ _sourceFolders = ["/home/user/Downloads", "/home/user/Documents"]
├─ Select Output Folder
│  └─ _outputFolder = "/home/user/Organized"
└─ Close app
   └─ _sourceFolders and _outputFolder are discarded

Session 2:
├─ Start app
├─ _sourceFolders = []           (empty again!)
├─ _outputFolder = ""            (empty again!)
└─ User must re-select folders
```

---

## 3. Design Rationale

### Why Persist Categories?
1. **Categories are static configuration** - Users create them once, use them repeatedly
2. **User effort** - Categories are valuable metadata that users invested time creating
3. **Functional requirement** - Requirement: "Create custom categories... Display all created categories"
4. **Reasonable default** - Users expect their categories to persist across sessions

### Why NOT Persist Paths?
1. **Transient/Workflow data** - Paths change per use case
2. **Session-specific** - Each file organization task may use different folders
3. **No explicit requirement** - The prompt says "Copy files to output folder" but doesn't require persistence
4. **Safety by design** - Don't want to accidentally copy to outdated folders from previous session
5. **User intent** - Explicit selection ensures user is working with correct folders

---

## 4. Current Implementation Summary

| Aspect | Categories | Source/Output Paths |
|--------|-----------|-------------------|
| **Storage** | JSON file on disk | In-memory only |
| **Location** | `~/.config/TidyFile/categories.json` | RAM (lost on exit) |
| **Lifecycle** | Load on startup → Persist to disk | Created on startup → Discarded on exit |
| **Save Trigger** | After CRUD operation | Never saved |
| **User Experience** | "My categories are saved!" | "Where were those folders again?" |

---

## 5. How Categories Flow Through the App

```
Program.cs
    ↓
ApplicationUI.RunAsync()
    ├─ await categoryService.LoadCategoriesAsync()  ← Load from disk
    │
    └─ Main Loop
        ├─ User: "Manage Categories"
        │  ├─ Create/Rename/Delete operations
        │  └─ await categoryService.SaveCategoriesAsync()  ← Save to disk
        │
        ├─ User: "Classify Files"
        │  └─ Assign files to persisted categories
        │
        └─ User: "Copy Classified Files"
           └─ Copy to output/category/ structure
```

---

## 6. How Paths Flow Through the App

```
Program.cs
    ↓
ApplicationUI.RunAsync()
    ├─ _sourceFolders = new()         ← Empty list created
    ├─ _outputFolder = ""             ← Empty string created
    │
    └─ Main Loop
        ├─ User: "Select Source Folders"
        │  └─ Adds to _sourceFolders (memory only)
        │
        ├─ User: "Select Output Folder"
        │  └─ Sets _outputFolder (memory only)
        │
        ├─ User: "Discover Files"
        │  └─ Uses _sourceFolders to scan
        │
        └─ Application closes
           └─ _sourceFolders and _outputFolder are garbage collected
              (they were never saved to disk)
```

---

## 7. Code Evidence

### Categories ARE Persisted
```csharp
// CategoryService.cs - Line 78-89
public async Task SaveCategoriesAsync()
{
    var json = JsonSerializer.Serialize(_categories, 
        new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(_configPath, json);  // ← WRITES TO DISK
}

public async Task LoadCategoriesAsync()
{
    if (File.Exists(_configPath))
    {
        var json = File.ReadAllText(_configPath);  // ← READS FROM DISK
        _categories = JsonSerializer.Deserialize<List<Category>>(json);
    }
}
```

### Paths are NOT Persisted
```csharp
// ApplicationUI.cs - Line 15-17
private List<string> _sourceFolders = new();      // ← In-memory only
private string _outputFolder = string.Empty;      // ← In-memory only
// No LoadPathsAsync() method
// No SavePathsAsync() method
// No persistence mechanism
```

---

## 8. To Add Path Persistence (If Needed)

If you wanted source/output paths to persist across sessions, you would:

1. Create a "PathsService" similar to CategoryService
2. Add JSON persistence for paths configuration
3. Load paths on startup: `await pathsService.LoadPathsAsync()`
4. Save paths after user selection: `await pathsService.SavePathsAsync()`

Example structure:
```json
{
  "SourceFolders": [
    "/home/user/Downloads",
    "/home/user/Documents"
  ],
  "OutputFolder": "/home/user/Organized",
  "LastUsedAt": "2025-11-15T12:30:00"
}
```

---

## Summary

The TidyFile application uses a **selective persistence strategy**:

- ✅ **Categories**: Permanently saved to `~/.config/TidyFile/categories.json` using `CategoryService`
- ❌ **Paths**: Temporarily stored in memory (`_sourceFolders`, `_outputFolder`) with no persistence layer

This design balances:
- **User convenience** (categories persist)
- **Safety** (paths require explicit re-selection each session)
- **Simplicity** (no complex path validation/change detection)
- **Intent** (users consciously select folders for each task)

---
