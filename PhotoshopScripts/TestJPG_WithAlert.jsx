// 另存为JPG - 带提示测试版

if (app.documents.length === 0) {
    alert("❌ 没有打开的文档");
} else {
    alert("开始执行另存为JPG...");
    
    try {
        var d = app.activeDocument;
        var n = d.name;
        var b = n.replace(/\.[^\.]+$/, '');
        
        var p;
        try {
            p = d.path;
        } catch(e) {
            p = Folder.desktop;
        }
        
        var f, c = 0;
        do {
            var fn = b + (c > 0 ? '_' + c : '') + '.jpg';
            f = new File(p + '/' + fn);
            if (!f.exists) break;
            c++;
        } while (c < 100);
        
        alert("准备保存到: " + f.fsName);
        
        var o = new JPEGSaveOptions();
        o.quality = 12;
        o.embedColorProfile = true;
        o.formatOptions = FormatOptions.STANDARDBASELINE;
        o.matte = MatteType.NONE;
        
        d.saveAs(f, o, true);
        
        alert("✅ 保存成功！\n文件位置: " + f.fsName);
    } catch(e) {
        alert("❌ 保存失败: " + e.message);
    }
}

