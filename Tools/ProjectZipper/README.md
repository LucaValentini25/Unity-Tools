# Project Zipper

**Project Zipper** is a Unity tool that allows exporting the essential directories of a project (`Assets`, `ProjectSettings`, `Packages`) into a `.zip` file as a manual backup.

## 🧰 What does it do?

- Creates a temporary copy of the project.
- Compresses the essential folders into a `.zip`.
- Prevents saving inside the Unity project folder.
- Shows a progress bar during the operation.

## ✅ How to use it?

1. Go to `Tools > Export to Zip`.
2. Select the location and name of the `.zip` file.
3. Done! The project is saved as a backup.

![image](https://github.com/user-attachments/assets/e7845fcb-7bb2-4205-af98-4c2d46c1415d)


## ⚠️ Limitations

- Does not include folders like `Library`, `Logs`, etc.
- The `.zip` name can be edited manually (defaults to the project name).
- Does not allow customizing which folders to export.
