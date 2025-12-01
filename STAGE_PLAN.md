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
