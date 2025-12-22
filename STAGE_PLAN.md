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

### [003] 2025-12-22 禁用主窗口边缘缩放防止卡死

日期：2025-12-22  
状态：已完成  

**变更类型：**  
- 行为调整  

**目标：**  
- 禁止主窗口边缘缩放，避免进入系统缩放模态导致卡住  
- 保持窗口布局仍由按钮列数与内容自适应控制  
- 不影响拖动移动、置顶与窗口跟随功能  

**触发原因：**  
- 用户在窗口边缘误触缩放后，窗口与鼠标交互被锁定，影响其它程序操作  

**涉及文件：**  
- `MainWindow.xaml`  

**改动概览：**  
- `MainWindow.xaml`  
  - 窗口配置：将 `ResizeMode` 设为 `NoResize`  
  - `WindowChrome`：将 `ResizeBorderThickness` 设为 `0`，移除边缘缩放命中区域  

**关键点说明：**  
- 仅禁用用户拖边缩放，窗口大小仍可由程序逻辑（按钮布局、自适应）调整  
- 保留无边框窗口样式与拖动移动行为，不改变现有功能流程  

**测试验证：**  
- [ ] 主窗口边缘不再出现缩放光标，无法拖动缩放  
- [ ] 调整按钮列数后窗口宽度仍随配置自动变化  
- [ ] 窗口拖动移动、置顶、跟随 Photoshop 显示/隐藏仍正常  

**后续 TODO：**  
- [ ] 无

### [004] 2025-12-22 主窗口拖动改为手动位移防止卡死

日期：2025-12-22  
状态：已完成  

**变更类型：**  
- Bug 修复  

**目标：**  
- 修复主窗口拖动释放后卡死的问题  
- 保持无激活窗口策略与窗口跟随逻辑不变  
- 不影响按钮拖拽与常规交互  

**触发原因：**  
- 用户反馈拖动窗口松开鼠标后卡死无响应  

**涉及文件：**  
- `MainWindow.xaml`  
- `MainWindow.xaml.cs`  

**改动概览：**  
- `MainWindow.xaml`  
  - `MainAreaBorder`：新增 `MouseMove`、`MouseLeftButtonUp`、`LostMouseCapture` 事件绑定  
- `MainWindow.xaml.cs`  
  - `MainAreaBorder_MouseLeftButtonDown()`：改为捕获鼠标并记录起始位置，避免 `DragMove()`  
  - `MainAreaBorder_MouseMove()`：根据鼠标移动量手动更新 `Left/Top`  
  - `MainAreaBorder_MouseLeftButtonUp()` / `MainAreaBorder_LostMouseCapture()`：统一结束拖动并释放鼠标捕获  

**关键点说明：**  
- 取消 `DragMove()` 的系统移动模态，避免与 `WS_EX_NOACTIVATE` 冲突导致卡死  
- 仅影响窗口拖动实现方式，不改变窗口跟随与置顶行为  

**测试验证：**  
- [ ] 按住主窗口空白区域拖动，松开鼠标后窗口仍可正常响应  
- [ ] 拖动过程中切换到其他程序不会导致窗口卡死  
- [ ] 按钮点击、按钮拖拽排序与快捷功能执行正常  

**后续 TODO：**  
- [ ] 无
