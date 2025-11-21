// 安全保存 - 带提示测试版

if (app.documents.length === 0) {
    alert("❌ 没有打开的文档");
} else {
    alert("开始执行安全保存...");
    
    try {
        // 尝试直接保存
        executeAction(charIDToTypeID('save'), undefined, DialogModes.NO);
        alert("✅ 直接保存成功！");
    } catch(e) {
        alert("直接保存失败，尝试另存为TIF（防覆盖）...\n错误: " + e.message);
        
        try {
            var d = app.activeDocument;
            var n = d.name;
            var b = n.replace(/\.[^\.]+$/, '');
            
            var p;
            try {
                p = d.path;
            } catch(x) {
                p = Folder.desktop;
            }
            
            var f, c = 0;
            do {
                var fn = b + (c > 0 ? '_' + c : '') + '.tif';
                f = new File(p + '/' + fn);
                if (!f.exists) break;
                c++;
            } while (c < 100);
            
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

