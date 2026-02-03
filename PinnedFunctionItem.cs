using System;

namespace Kuaijiejian
{
    /// <summary>
    /// 固定在顶端的内置按钮（不可拖拽/不可删除/不参与持久化）
    /// 通过继承 FunctionItem 复用现有按钮模板与点击路由。
    /// </summary>
    public enum PinnedFunctionKind
    {
        BatchDelete,
        ClearAll,
        AddLayer,
        AddAction
    }

    /// <summary>
    /// 固定按钮：继承 FunctionItem，但仅用于 UI 展示与内置行为分发
    /// </summary>
    public sealed class PinnedFunctionItem : FunctionItem
    {
        public PinnedFunctionKind Kind { get; private set; }

        private PinnedFunctionItem(PinnedFunctionKind kind, string name)
        {
            Kind = kind;
            Name = name;
            Category = "Pinned";
            FunctionType = "Pinned";
            Hotkey = "";
            Command = "";
        }

        public static PinnedFunctionItem Create(PinnedFunctionKind kind, string name)
        {
            return new PinnedFunctionItem(kind, name);
        }
    }
}
