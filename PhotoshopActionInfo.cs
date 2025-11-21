using System;

namespace Kuaijiejian
{
    /// <summary>
    /// Photoshop 动作信息
    /// 用于存储从 Adobe Photoshop 动作面板获取的动作数据
    /// </summary>
    public class PhotoshopActionInfo
    {
        /// <summary>
        /// 动作集名称
        /// </summary>
        public string ActionSetName { get; set; } = string.Empty;

        /// <summary>
        /// 动作名称
        /// </summary>
        public string ActionName { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称（用于UI显示）
        /// 格式：动作集名称 - 动作名称
        /// </summary>
        public string DisplayName => $"{ActionSetName} - {ActionName}";

        public override string ToString()
        {
            return DisplayName;
        }
    }
}

