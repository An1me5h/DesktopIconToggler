# 🖥️ Desktop Icon Toggler

> A lightweight Windows system tray app that lets you **hide/show desktop icons with a single click** and **automatically makes your taskbar transparent** when you're on the desktop.

![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0078D4?style=flat-square&logo=windows)
![Framework](https://img.shields.io/badge/.NET-6.0-512BD4?style=flat-square&logo=dotnet)
![Language](https://img.shields.io/badge/language-C%23-239120?style=flat-square&logo=csharp)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)

---

## ✨ Features

- **One-click icon toggle** — Left-click the tray icon to instantly show or hide all desktop icons
- **Auto transparent taskbar** — Taskbar becomes transparent when you're on the desktop, returns to normal when any window is open
- **System tray app** — Runs silently in the background, no windows, no clutter
- **Multi-monitor support** — Transparency applies to all taskbars on all connected displays
- **Startup on login** — Installed to run automatically when Windows starts
- **Clean uninstall** — Fully listed in Add/Remove Programs, restores all settings on exit

---

## 🚀 Installation

### Option 1 — Installer (Recommended)

1. Download the latest `DesktopIconToggler_Setup.exe` from [Releases](../../releases)
2. Run the installer and follow the setup wizard
3. The app starts automatically and appears in your system tray

### Option 2 — Build from Source

**Prerequisites:**
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [NSIS](https://nsis.sourceforge.io/Download) *(only if you want to build the installer)*

**Steps:**

```bash
# Clone the repo
git clone https://github.com/yourusername/DesktopIconToggler.git
cd DesktopIconToggler

# Build the executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The `.exe` will be output to:
```
bin\Release\net6.0-windows\win-x64\publish\DesktopIconToggler.exe
```

To also build the installer, run:
```bash
BUILD_INSTALLER.bat
```

---

## 🖱️ How to Use

| Action | Result |
|--------|--------|
| **Left-click** tray icon | Toggle desktop icons on/off |
| **Right-click** tray icon | Open context menu |
| Context menu → **Show Icons** | Force icons visible |
| Context menu → **Hide Icons** | Force icons hidden |
| Context menu → **Auto Taskbar Transparency: ON/OFF** | Enable or disable auto transparency |
| Context menu → **Exit** | Quit app and restore all settings |

The tray icon itself changes to reflect the current state:
- 🟦 Blue grid = icons are **visible**
- ⬜ Grey + ❌ = icons are **hidden**

---

## 🪟 Taskbar Transparency

When enabled, the taskbar automatically becomes **transparent** whenever your desktop is visible (no open windows), and returns to your **normal Windows style** as soon as any window opens.

> **Windows 11 requirement:**  
> Go to **Settings → Personalization → Colors** and make sure **Transparency effects** is turned **ON**.

The transparency feature uses three layered methods for maximum compatibility:
1. Registry keys (`EnableTransparency`, `TaskbarAcrylicOpacity`)
2. `WM_SETTINGCHANGE` broadcast to apply instantly
3. `SetWindowCompositionAttribute` API fallback

---

## 🛠️ How It Works

The app uses native Windows APIs (`user32.dll`) to:

- **Icon toggle** — Finds the desktop's `SysListView32` window (the actual icon list) and calls `ShowWindow()` to hide or show it. No registry changes, fully reversible.
- **Taskbar transparency** — Writes to `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize` and broadcasts a settings change notification to Explorer.
- **Desktop detection** — Runs a 600ms polling loop using `EnumWindows()` to check if any real application window is open, ignoring shell/system windows.

---

## 📁 Project Structure

```
DesktopIconToggler/
├── Program.cs                  # Main app + tray logic
├── DesktopIconToggler.csproj   # .NET 6 project file
├── app.manifest                # Windows app manifest
installer/
├── DesktopIconToggler.nsi      # NSIS installer script
├── License.txt
BUILD_INSTALLER.bat             # One-click build script
```

---

## 🔧 Requirements

| Requirement | Version |
|------------|---------|
| Windows | 10 or 11 (64-bit) |
| .NET Runtime | 6.0+ *(bundled in installer)* |

---

## 📝 License

MIT License — see [LICENSE](LICENSE) for details.

---

## 🤝 Contributing

Pull requests are welcome! For major changes, please open an issue first to discuss what you'd like to change.

1. Fork the repo
2. Create your branch (`git checkout -b feature/YourFeature`)
3. Commit your changes (`git commit -m 'Add YourFeature'`)
4. Push to the branch (`git push origin feature/YourFeature`)
5. Open a Pull Request

---

<p align="center">Made with ❤️ for a cleaner Windows desktop</p>
