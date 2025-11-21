// 测试三个保存功能的脚本
// 这是从配置文件中提取的实际执行代码

alert("=== 开始测试三个保存功能 ===");

// 测试1: 智能保存
alert("【测试1】智能保存");
try{
    var desc=new ActionDescriptor();
    executeAction(charIDToTypeID('save'),desc,DialogModes.NO);
    alert("✓ 智能保存: 文档已保存（直接保存成功）");
}catch(e){
    alert("智能保存: 直接保存失败，尝试另存为TIF\n错误: " + e.message);
    try{
        var doc=app.activeDocument,docName=doc.name,baseName=docName.replace(/\.[^\.]+$/,''),savePath;
        alert("文档名: " + docName + "\n基础名: " + baseName);
        try{
            savePath=doc.path;
            alert("文档路径: " + savePath.fsName);
        }catch(pe){
            savePath=Folder.desktop;
            alert("文档未保存，使用桌面: " + savePath.fsName);
        }
        var tifFile=new File(savePath+'/'+baseName+'.tif');
        alert("准备保存为: " + tifFile.fsName);
        var saveDesc=new ActionDescriptor();
        saveDesc.putPath(charIDToTypeID('In  '),tifFile);
        saveDesc.putClass(charIDToTypeID('As  '),charIDToTypeID('TIFF'));
        executeAction(charIDToTypeID('save'),saveDesc,DialogModes.NO);
        alert("✓ 智能保存: 已保存为TIF");
    }catch(se){
        alert("✗ 智能保存失败: " + se.message);
    }
}

alert("=== 测试完成 ===");

