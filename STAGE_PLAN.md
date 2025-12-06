### [001] 2025-12-01 窗口跟随不抢焦点调整

日期：2025-12-01  
状态：已完成  

**变更类型：**  
- 行为调整 / Bug 修复

**目标：**  
- 保持窗口跟随 Photoshop 时不抢夺焦点  
- Alt/Win 切换和任务栏预览时避免干扰其他窗口  
- 维持自动显示/隐藏能力并提升用户体验

**触发原因：**  
- 用户反馈 Alt+Tab 与任务栏预览时窗口抢占焦点，导致切换失败

**涉及文件：**  
- WindowsApiHelper.cs  
- MainWindow.xaml.cs

**改动概览：**  
- WindowsApiHelper.cs：补充 ShowWindow/SetWindowPos 封装，新增无激活显示/最小化方法。  
- MainWindow.xaml.cs → PhotoshopMonitorTimer_Tick：使用无激活显示/最小化，增加 Alt/Win 切换防抖与句柄缓存，避免前台抢焦点。  
- MainWindow.xaml.cs → 新增 IsTaskSwitchInProgress 辅助方法，维护窗口句柄字段。

**关键点说明：**  
- 窗口恢复使用 SW_SHOWNOACTIVATE + SWP_NOACTIVATE，最小化使用 SW_SHOWMINNOACTIVE，确保不触发激活。  
- Alt/Win 组合切换时跳过跟随逻辑，防止切换过程中被重新激活。  
- 仅在前台为 Photoshop 时恢复窗口，离开后添加 300ms 防抖再最小化。

**测试验证：**  
- [ ] Photoshop 前台时窗口自动显示且不夺焦点（连续 Alt+Tab 切换验证）  
- [ ] Photoshop 置后台后窗口 300ms 内不闪烁并自动最小化  
- [ ] 任务栏缩略图悬停预览其他窗口时，工具窗口不抢焦点  
- [ ] 常规功能按钮点击仍可正常触发 Photoshop 脚本

**后续 TODO：**  
- [ ] 如仍有抢焦点场景，考虑在显示前检测前台线程输入状态进一步防抖  
- [ ] 增加可配置的防抖时间与切换键监听开关

### [002] 2025-12-06 按钮拖拽顺序持久化修复

日期：2025-12-06  
状态：已完成  

**变更类型：**  
- Bug 修复

**涉及阶段：**  
- 无

**目标：**  
- 重新启动后保留用户拖拽后的按钮顺序  
- 兼容旧版 functions_config.json 文件结构  
- 新增/删除功能后仍保持稳定排序

**触发原因：**  
- 用户拖动主窗口按钮后重启，顺序恢复为分组默认顺序

**涉及文件：**  
- `FunctionManager.cs`  
- `functions_config.json`（运行后生成的配置）

**改动概览：**  
- `FunctionManager.cs` -> `RefreshAllFunctions()`: 按现有 AllFunctions 顺序对齐，新增项追加，避免拖拽顺序被重排  
- `FunctionManager.cs` -> `SaveFunctions()/LoadFunctions()`: 新增 AllFunctions 序列化字段，优先按统一列表保存/加载，旧配置回退到 System/Application 列表  
- `FunctionManager.cs` -> `ApplyOrderedFunctions()/BuildFunctionKey()`: 新增有序应用及唯一键生成，三套集合同步保持一致

**关键点说明：**  
- AllFunctions 作为顺序真源，保存时仍写入旧字段以兼容历史配置  
- 去重依据 FunctionType + Category 及命令/动作标识，避免重复且保持顺序

**测试验证：**  
- [ ] 拖拽调整任意按钮顺序后重启，顺序保持不变  
- [ ] 新增功能后重启，新增项保留在添加位置  
- [ ] 旧版仅含 System/Application 字段的 functions_config.json 正常加载并支持顺序调整

**后续 TODO：**  
- [ ] 无
