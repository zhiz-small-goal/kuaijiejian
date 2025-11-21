// Adobe Photoshop 脚本 - 删除当前图层（ActionManager版本，完全静默）
// 基于 Adobe Photoshop Scripting Guide 官方文档
// 使用 ActionManager API 确保不弹出任何对话框
// ActionID: Dlt  (Delete)

#target photoshop

try {
    if (app.documents.length > 0 && app.activeDocument.activeLayer) {
        var idDlt = charIDToTypeID('Dlt ');
        var desc = new ActionDescriptor();
        var ref = new ActionReference();
        ref.putEnumerated(charIDToTypeID('Lyr '), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
        desc.putReference(charIDToTypeID('null'), ref);
        executeAction(idDlt, desc, DialogModes.NO);
    }
} catch (e) {
    // 静默失败：无法删除时不做任何处理（例如背景图层或最后一个图层）
}
