# 贡献指南

感谢您考虑为 Photoshop 快捷键工具做出贡献！

## 行为准则

参与本项目即表示您同意遵守我们的行为准则，请保持友好和尊重。

## 如何贡献

### 报告 Bug

如果您发现了 Bug，请创建一个 Issue 并包含以下信息：

- **清晰的标题**：简洁地描述问题
- **详细描述**：问题的详细说明
- **复现步骤**：如何重现这个问题
  1. 步骤一
  2. 步骤二
  3. ...
- **预期行为**：您期望发生什么
- **实际行为**：实际发生了什么
- **环境信息**：
  - 操作系统版本
  - .NET 版本
  - Photoshop 版本
  - 工具版本
- **截图**：如果适用，添加截图帮助说明问题
- **额外信息**：其他可能有用的信息

### 提出新功能

如果您有新功能的想法，请创建一个 Issue 并包含：

- **功能描述**：清晰地描述您想要的功能
- **使用场景**：为什么需要这个功能
- **建议的实现方式**：如果有的话
- **替代方案**：考虑过的其他方案

### 提交代码

1. **Fork 本仓库**
   - 点击右上角的 "Fork" 按钮

2. **克隆您的 Fork**
   ```bash
   git clone https://github.com/your-username/photoshop-hotkey-tool.git
   cd photoshop-hotkey-tool
   ```

3. **创建特性分支**
   ```bash
   git checkout -b feature/your-feature-name
   # 或
   git checkout -b fix/your-bug-fix
   ```

4. **进行修改**
   - 遵循现有的代码风格
   - 添加必要的注释
   - 更新相关文档

5. **提交更改**
   ```bash
   git add .
   git commit -m "feat: 添加某某功能"
   # 或
   git commit -m "fix: 修复某某问题"
   ```

   提交信息格式：
   - `feat: 新功能`
   - `fix: Bug 修复`
   - `docs: 文档更新`
   - `style: 代码格式调整`
   - `refactor: 代码重构`
   - `test: 测试相关`
   - `chore: 构建/工具相关`

6. **推送到您的 Fork**
   ```bash
   git push origin feature/your-feature-name
   ```

7. **创建 Pull Request**
   - 在 GitHub 上打开 Pull Request
   - 填写 PR 模板
   - 等待审核

## 开发环境设置

### 必需工具

- Visual Studio 2022 或更高版本
- .NET 8.0 SDK
- Adobe Photoshop（用于测试）

### 构建项目

1. 打开 `kuaijiejian.sln`
2. 还原 NuGet 包
3. 按 `F5` 运行/调试

### 项目结构

```
kuaijiejian/
├── PhotoshopScripts/     # Photoshop 脚本文件 (.jsx)
├── *.cs                  # C# 源代码文件
├── *.xaml                # WPF 界面文件
├── kuaijiejian.csproj    # 项目文件
└── ...
```

## 代码规范

### C# 代码风格

- 使用 4 个空格缩进
- 类名使用 PascalCase
- 方法名使用 PascalCase
- 变量名使用 camelCase
- 私有字段使用 camelCase 或 _camelCase
- 常量使用 UPPER_CASE 或 PascalCase

示例：
```csharp
public class FunctionManager
{
    private List<FunctionItem> _functions;
    
    public void AddFunction(FunctionItem item)
    {
        // 实现
    }
}
```

### XAML 代码风格

- 属性按字母顺序排列（除了 Name 和 Class 优先）
- 使用有意义的控件名称

### ExtendScript (JSX) 代码风格

- 使用 4 个空格缩进
- 函数名使用 camelCase
- 添加必要的注释说明功能

## 添加新的 Photoshop 脚本

如果您想添加新的 Photoshop 功能：

1. 在 `PhotoshopScripts/` 目录创建 `.jsx` 文件
2. 实现功能，确保兼容性
3. 添加错误处理
4. 在代码中添加注释说明用途
5. 测试脚本在不同 Photoshop 版本中的表现
6. 更新文档，说明新功能

脚本模板：
```javascript
// 脚本名称: YourScript.jsx
// 功能说明: 这个脚本做什么
// 作者: 您的名字
// 日期: 2025-XX-XX

try {
    // 检查前置条件
    if (!app.documents.length) {
        alert("请先打开一个文档");
    } else {
        // 主要功能实现
        
    }
} catch (e) {
    alert("错误: " + e.message);
}
```

## 测试

在提交 PR 之前，请确保：

- ✅ 代码可以正常编译
- ✅ 新功能经过测试
- ✅ 没有破坏现有功能
- ✅ 在不同 Photoshop 版本中测试（如果相关）
- ✅ 检查是否有内存泄漏
- ✅ 更新了相关文档

## 文档更新

如果您的更改影响用户使用，请更新：

- `README.md` - 如果是重要功能
- `使用说明.html` 和 `使用说明.txt` - 详细使用说明
- `CHANGELOG.md` - 记录更改
- 代码注释 - 解释复杂逻辑

## Pull Request 准则

一个好的 PR 应该：

- ✅ 只关注一个功能或修复
- ✅ 有清晰的标题和描述
- ✅ 包含必要的测试
- ✅ 更新相关文档
- ✅ 遵循代码规范
- ✅ 提交历史清晰
- ✅ 没有不相关的文件变更

## 获取帮助

如果您在贡献过程中遇到问题：

- 📖 查看现有文档
- 💬 在 Issue 中提问
- 📧 联系维护者

## 许可协议

提交代码即表示您同意您的贡献将在 MIT 许可证下发布。

---

再次感谢您的贡献！🎉

