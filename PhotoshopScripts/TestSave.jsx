// 测试保存功能 (Ctrl+S)

#target photoshop

if (app.documents.length === 0) {
    alert("请先打开一个文档！");
} else {
    var results = "";
    
    // 测试保存
    try {
        var desc = new ActionDescriptor();
        executeAction(charIDToTypeID('save'), desc, DialogModes.NO);
        results += "✅ 保存成功：charIDToTypeID('save')\n";
        results += "文档已保存！";
    } catch(e) {
        results += "❌ 保存失败：" + e.message + "\n";
        results += "\n提示：如果是未保存过的新文档，会失败是正常的";
    }
    
    alert("保存测试结果：\n\n" + results);
}


