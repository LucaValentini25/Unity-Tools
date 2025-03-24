# Asset Renamer

**Asset Renamer** is a Unity Editor tool for batch-renaming assets using a configurable prefix system and category filters.  
It supports drag & drop, previewing names, undo, and persistent settings stored in ScriptableObjects.

## 🔧 Features

- Drag & drop assets to rename
- Automatically assign prefixes based on file type
- Live name preview before applying
- Filter assets by category
- Undo support (session-limited)
- Customizable settings asset (with export/import support)

## ⚙️ Settings Management

- Create a new configuration file (ScriptableObject)
- Import an existing `.asset` file
- Export the current settings for reuse

## ✅ How to Use

1. Open `Tools > Asset Renamer Tool`
2. Drop your assets or select them manually
3. Customize names and prefixes
4. Apply to all or filtered items
5. Use the Undo button if needed