// 诊断保存功能 - 显示详细错误信息
// 请先在Photoshop中打开一个文档，然后执行此脚本

alert("📋 开始诊断保存功能\n\n请确保已打开一个文档");

if (app.documents.length === 0) {
    alert("❌ 错误：没有打开的文档！\n\n请先在Photoshop中打开或创建一个文档");
} else {
    var doc = app.activeDocument;
    var results = "";
    
    results += "📄 当前文档信息：\n";
    results += "名称: " + doc.name + "\n";
    
    // 检查文档是否已保存
    var isSaved = false;
    var docPath = "";
    try {
        docPath = doc.path.fsName;
        isSaved = true;
        results += "路径: " + docPath + "\n";
        results += "状态: ✅ 已保存过\n\n";
    } catch(e) {
        results += "路径: (未保存)\n";
        results += "状态: ⚠️ 新文档，从未保存\n\n";
    }
    
    // 测试1: 直接保存 (Ctrl+S)
    results += "【测试1】直接保存 (charIDToTypeID('save'))：\n";
    try {
        var desc = new ActionDescriptor();
        executeAction(charIDToTypeID('save'), desc, DialogModes.NO);
        results += "✅ 成功！文档已保存\n\n";
    } catch(e) {
        results += "❌ 失败: " + e.message + "\n";
        results += "   原因: " + (isSaved ? "未知错误" : "文档未保存过，需要指定路径") + "\n\n";
    }
    
    // 测试2: 另存为TIF
    results += "【测试2】另存为TIF：\n";
    try {
        var baseName = doc.name.replace(/\.[^\.]+$/, '');
        var savePath = isSaved ? doc.path : Folder.desktop;
        var tifFile = new File(savePath + '/' + baseName + '_test.tif');
        
        results += "目标文件: " + tifFile.fsName + "\n";
        
        var saveDesc = new ActionDescriptor();
        saveDesc.putPath(charIDToTypeID('In  '), tifFile);
        saveDesc.putClass(charIDToTypeID('As  '), charIDToTypeID('TIFF'));
        executeAction(charIDToTypeID('save'), saveDesc, DialogModes.NO);
        
        results += "✅ 成功！文件已保存\n\n";
    } catch(e) {
        results += "❌ 失败: " + e.message + "\n\n";
    }
    
    // 测试3: 另存为JPG (使用4字符ID)
    results += "【测试3】另存为JPG (方法1: charIDToTypeID)：\n";
    try {
        var baseName = doc.name.replace(/\.[^\.]+$/, '');
        var savePath = isSaved ? doc.path : Folder.desktop;
        var jpgFile = new File(savePath + '/' + baseName + '_test1.jpg');
        
        results += "目标文件: " + jpgFile.fsName + "\n";
        
        var saveDesc = new ActionDescriptor();
        saveDesc.putPath(charIDToTypeID('In  '), jpgFile);
        saveDesc.putClass(charIDToTypeID('As  '), charIDToTypeID('JPEG'));
        executeAction(charIDToTypeID('save'), saveDesc, DialogModes.NO);
        
        results += "✅ 成功！\n\n";
    } catch(e) {
        results += "❌ 失败: " + e.message + "\n\n";
    }
    
    // 测试4: 另存为JPG (使用stringID)
    results += "【测试4】另存为JPG (方法2: stringIDToTypeID)：\n";
    try {
        var baseName = doc.name.replace(/\.[^\.]+$/, '');
        var savePath = isSaved ? doc.path : Folder.desktop;
        var jpgFile = new File(savePath + '/' + baseName + '_test2.jpg');
        
        results += "目标文件: " + jpgFile.fsName + "\n";
        
        var saveDesc = new ActionDescriptor();
        saveDesc.putPath(stringIDToTypeID('in'), jpgFile);
        saveDesc.putClass(stringIDToTypeID('as'), stringIDToTypeID('JPEG'));
        executeAction(stringIDToTypeID('save'), saveDesc, DialogModes.NO);
        
        results += "✅ 成功！\n\n";
    } catch(e) {
        results += "❌ 失败: " + e.message + "\n\n";
    }
    
    alert("📊 诊断结果：\n\n" + results);
}

