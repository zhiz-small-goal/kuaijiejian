<div align="center">

# 🎨 Photoshop 快捷键工具

一个专为 Photoshop 用户设计的强大快捷键工具，让您的工作效率翻倍！

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue)](https://www.microsoft.com/windows)
[![Photoshop](https://img.shields.io/badge/Photoshop-CS6%2B-31A8FF)](https://www.adobe.com/products/photoshop.html)

[功能特性](#-功能特性) • [快速开始](#-快速开始) • [使用文档](#-使用文档) • [常见问题](#-常见问题) • [贡献指南](#-贡献) • [许可证](#-许可证)

</div>

---

## 📖 简介

这是一个运行在 Windows 系统上的快捷键增强工具，可以让您通过自定义的快捷键快速执行 Photoshop 中的各种操作。告别繁琐的菜单点击，让设计工作更加流畅高效！

### ✨ 功能特性

- ⌨️ **自定义快捷键** - 设置任意键盘组合键执行 Photoshop 操作
- 🎨 **丰富的功能** - 支持图层、文档、路径等常用操作
- 🎭 **多种显示模式** - 悬浮窗、桌面、隐藏模式随心切换
- 🌈 **个性化主题** - 自定义界面颜色和按钮样式
- 📝 **智能记忆** - 自动保存配置和使用习惯
- ⚡ **高性能** - 基于 .NET 8.0，响应迅速
- 🔌 **扩展性强** - 支持执行 Photoshop 动作（Action）
- 💾 **配置导出** - 轻松备份和迁移配置

### 🎬 演示

> TODO: 添加 GIF 或视频演示

## 🚀 快速开始

### 系统要求

| 项目 | 要求 |
|-----|-----|
| 操作系统 | Windows 10 / 11 |
| 运行环境 | [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Photoshop | CS6 或更高版本 |
| 磁盘空间 | 至少 50MB |

### 📥 下载安装

#### 方式一：下载发布版本（推荐）

1. 前往 [Releases](https://github.com/yourusername/photoshop-hotkey-tool/releases) 页面
2. 下载最新版本的压缩包
3. 解压到任意目录（建议：`C:\Program Files\PS快捷键工具`）
4. 双击 `kuaijiejian.exe` 启动工具

> ⚠️ **注意**：请勿将工具放在中文路径或包含特殊字符的文件夹中

#### 方式二：从源码编译

```bash
# 克隆仓库
git clone https://github.com/yourusername/photoshop-hotkey-tool.git
cd photoshop-hotkey-tool

# 使用 Visual Studio 2022 打开
# 或使用命令行编译
dotnet build -c Release

# 运行
cd bin\Release\net8.0-windows
kuaijiejian.exe
```

### 🎯 首次使用

1. **启动工具**
   ```
   双击 kuaijiejian.exe
   ```

2. **验证工作**
   - 打开 Photoshop
   - 在 Photoshop 中打开任意图片
   - 点击工具中的"新建图层"按钮
   - 如果成功创建图层，说明工具工作正常！🎉

3. **添加快捷键**
   - 点击"添加功能"按钮
   - 按下想要的快捷键（如 `Ctrl+Shift+N`）
   - 选择功能（如"新建图层"）
   - 保存并开始使用

## 📚 使用文档

- [📖 使用说明（HTML）](使用说明.html) - 图文并茂的完整使用指南
- [📄 使用说明（TXT）](使用说明.txt) - 纯文本版本
- [🔧 开发规范](开发规范.md) - 开发者文档

### 支持的功能

<details>
<summary>点击展开查看所有功能</summary>

#### 图层操作
- ✅ 新建图层
- ✅ 复制图层
- ✅ 删除图层
- ✅ 合并图层
- ✅ 合并可见图层
- ✅ 图层可见性切换
- ✅ 栅格化图层

#### 文档操作
- ✅ 新建文档
- ✅ 保存文档
- ✅ 智能保存
- ✅ 另存为 JPG
- ✅ 拼合图像

#### 路径操作
- ✅ 路径转形状图层
- ✅ 路径转形状（带羽化）
- ✅ 路径转选区
- ✅ 路径转蒙版
- ✅ 设置矢量蒙版羽化

#### 其他功能
- ✅ 执行 Photoshop 动作（Action）
- ✅ 批量管理快捷键
- ✅ 自定义按钮颜色
- ✅ 多显示模式切换

</details>

## 🔧 常见问题

<details>
<summary><strong>Q: 点击按钮后 Photoshop 没有反应？</strong></summary>

**检查清单：**
- [ ] Photoshop 是否正在运行
- [ ] `PhotoshopScripts` 文件夹是否存在
- [ ] 脚本文件（.jsx）是否完整
- [ ] Photoshop 版本是否支持（CS6+）
- [ ] 是否被防火墙/杀毒软件拦截

**解决方法：**
1. 确保 Photoshop 已启动
2. 检查 `PhotoshopScripts` 文件夹与 exe 在同一目录
3. 将工具添加到安全软件白名单
</details>

<details>
<summary><strong>Q: 提示缺少 .NET 运行环境？</strong></summary>

下载并安装 [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)：
1. 访问官方下载页面
2. 选择 "Download .NET Runtime 8.0" → Windows x64
3. 安装后重启工具
</details>

<details>
<summary><strong>Q: 快捷键冲突怎么办？</strong></summary>

1. 点击功能按钮旁的 "..." 菜单
2. 选择"删除"
3. 重新添加并设置新的快捷键
</details>

<details>
<summary><strong>Q: 配置文件在哪里？</strong></summary>

配置文件位于工具目录：
- `functions_config.json` - 功能配置
- `theme_config.json` - 主题配置
- `display_mode_config.json` - 显示模式配置

可直接复制备份或编辑。
</details>

更多问题请查看 [完整使用说明](使用说明.html) 或提交 [Issue](https://github.com/yourusername/photoshop-hotkey-tool/issues)。

## 🛠️ 技术栈

- **框架**: WPF (.NET 8.0)
- **UI 库**: [HandyControl](https://github.com/HandyOrg/HandyControl)
- **语言**: C# 12.0
- **脚本**: Adobe ExtendScript (JSX)
- **构建**: Visual Studio 2022

## 🤝 贡献

欢迎贡献代码、报告问题或提出建议！

### 贡献方式

1. 🐛 [报告 Bug](https://github.com/yourusername/photoshop-hotkey-tool/issues/new?template=bug_report.md)
2. 💡 [提出新功能](https://github.com/yourusername/photoshop-hotkey-tool/issues/new?template=feature_request.md)
3. 📝 改进文档
4. 🔧 提交代码

### 贡献步骤

```bash
# Fork 本仓库并克隆
git clone https://github.com/your-username/photoshop-hotkey-tool.git

# 创建特性分支
git checkout -b feature/amazing-feature

# 提交更改
git commit -m "feat: 添加某某功能"

# 推送到分支
git push origin feature/amazing-feature

# 创建 Pull Request
```

详细信息请查看 [贡献指南](CONTRIBUTING.md)。

## 📋 开发计划

- [ ] 支持更多 Photoshop 功能
- [ ] 快捷键配置导入/导出
- [ ] 多语言支持（英文）
- [ ] 云同步配置
- [ ] 插件系统
- [ ] macOS 版本（待评估）

查看完整计划：[项目看板](https://github.com/yourusername/photoshop-hotkey-tool/projects)

## 📜 更新日志

查看 [CHANGELOG.md](CHANGELOG.md) 了解各版本的详细变更。

### 最新版本 v1.0.0 (2025-10-17)

- 🎉 首次发布
- ⌨️ 自定义快捷键功能
- 🎨 支持常用 Photoshop 操作
- 🎭 三种显示模式
- 🌈 自定义主题

## ⚖️ 许可证

本项目采用 [MIT License](LICENSE) 许可证。

```
MIT License

Copyright (c) 2025 Photoshop 快捷键工具

允许任何人免费获取、使用、修改和分发本软件及其文档。
```

## 💖 致谢

- [HandyControl](https://github.com/HandyOrg/HandyControl) - 优秀的 WPF UI 库
- Adobe Photoshop - 强大的图像处理软件
- 所有贡献者和用户的支持

## 📞 联系方式

- 提交 Issue: [GitHub Issues](https://github.com/yourusername/photoshop-hotkey-tool/issues)
- 讨论交流: [GitHub Discussions](https://github.com/yourusername/photoshop-hotkey-tool/discussions)

---

<div align="center">

**如果这个项目对您有帮助，请给个 ⭐ Star 支持一下！**

Made with ❤️ for Photoshop Users

[返回顶部](#-photoshop-快捷键工具)

</div>
