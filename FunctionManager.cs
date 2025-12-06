using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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
        /// 刷新统一列表
        /// 保持已有顺序，追加新增项，移除已删除项
        /// </summary>
        public void RefreshAllFunctions()
        {
            // 1) 组合最新的功能集合
            var latestItems = SystemFunctions.Concat(ApplicationFunctions).ToList();

            // 2) 按现有顺序对齐（避免拖拽顺序被打乱）
            var ordered = new List<FunctionItem>();
            var remaining = new Dictionary<string, FunctionItem>();

            foreach (var item in latestItems)
            {
                var key = BuildFunctionKey(item);
                if (!remaining.ContainsKey(key))
                {
                    remaining[key] = item;
                }
            }

            foreach (var item in AllFunctions)
            {
                var key = BuildFunctionKey(item);
                if (remaining.TryGetValue(key, out var matched))
                {
                    ordered.Add(matched);
                    remaining.Remove(key);
                }
            }

            // 3) 追加新增功能（用户新添加的功能按添加顺序在末尾）
            ordered.AddRange(remaining.Values);

            AllFunctions.Clear();
            foreach (var item in ordered)
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
                // 优先使用 AllFunctions 的顺序进行持久化，避免跨类别顺序丢失
                var orderedFunctions = AllFunctions.Count > 0
                    ? AllFunctions.ToList()
                    : SystemFunctions.Concat(ApplicationFunctions).ToList();

                var config = new FunctionConfig
                {
                    AllFunctions = orderedFunctions,
                    SystemFunctions = orderedFunctions.Where(f => f.Category == "System").ToList(),
                    ApplicationFunctions = orderedFunctions.Where(f => f.Category == "Application").ToList()
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
                        // 读取 AllFunctions 以保留用户自定义顺序；若不存在则回退到旧结构
                        var orderedFunctions = new List<FunctionItem>();
                        var seenKeys = new HashSet<string>();

                        if (config.AllFunctions != null && config.AllFunctions.Count > 0)
                        {
                            foreach (var item in config.AllFunctions)
                            {
                                var key = BuildFunctionKey(item);
                                if (seenKeys.Add(key))
                                {
                                    orderedFunctions.Add(item);
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in config.SystemFunctions ?? Enumerable.Empty<FunctionItem>())
                            {
                                var key = BuildFunctionKey(item);
                                if (seenKeys.Add(key))
                                {
                                    orderedFunctions.Add(item);
                                }
                            }

                            foreach (var item in config.ApplicationFunctions ?? Enumerable.Empty<FunctionItem>())
                            {
                                var key = BuildFunctionKey(item);
                                if (seenKeys.Add(key))
                                {
                                    orderedFunctions.Add(item);
                                }
                            }
                        }

                        ApplyOrderedFunctions(orderedFunctions);
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

            RefreshAllFunctions();
            SaveFunctions();
        }

        /// <summary>
        /// 将有序列表应用到三个集合，保持顺序一致
        /// </summary>
        private void ApplyOrderedFunctions(IEnumerable<FunctionItem> orderedFunctions)
        {
            SystemFunctions.Clear();
            ApplicationFunctions.Clear();
            AllFunctions.Clear();

            foreach (var item in orderedFunctions)
            {
                AllFunctions.Add(item);

                if (item.Category == "System")
                {
                    SystemFunctions.Add(item);
                }
                else
                {
                    ApplicationFunctions.Add(item);
                }
            }
        }

        /// <summary>
        /// 生成用于判定唯一性的键，确保跨类别顺序一致性
        /// </summary>
        private static string BuildFunctionKey(FunctionItem item)
        {
            if (item == null) return string.Empty;

            string type = item.FunctionType ?? string.Empty;
            string category = item.Category ?? string.Empty;

            if (type == "PhotoshopAction")
            {
                return $"{type}|{category}|{item.ActionSetName}|{item.ActionName}";
            }

            return $"{type}|{category}|{item.Command}|{item.Name}";
        }

        private class FunctionConfig
        {
            public List<FunctionItem> AllFunctions { get; set; } = new();
            public List<FunctionItem> SystemFunctions { get; set; } = new();
            public List<FunctionItem> ApplicationFunctions { get; set; } = new();
        }
    }
}
