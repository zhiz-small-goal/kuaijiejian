<div align="center">

# 🎨 Photoshop Hotkey Tool

A powerful hotkey tool designed for Photoshop users to boost your productivity!

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue)](https://www.microsoft.com/windows)
[![Photoshop](https://img.shields.io/badge/Photoshop-CS6%2B-31A8FF)](https://www.adobe.com/products/photoshop.html)

[Features](#-features) • [Quick Start](#-quick-start) • [Documentation](#-documentation) • [FAQ](#-faq) • [Contributing](#-contributing) • [License](#-license)

[中文文档](README.md)

</div>

---

## 📖 Introduction

A Windows-based hotkey enhancement tool that allows you to execute various Photoshop operations through custom keyboard shortcuts. Say goodbye to tedious menu clicking and make your design work more fluid and efficient!

### ✨ Features

- ⌨️ **Custom Hotkeys** - Set any keyboard combination to execute Photoshop operations
- 🎨 **Rich Functions** - Support for layers, documents, paths, and other common operations
- 🎭 **Multiple Display Modes** - Switch between floating, desktop, and hidden modes
- 🌈 **Personalized Themes** - Customize interface colors and button styles
- 📝 **Smart Memory** - Automatically save configurations and usage habits
- ⚡ **High Performance** - Based on .NET 8.0 for quick response
- 🔌 **Extensible** - Support for executing Photoshop Actions
- 💾 **Config Export** - Easy backup and migration of configurations

## 🚀 Quick Start

### System Requirements

| Item | Requirement |
|------|-------------|
| Operating System | Windows 10 / 11 |
| Runtime | [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Photoshop | CS6 or higher |
| Disk Space | At least 50MB |

### 📥 Installation

#### Option 1: Download Release (Recommended)

1. Go to [Releases](https://github.com/yourusername/photoshop-hotkey-tool/releases) page
2. Download the latest version
3. Extract to any directory (Recommended: `C:\Program Files\PS Hotkey Tool`)
4. Double-click `kuaijiejian.exe` to launch

> ⚠️ **Note**: Do not place the tool in paths containing Chinese characters or special symbols

#### Option 2: Build from Source

```bash
# Clone repository
git clone https://github.com/yourusername/photoshop-hotkey-tool.git
cd photoshop-hotkey-tool

# Open with Visual Studio 2022
# Or build via command line
dotnet build -c Release

# Run
cd bin\Release\net8.0-windows
kuaijiejian.exe
```

### 🎯 First Use

1. **Launch the tool**
   ```
   Double-click kuaijiejian.exe
   ```

2. **Verify it works**
   - Open Photoshop
   - Open any image in Photoshop
   - Click the "New Layer" button in the tool
   - If a layer is created successfully, the tool is working! 🎉

3. **Add hotkeys**
   - Click "Add Function" button
   - Press your desired hotkey (e.g., `Ctrl+Shift+N`)
   - Select function (e.g., "New Layer")
   - Save and start using

## 📚 Documentation

- [📖 User Guide (HTML)](使用说明.html) - Complete illustrated guide
- [📄 User Guide (TXT)](使用说明.txt) - Plain text version
- [🔧 Development Guide](开发规范.md) - Developer documentation (Chinese)

### Supported Features

<details>
<summary>Click to expand all features</summary>

#### Layer Operations
- ✅ New Layer
- ✅ Duplicate Layer
- ✅ Delete Layer
- ✅ Merge Layers
- ✅ Merge Visible
- ✅ Toggle Layer Visibility
- ✅ Rasterize Layer

#### Document Operations
- ✅ New Document
- ✅ Save Document
- ✅ Smart Save
- ✅ Save as JPG
- ✅ Flatten Image

#### Path Operations
- ✅ Path to Shape Layer
- ✅ Path to Shape (with Feather)
- ✅ Path to Selection
- ✅ Path to Mask
- ✅ Set Vector Mask Feather

#### Other Features
- ✅ Execute Photoshop Actions
- ✅ Batch Manage Hotkeys
- ✅ Custom Button Colors
- ✅ Multiple Display Modes

</details>

## 🔧 FAQ

<details>
<summary><strong>Q: Photoshop doesn't respond after clicking buttons?</strong></summary>

**Checklist:**
- [ ] Is Photoshop running
- [ ] Does `PhotoshopScripts` folder exist
- [ ] Are script files (.jsx) complete
- [ ] Is Photoshop version supported (CS6+)
- [ ] Is it blocked by firewall/antivirus

**Solution:**
1. Ensure Photoshop is running
2. Check if `PhotoshopScripts` folder is in the same directory as exe
3. Add tool to security software whitelist
</details>

<details>
<summary><strong>Q: Missing .NET runtime error?</strong></summary>

Download and install [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0):
1. Visit official download page
2. Select "Download .NET Runtime 8.0" → Windows x64
3. Restart tool after installation
</details>

<details>
<summary><strong>Q: Hotkey conflicts?</strong></summary>

1. Click "..." menu next to function button
2. Select "Delete"
3. Re-add with a new hotkey
</details>

More questions? Check [complete guide](使用说明.html) or submit an [Issue](https://github.com/yourusername/photoshop-hotkey-tool/issues).

## 🛠️ Tech Stack

- **Framework**: WPF (.NET 8.0)
- **UI Library**: [HandyControl](https://github.com/HandyOrg/HandyControl)
- **Language**: C# 12.0
- **Scripts**: Adobe ExtendScript (JSX)
- **Build**: Visual Studio 2022

## 🤝 Contributing

Contributions are welcome! Feel free to submit code, report issues, or suggest features.

### How to Contribute

1. 🐛 [Report Bugs](https://github.com/yourusername/photoshop-hotkey-tool/issues/new?template=bug_report.md)
2. 💡 [Request Features](https://github.com/yourusername/photoshop-hotkey-tool/issues/new?template=feature_request.md)
3. 📝 Improve Documentation
4. 🔧 Submit Code

See [Contributing Guide](CONTRIBUTING.md) for details.

## 📋 Roadmap

- [ ] More Photoshop features
- [ ] Import/Export hotkey configurations
- [ ] Multi-language support
- [ ] Cloud sync for configurations
- [ ] Plugin system
- [ ] macOS version (under evaluation)

## 📜 Changelog

See [CHANGELOG.md](CHANGELOG.md) for detailed changes.

### Latest: v1.0.0 (2025-10-17)

- 🎉 Initial release
- ⌨️ Custom hotkey functionality
- 🎨 Common Photoshop operations support
- 🎭 Three display modes
- 🌈 Custom themes

## ⚖️ License

This project is licensed under the [MIT License](LICENSE).

## 💖 Acknowledgments

- [HandyControl](https://github.com/HandyOrg/HandyControl) - Excellent WPF UI library
- Adobe Photoshop - Powerful image processing software
- All contributors and users for their support

## 📞 Contact

- Submit Issue: [GitHub Issues](https://github.com/yourusername/photoshop-hotkey-tool/issues)
- Discussions: [GitHub Discussions](https://github.com/yourusername/photoshop-hotkey-tool/discussions)

---

<div align="center">

**If this project helps you, please give it a ⭐ Star!**

Made with ❤️ for Photoshop Users

[Back to Top](#-photoshop-hotkey-tool)

</div>

