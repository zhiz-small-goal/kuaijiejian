// Adobe Photoshop 脚本 - 栅格化图层（ActionManager版本，完全静默）
// 基于 Adobe Photoshop Scripting Guide 官方文档
// 使用 ActionManager API 确保不弹出任何对话框
// ActionID: RstL (Rasterize Layer)

#target photoshop

try {
    if (app.documents.length > 0 && app.activeDocument.activeLayer) {
        var idRstL = charIDToTypeID('RstL');
        var desc = new ActionDescriptor();
        var ref = new ActionReference();
        ref.putEnumerated(charIDToTypeID('Lyr '), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
        desc.putReference(charIDToTypeID('null'), ref);
        executeAction(idRstL, desc, DialogModes.NO);
    }
} catch (e) {
    // 静默失败：无法栅格化时不做任何处理
}
