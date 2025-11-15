# Quick Reference: Categories vs Paths Persistence

## The Simple Answer

| Question | Categories | Paths |
|----------|-----------|-------|
| Do they survive app restart? | ✅ YES | ❌ NO |
| Where are they stored? | Disk (`~/.config/TidyFile/categories.json`) | Memory only (RAM) |
| How long do they last? | Permanently (until deleted) | Only during current session |
| Is there a save mechanism? | ✅ YES - `SaveCategoriesAsync()` | ❌ NO - Not implemented |
| Is there a load mechanism? | ✅ YES - `LoadCategoriesAsync()` | ❌ NO - Not implemented |

---

## Code Comparison

### Categories - Persisted to Disk ✅

```csharp
// CategoryService.cs - Lines 16-22
private readonly string _configPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "TidyFile",
    "categories.json");  // ← Disk file path

// Loaded on startup
await _categoryService.LoadCategoriesAsync();  // Reads from disk
// File.ReadAllText(_configPath) ← Disk I/O

// Saved after changes
await _categoryService.SaveCategoriesAsync();  // Writes to disk
// File.WriteAllText(_configPath, json) ← Disk I/O
```

### Paths - In Memory Only ❌

```csharp
// ApplicationUI.cs - Lines 15-17
private List<string> _sourceFolders = new();      // RAM only
private string _outputFolder = string.Empty;      // RAM only

// No LoadPathsAsync() method
// No SavePathsAsync() method
// No persistence layer whatsoever

// On restart, these become empty again:
// _sourceFolders = new();           ← []
// _outputFolder = "";               ← ""
```

---

## Why This Design?

### Categories Persistence Makes Sense Because:
1. **Static Configuration** - Users create categories once, reuse them forever
2. **High Value** - Worth persisting since they don't change frequently
3. **No Safety Concerns** - Safe to load the same categories each time
4. **User Expectation** - "I created these categories, they should be saved"

### Paths Non-Persistence Makes Sense Because:
1. **Transient/Workflow Data** - Paths change per task
2. **Session-Specific** - Different files organized each time
3. **Safety First** - Don't auto-use old paths from previous session
4. **Explicit Intent** - User must consciously select folders each time
5. **Flexibility** - Supports ad-hoc organization tasks without config burden

---

## Example Workflows

### Session 1 - Categories Persist ✅
```
1. Start TidyFile
2. Manage Categories → Create "Resume", "Photos", "Documents"
3. Exit
   → Categories saved to ~/.config/TidyFile/categories.json

Session 2:
4. Start TidyFile
5. Manage Categories → "Resume" is still there! ✅
   → Loaded from ~/.config/TidyFile/categories.json
```

### Session 1 - Paths Don't Persist ❌
```
1. Start TidyFile
2. Select Source Folders → /home/user/Downloads, /home/user/Documents
3. Select Output Folder → /home/user/Organized
4. Exit
   → Paths are discarded (only in RAM, no save)

Session 2:
5. Start TidyFile
6. Select Source Folders → Empty list! ❌
   → Must re-select folders
   → This is by design for safety
```

---

## The Core Mechanism

### How Categories Remember (Disk I/O)
```
Startup:
  Program.cs 
    → ApplicationUI.RunAsync()
      → categoryService.LoadCategoriesAsync()
        → File.ReadAllText("~/.config/TidyFile/categories.json")
          → JsonSerializer.Deserialize<List<Category>>()
            → _categories = [...loaded categories...]

User Action:
  User creates new category
    → categoryService.CreateCategoryAsync("NewCategory")
      → _categories.Add(new Category(...))
      → categoryService.SaveCategoriesAsync()
        → JsonSerializer.Serialize(_categories)
          → File.WriteAllText("~/.config/TidyFile/categories.json", json)
            → Saved to disk! ✅

Next Startup:
  LoadCategoriesAsync() loads from disk
    → NewCategory is there! ✅
```

### How Paths Forget (In-Memory Only)
```
Startup:
  ApplicationUI constructor
    → private List<string> _sourceFolders = new();
      → _sourceFolders = [] (empty, in RAM only)
    → private string _outputFolder = string.Empty;
      → _outputFolder = "" (empty, in RAM only)

User Action:
  User selects folders
    → _sourceFolders.Add("/home/user/Downloads")
    → _outputFolder = "/home/user/Organized"
    → These are in RAM, not saved anywhere

App Closes:
  Everything in RAM is garbage collected
    → _sourceFolders = null
    → _outputFolder = null

Next Startup:
  ApplicationUI constructor
    → private List<string> _sourceFolders = new();
      → _sourceFolders = [] (empty again!)
    → User sees empty folder list ❌
```

---

## If You Want Paths to Persist

To save paths across sessions, you would need to:

1. Create a `PathsService` (similar to `CategoryService`)
2. Implement `LoadPathsAsync()` → Read from JSON file
3. Implement `SavePathsAsync()` → Write to JSON file
4. Call `await pathsService.LoadPathsAsync()` on startup
5. Call `await pathsService.SavePathsAsync()` after folder selection

Example persistence file:
```json
// ~/.config/TidyFile/paths.json
{
  "SourceFolders": [
    "/home/user/Downloads",
    "/home/user/Documents"
  ],
  "OutputFolder": "/home/user/Organized",
  "LastUsedAt": "2025-11-15T20:15:00Z"
}
```

Then modify `ApplicationUI.cs`:
```csharp
public async Task RunAsync()
{
    // Load paths from disk
    await _pathsService.LoadPathsAsync();  // ← NEW
    
    _sourceFolders = await _pathsService.GetSourceFoldersAsync();
    _outputFolder = await _pathsService.GetOutputFolderAsync();
    
    // ... rest of UI ...
}

private async Task SelectSourceFoldersAsync()
{
    // ... user selections ...
    
    // Save paths to disk
    await _pathsService.SavePathsAsync();  // ← NEW
}
```

---

## Summary Table

| Aspect | Categories | Paths |
|--------|-----------|-------|
| **Storage** | `CategoryService` + JSON file | `ApplicationUI` private fields |
| **Persistence** | YES (disk) | NO (RAM only) |
| **Load Method** | `LoadCategoriesAsync()` | None (initialized empty) |
| **Save Method** | `SaveCategoriesAsync()` | None (never saved) |
| **Restart Behavior** | Remembered from disk | Reset to empty |
| **User Experience** | "My categories are saved" | "Where were those folders?" |
| **Design Intent** | Static config (reuse) | Transient workflow (per-session) |

---

**File Locations:**
- 📁 **Categories**: `~/.config/TidyFile/categories.json`
- 📁 **Logs**: `~/.config/TidyFile/logs/tidyfile-YYYY-MM-DD.txt`
- 📁 **Source**: `src/TidyFile/Services/CategoryService.cs`
- 📁 **Source**: `src/TidyFile/UI/ApplicationUI.cs`
