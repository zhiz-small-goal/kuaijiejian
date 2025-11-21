using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Kuaijiejian
{
    /// <summary>
    /// 功能管理器 - 管理系统功能和应用功能
    /// </summary>
    public class FunctionManager
    {
        // 配置文件保存在应用程序目录（确保有写入权限）
        private static readonly string ConfigPath = System.IO.Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory,
            "functions_config.json"
        );

        /// <summary>
        /// 系统功能列表（保留用于配置文件存储）
        /// </summary>
        public ObservableCollection<FunctionItem> SystemFunctions { get; set; } = new();

        /// <summary>
        /// 应用功能列表（保留用于配置文件存储）
        /// </summary>
        public ObservableCollection<FunctionItem> ApplicationFunctions { get; set; } = new();

        /// <summary>
        /// 统一的所有功能列表（UI显示用）
        /// 按添加顺序混合显示图层功能和动作功能
        /// </summary>
        public ObservableCollection<FunctionItem> AllFunctions { get; set; } = new();

        public FunctionManager()
        {
            LoadFunctions();
            RefreshAllFunctions();
        }

        /// <summary>
        /// 刷新统一列表 - 合并系统功能和应用功能
        /// 按添加顺序排列（保持原有顺序）
        /// </summary>
        public void RefreshAllFunctions()
        {
            AllFunctions.Clear();
            
            // 按添加顺序合并两个列表
            foreach (var item in SystemFunctions)
            {
                AllFunctions.Add(item);
            }
            
            foreach (var item in ApplicationFunctions)
            {
                AllFunctions.Add(item);
            }
        }

        /// <summary>
        /// 添加功能
        /// </summary>
        public void AddFunction(FunctionItem item)
        {
            if (item.Category == "System")
            {
                // 去重检查：按功能内容（Command）去重，而不是名字
                // 对于 PhotoshopScript，Command 存储的是脚本内容
                var existingItem = SystemFunctions.FirstOrDefault(f => 
                    f.Command == item.Command && f.FunctionType == item.FunctionType);
                
                if (existingItem != null)
                {
                    // 更新已存在的功能（保留脚本，但可以更新名字和描述）
                    int index = SystemFunctions.IndexOf(existingItem);
                    SystemFunctions[index] = item;
                }
                else
                {
                    // 添加新功能（脚本内容不同，即使名字相同也添加）
                    SystemFunctions.Add(item);
                }
            }
            else
            {
                // 去重检查：对于动作功能，检查动作名称和动作集名称
                if (item.FunctionType == "PhotoshopAction")
                {
                    if (!ApplicationFunctions.Any(f => f.ActionSetName == item.ActionSetName && f.ActionName == item.ActionName))
                    {
                        ApplicationFunctions.Add(item);
                    }
                }
                else
                {
                    // 普通命令，按命令内容去重（而不是名字）
                    if (!ApplicationFunctions.Any(f => f.Command == item.Command && f.FunctionType == item.FunctionType))
                    {
                        ApplicationFunctions.Add(item);
                    }
                }
            }
            
            RefreshAllFunctions();
            SaveFunctions();
        }

        /// <summary>
        /// 删除功能
        /// </summary>
        public void RemoveFunction(FunctionItem item)
        {
            if (item.Category == "System")
            {
                SystemFunctions.Remove(item);
            }
            else
            {
                ApplicationFunctions.Remove(item);
            }
            
            RefreshAllFunctions();
            SaveFunctions();
        }

        /// <summary>
        /// 清空所有应用功能
        /// </summary>
        public void ClearApplicationFunctions()
        {
            ApplicationFunctions.Clear();
            RefreshAllFunctions();
            SaveFunctions();
        }

        /// <summary>
        /// 清空所有系统功能
        /// </summary>
        public void ClearSystemFunctions()
        {
            SystemFunctions.Clear();
            RefreshAllFunctions();
            SaveFunctions();
        }

        /// <summary>
        /// 清空所有功能
        /// </summary>
        public void ClearAllFunctions()
        {
            SystemFunctions.Clear();
            ApplicationFunctions.Clear();
            RefreshAllFunctions();
            SaveFunctions();
        }

        /// <summary>
        /// 保存功能配置到文件
        /// </summary>
        public void SaveFunctions()
        {
            try
            {
                var config = new FunctionConfig
                {
                    SystemFunctions = SystemFunctions.ToList(),
                    ApplicationFunctions = ApplicationFunctions.ToList()
                };

                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存功能配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 从文件加载功能配置
        /// </summary>
        public void LoadFunctions()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var config = JsonSerializer.Deserialize<FunctionConfig>(json);

                    if (config != null)
                    {
                        SystemFunctions.Clear();
                        ApplicationFunctions.Clear();

                        // 去重加载：按功能内容（Command + FunctionType）作为唯一标识
                        // 这样即使名字相同，只要脚本不同就是不同的功能
                        var uniqueSystemFunctions = config.SystemFunctions
                            .GroupBy(f => new { f.Command, f.FunctionType })
                            .Select(g => g.First())
                            .ToList();

                        // 【关键修复】对 ApplicationFunctions 按类型分别去重
                        var uniqueApplicationFunctions = config.ApplicationFunctions
                            .GroupBy(f => new 
                            { 
                                f.FunctionType,
                                // 动作功能用 ActionSetName + ActionName 去重
                                // 其他功能用 Command 去重
                                UniqueKey = f.FunctionType == "PhotoshopAction" 
                                    ? $"{f.ActionSetName}|{f.ActionName}" 
                                    : f.Command
                            })
                            .Select(g => g.First())
                            .ToList();

                        foreach (var item in uniqueSystemFunctions)
                        {
                            SystemFunctions.Add(item);
                        }

                        foreach (var item in uniqueApplicationFunctions)
                        {
                            ApplicationFunctions.Add(item);
                        }

                        RefreshAllFunctions();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载功能配置失败: {ex.Message}");
            }

            // 如果加载失败或文件不存在，创建默认功能
            CreateDefaultFunctions();
        }

        /// <summary>
        /// 创建默认功能
        /// </summary>
        private void CreateDefaultFunctions()
        {
            SystemFunctions.Clear();
            ApplicationFunctions.Clear();

            // 默认系统功能
            SystemFunctions.Add(new FunctionItem
            {
                Name = "打开文件夹",
                Icon = "📂",
                Hotkey = "Ctrl+O",
                Command = "explorer.exe",
                Category = "System"
            });

            SystemFunctions.Add(new FunctionItem
            {
                Name = "系统设置",
                Icon = "⚙️",
                Hotkey = "Ctrl+,",
                Command = "ms-settings:",
                Category = "System"
            });

            // 默认应用功能
            ApplicationFunctions.Add(new FunctionItem
            {
                Name = "记事本",
                Icon = "📝",
                Hotkey = "Ctrl+N",
                Command = "notepad.exe",
                Category = "Application"
            });

            ApplicationFunctions.Add(new FunctionItem
            {
                Name = "计算器",
                Icon = "🔢",
                Hotkey = "Ctrl+C",
                Command = "calc.exe",
                Category = "Application"
            });

            SaveFunctions();
        }

        private class FunctionConfig
        {
            public System.Collections.Generic.List<FunctionItem> SystemFunctions { get; set; } = new();
            public System.Collections.Generic.List<FunctionItem> ApplicationFunctions { get; set; } = new();
        }
    }
}


