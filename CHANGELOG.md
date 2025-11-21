# 更新日志

所有项目的重要变更都将记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)，
并且本项目遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [未发布]

### 计划添加
- 更多 Photoshop 脚本支持
- 快捷键导入/导出功能
- 多语言支持（英文）
- 云同步配置功能

## [1.0.0] - 2025-10-17

### 新增
- 🎉 首次发布
- ⌨️ 自定义快捷键功能
- 🎨 支持常用 Photoshop 操作
  - 图层操作（新建、复制、删除、合并等）
  - 文档操作（新建、保存、另存为 JPG 等）
  - 路径操作（路径转形状、蒙版、选区等）
- 🎭 三种显示模式（悬浮窗、桌面、隐藏）
- 🌈 自定义按钮颜色
- 📝 图层选择器
- ⚡ 执行 Photoshop 动作（Action）
- 🔧 批量删除功能
- 💾 配置文件自动保存
- 📚 完整的用户文档（HTML/TXT/MD 格式）
- 🛠️ 一键打包脚本

### 技术特性
- 基于 .NET 8.0 和 WPF
- 使用 HandyControl UI 库
- ExtendScript 脚本集成
- 全局热键支持
- 配置文件持久化（JSON）

### 已知问题
- Photoshop CS6 部分功能可能不稳定
- 某些复杂路径转换可能需要多次尝试
- 高 DPI 屏幕下可能存在显示缩放问题

---

## 版本说明

### 版本号格式
版本号格式为 `主版本号.次版本号.修订号`：
- **主版本号**：不兼容的 API 修改
- **次版本号**：向下兼容的功能性新增
- **修订号**：向下兼容的问题修正

### 更新类型
- **新增**：新功能
- **变更**：已有功能的变更
- **弃用**：即将移除的功能
- **移除**：已移除的功能
- **修复**：Bug 修复
- **安全**：安全相关的修复

---

[未发布]: https://github.com/yourusername/photoshop-hotkey-tool/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/yourusername/photoshop-hotkey-tool/releases/tag/v1.0.0

