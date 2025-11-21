// 另存为JPG - 修复版本
// 功能：另存为JPG格式（防覆盖模式）

if (app.documents.length === 0) {
    alert("❌ 请先打开一个文档");
} else {
    try {
        var doc = app.activeDocument;
        var docName = doc.name;
        var baseName = docName.replace(/\.[^\.]+$/, '');
        
        // 确定保存路径
        var savePath;
        try {
            savePath = doc.path;
        } catch(pe) {
            savePath = Folder.desktop;
        }
        
        // 查找不存在的文件名（防覆盖）
        var jpgFile;
        var counter = 0;
        do {
            var fileName = baseName + (counter > 0 ? '_' + counter : '') + '.jpg';
            jpgFile = new File(savePath + '/' + fileName);
            if (!jpgFile.exists) break;
            counter++;
        } while (counter < 100);
        
        // 使用JPG选项保存
        var jpgSaveOptions = new JPEGSaveOptions();
        jpgSaveOptions.quality = 12; // 最高质量
        jpgSaveOptions.embedColorProfile = true;
        jpgSaveOptions.formatOptions = FormatOptions.STANDARDBASELINE;
        jpgSaveOptions.matte = MatteType.NONE;
        
        doc.saveAs(jpgFile, jpgSaveOptions, true);
        
        alert("✅ 已保存为JPG格式\n位置: " + jpgFile.fsName);
    } catch(e) {
        alert("❌ 保存失败: " + e.message);
    }
}

