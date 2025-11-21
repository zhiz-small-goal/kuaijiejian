// 智能保存 - 基于测试结果的最佳方案
// 使用 doc.saveAs() 方法（测试2已验证成功）

#target photoshop

if (app.documents.length == 0) {
    // 静默失败，不弹窗
} else {
    try {
        // 方法1: 尝试直接保存（已保存过的文档）
        var desc = new ActionDescriptor();
        executeAction(charIDToTypeID('save'), desc, DialogModes.NO);
    } catch(e) {
        // 方法2: 使用 doc.saveAs() 保存为TIF（测试2验证成功）
        try {
            var doc = app.activeDocument;
            var docName = doc.name;
            var baseName = docName.replace(/\.[^\.]+$/, '');
            
            var savePath;
            try {
                savePath = doc.path;
            } catch(pe) {
                savePath = Folder.desktop;
            }
            
            var tifFile = new File(savePath + '/' + baseName + '.tif');
            
            var tiffOpts = new TiffSaveOptions();
            tiffOpts.imageCompression = TIFFEncoding.TIFFLZW;
            tiffOpts.layers = true;
            
            doc.saveAs(tifFile, tiffOpts, true);
        } catch(se) {
            // 静默失败
        }
    }
}
