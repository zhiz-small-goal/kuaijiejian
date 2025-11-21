// 安全保存 - 修复版本
// 功能：已保存文档直接保存，未保存文档自动保存为TIF（防覆盖）

if (app.documents.length === 0) {
    alert("❌ 请先打开一个文档");
} else {
    try {
        // 尝试直接保存
        var desc = new ActionDescriptor();
        executeAction(charIDToTypeID('save'), desc, DialogModes.NO);
        alert("✅ 文档已保存");
    } catch(e) {
        // 直接保存失败，说明是未保存的新文档
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
            var tifFile;
            var counter = 0;
            do {
                var fileName = baseName + (counter > 0 ? '_' + counter : '') + '.tif';
                tifFile = new File(savePath + '/' + fileName);
                if (!tifFile.exists) break;
                counter++;
            } while (counter < 100);
            
            // 使用TIF选项保存
            var tiffSaveOptions = new TiffSaveOptions();
            tiffSaveOptions.imageCompression = TIFFEncoding.TIFFLZW;
            tiffSaveOptions.layers = true;
            
            doc.saveAs(tifFile, tiffSaveOptions, true);
            
            alert("✅ 已保存为TIF格式\n位置: " + tifFile.fsName);
        } catch(se) {
            alert("❌ 保存失败: " + se.message);
        }
    }
}

