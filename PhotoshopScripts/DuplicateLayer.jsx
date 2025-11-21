// Adobe Photoshop 脚本 - 复制当前图层
// 基于 Adobe Photoshop Scripting Guide 官方文档
// 使用 ActionManager API 避免对话框
// 参考：ScriptListener 记录的代码

#target photoshop

if (app.documents.length > 0) {
    // 使用 ActionManager API - 官方推荐的底层API
    // 复制当前选中的图层，不显示任何对话框
    var desc = new ActionDescriptor();
    var ref = new ActionReference();
    ref.putEnumerated(charIDToTypeID('Lyr '), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
    desc.putReference(charIDToTypeID('null'), ref);
    // DialogModes.NO 确保不显示任何对话框
    executeAction(charIDToTypeID('Dplc'), desc, DialogModes.NO);
}

