// 调试版本的JPG保存 - 会显示执行过程

alert("开始执行JPG保存");

try {
    alert("步骤1: 获取文档");
    var doc = app.activeDocument;
    var docName = doc.name;
    var baseName = docName.replace(/\.[^\.]+$/, '');
    
    alert("步骤2: 文档名称 = " + docName + "\n基础名称 = " + baseName);
    
    var savePath;
    try {
        savePath = doc.path;
        alert("步骤3: 文档路径 = " + savePath.fsName);
    } catch(pe) {
        savePath = Folder.desktop;
        alert("步骤3: 文档未保存，使用桌面 = " + savePath.fsName);
    }
    
    var jpgFile, counter = 0;
    do {
        var fileName = baseName + (counter > 0 ? '_' + counter : '') + '.jpg';
        jpgFile = new File(savePath + '/' + fileName);
        alert("步骤4: 尝试文件名 = " + jpgFile.fsName + "\n是否存在 = " + jpgFile.exists);
        if (!jpgFile.exists) break;
        counter++;
    } while (counter < 100);
    
    alert("步骤5: 最终文件名 = " + jpgFile.fsName);
    
    var saveDesc = new ActionDescriptor();
    saveDesc.putPath(charIDToTypeID('In  '), jpgFile);
    saveDesc.putClass(charIDToTypeID('As  '), charIDToTypeID('JPEG'));
    
    alert("步骤6: 开始执行保存...");
    executeAction(charIDToTypeID('save'), saveDesc, DialogModes.NO);
    
    alert("✓ 成功保存JPG！\n文件位置: " + jpgFile.fsName);
    
} catch(e) {
    alert("✗ 错误发生！\n错误信息: " + e.message + "\n错误行号: " + e.line);
}

