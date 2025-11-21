// 智能保存 - 带提示测试版

if (app.documents.length === 0) {
    alert("❌ 没有打开的文档");
} else {
    alert("开始执行智能保存...");
    
    try {
        // 尝试直接保存
        executeAction(charIDToTypeID('save'), undefined, DialogModes.NO);
        alert("✅ 直接保存成功！");
    } catch(e) {
        alert("直接保存失败，尝试另存为TIF...\n错误: " + e.message);
        
        try {
            var d = app.activeDocument;
            var n = d.name;
            var b = n.replace(/\.[^\.]+$/, '');
            
            var p;
            try {
                p = d.path;
                alert("文档路径: " + p.fsName);
            } catch(x) {
                p = Folder.desktop;
                alert("使用桌面路径: " + p.fsName);
            }
            
            var f = new File(p + '/' + b + '.tif');
            alert("准备保存到: " + f.fsName);
            
            var o = new TiffSaveOptions();
            o.imageCompression = TIFFEncoding.TIFFLZW;
            o.layers = true;
            
            d.saveAs(f, o, true);
            
            alert("✅ 保存成功！\n文件位置: " + f.fsName);
        } catch(x) {
            alert("❌ 保存失败: " + x.message);
        }
    }
}

