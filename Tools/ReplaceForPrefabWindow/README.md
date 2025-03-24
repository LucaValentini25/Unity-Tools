# Prefab Replacer

**Prefab Replacer** is a Unity Editor tool that replaces selected scene objects with a prefab while preserving their transform, name, and optionally their components.

## 🔧 Features

- Replace one or many objects with a prefab
- Keep position, rotation, and scale
- Copy name from object, prefab, or custom string
- Optionally copy all components from original object
- Disable or destroy the original objects
- Organized UI with collapsible sections

## 📦 How to Install
You can install this tool via Unity's Package Manager using a Git URL:
1. Open Unity
2. Go to `Window > Package Manager`
3. Click the `+` button → *Add package from Git URL...*
4. Paste the link and click `Add`
```bash
https://github.com/LucaValentini25/Unity-Tools.git?path=Tools/PrefabReplacer
```

## ✅ How to Use

1. Go to `Tools > Replace For Prefab`.
2. Assign a prefab and the objects to replace.
3. Customize settings (transform, naming, components, handling).
4. Click **Replace** to apply the operation.

![image](https://github.com/user-attachments/assets/dae2d50a-4966-4ff1-8d94-85f683f8d412)

## ⚠️ Limitations

- Does not copy child objects or hierarchies.
- Copying components may not handle all types perfectly (use with caution).
- No undo – make a backup before running if needed.

## 🧾 Requirements

- Unity 2021.3 or higher
- Editor only


