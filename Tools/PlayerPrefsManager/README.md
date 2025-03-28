# PlayerPrefs Manager

**PlayerPrefs Manager** is a Unity Editor tool that allows you to **view**, **edit**, **delete**, and **add** PlayerPrefs directly from the Windows Registry. It includes locking support to prevent accidental deletion and visual organization for easy inspection.

> ⚠️ Windows-only. Requires Unity Editor running on Windows.

## ⚠️ Disclaimer

This tool **reads and modifies** PlayerPrefs stored in the **Windows Registry** under your Unity project’s path.  
Use with caution and always double-check entries before modifying or deleting them.

## 🔧 Features

- View all existing PlayerPrefs stored via Windows Registry
- Filter out internal Unity prefs automatically
- Lock specific prefs to protect them from deletion
- Add new custom PlayerPrefs (int, float, string)
- Modify or delete existing prefs individually
- Copy key or value to clipboard with visual feedback
- “Clear All” respects locked entries

## 🧩 How to Install

Install via Unity Package Manager using the Git URL:

```bash
https://github.com/LucaValentini25/Unity-Tools.git?path=Tools/PlayerPrefsManager
```


## ✅ How to Use

1. Go to `Tools > PlayerPrefs Manager` in Unity
2. Use the `Existing Prefs` tab to inspect, lock, edit, or delete values
3. Use the `Add New` tab to create new PlayerPrefs
4. Click **Apply New Prefs** to commit added entries

<p align="center">
  <img src="https://github.com/user-attachments/assets/c6437ef9-4f03-4580-bf73-d9ef83ad3b69" width="45%">
  <img src="https://github.com/user-attachments/assets/cb431169-a361-44ed-a283-ebd9174c6e5f" width="45%">
</p>
---

## 🧾 Requirements

- Unity 2021.3 or newer
- Only supported on Windows (due to Registry access)

---

## 📚 License

If not otherwise specified, this tool is covered by the default license of this repository.

---

Made with ❤️ by **Claster Tools**
