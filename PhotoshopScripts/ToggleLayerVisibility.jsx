// Adobe Photoshop 脚本 - 切换图层可见性（ActionManager版本，完全静默）
// 基于 Adobe Photoshop Scripting Guide 官方文档
// 使用 ActionManager API 确保不弹出任何对话框
// ActionID: Shw  (Show) / Hd   (Hide)

#target photoshop

try {
    if (app.documents.length > 0 && app.activeDocument.activeLayer) {
        var layer = app.activeDocument.activeLayer;
        var idAction = layer.visible ? charIDToTypeID('Hd  ') : charIDToTypeID('Shw ');
        var desc = new ActionDescriptor();
        var list = new ActionList();
        var ref = new ActionReference();
        ref.putEnumerated(charIDToTypeID('Lyr '), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
        list.putReference(ref);
        desc.putList(charIDToTypeID('null'), list);
        executeAction(idAction, desc, DialogModes.NO);
    }
} catch (e) {
    // 静默失败：无法切换可见性时不做任何处理
}
