// 智能保存 - 修复版本
// 功能：已保存文档直接保存，未保存文档自动保存为TIF（覆盖同名）

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
            
            var tifFile = new File(savePath + '/' + baseName + '.tif');
            
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

