// Adobe Photoshop 脚本 - 拼合图像（ActionManager版本，完全静默）
// 基于 Adobe Photoshop Scripting Guide 官方文档
// 使用 ActionManager API 确保不弹出任何对话框
// ActionID: FltI (Flatten Image)

#target photoshop

try {
    if (app.documents.length > 0) {
        var idFltI = charIDToTypeID('FltI');
        executeAction(idFltI, undefined, DialogModes.NO);
    }
} catch (e) {
    // 静默失败：操作不可用时不做任何处理
}
