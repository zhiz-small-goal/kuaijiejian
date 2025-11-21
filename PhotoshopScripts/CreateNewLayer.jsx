// Adobe Photoshop 脚本 - 创建新图层
// 基于 Adobe Photoshop Scripting Guide 官方文档
// 使用 ActionManager API 避免新建图层对话框
// 参考：ScriptListener 记录的代码

#target photoshop

if (app.documents.length > 0) {
    // 使用 ActionManager API - 官方推荐的底层API
    var desc = new ActionDescriptor();
    var ref = new ActionReference();
    ref.putClass(charIDToTypeID('Lyr '));
    desc.putReference(charIDToTypeID('null'), ref);
    // DialogModes.NO 确保不显示任何对话框
    executeAction(charIDToTypeID('Mk  '), desc, DialogModes.NO);
}

