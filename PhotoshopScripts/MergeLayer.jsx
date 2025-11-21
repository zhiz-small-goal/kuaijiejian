// Adobe Photoshop 脚本 - 合并图层（ActionManager版本，完全静默）
// 基于 Adobe Photoshop Scripting Guide 官方文档
// 使用 ActionManager API 确保不弹出任何对话框
// ActionID: Mrg2 (Merge Down)

#target photoshop

try {
    if (app.documents.length > 0) {
        var idMrg2 = charIDToTypeID('Mrg2');
        executeAction(idMrg2, undefined, DialogModes.NO);
    }
} catch (e) {
    // 静默失败：操作不可用时不做任何处理（例如只有一个图层时）
}
