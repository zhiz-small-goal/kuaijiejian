// 测试文件操作功能

#target photoshop

if (app.documents.length === 0) {
    alert("请先打开一个文档！");
} else {
    var results = "";
    
    // 测试1: 保存 (Ctrl+S)
    try {
        var desc1 = new ActionDescriptor();
        executeAction(charIDToTypeID('save'), desc1, DialogModes.NO);
        results += "✅ 保存成功：charIDToTypeID('save')\n";
    } catch(e) {
        results += "❌ 保存失败：" + e.message + "\n";
    }
    
    // 测试2: 另存为 (Ctrl+Shift+S)
    try {
        executeAction(charIDToTypeID('SvAs'), undefined, DialogModes.ALL);
        results += "✅ 另存为成功：charIDToTypeID('SvAs')\n";
    } catch(e) {
        results += "❌ 另存为失败：" + e.message + "\n";
    }
    
    // 测试3: 导出为
    try {
        executeAction(stringIDToTypeID('exportSelectionAsFileTypePressed'), undefined, DialogModes.ALL);
        results += "✅ 导出为成功：stringIDToTypeID('exportSelectionAsFileTypePressed')\n";
    } catch(e) {
        results += "❌ 导出为失败（尝试方法1）：" + e.message + "\n";
        
        // 尝试方法2
        try {
            executeAction(stringIDToTypeID('exportDocument'), undefined, DialogModes.ALL);
            results += "✅ 导出为成功（方法2）：stringIDToTypeID('exportDocument')\n";
        } catch(e2) {
            results += "❌ 导出为失败（方法2）：" + e2.message + "\n";
        }
    }
    
    // 测试4: 关闭文档 (Ctrl+W) - 不保存
    try {
        var desc4 = new ActionDescriptor();
        desc4.putEnumerated(charIDToTypeID('Svng'), charIDToTypeID('YsN '), charIDToTypeID('N   '));
        executeAction(charIDToTypeID('Cls '), desc4, DialogModes.NO);
        results += "✅ 关闭文档成功：charIDToTypeID('Cls ') + 不保存\n";
    } catch(e) {
        results += "❌ 关闭文档失败：" + e.message + "\n";
    }
    
    // 测试5: 关闭所有文档
    try {
        executeAction(charIDToTypeID('ClsA'), undefined, DialogModes.NO);
        results += "✅ 关闭所有文档成功：charIDToTypeID('ClsA')\n";
    } catch(e) {
        results += "❌ 关闭所有文档失败：" + e.message + "\n";
    }
    
    alert("文件操作测试结果：\n\n" + results + "\n注意：部分功能会触发对话框是正常的！");
}


