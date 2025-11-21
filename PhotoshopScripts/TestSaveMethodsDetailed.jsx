// 详细测试所有保存方法 - 找出最有效的方法

if(app.documents.length===0){
    alert("❌ 请先打开一个文档");
}else{
    var results="";
    var doc=app.activeDocument;
    var docName=doc.name;
    var baseName=docName.replace(/\.[^\.]+$/,'');
    
    var savePath;
    var isSaved=false;
    try{
        savePath=doc.path;
        isSaved=true;
    }catch(e){
        savePath=Folder.desktop;
    }
    
    results+="📄 文档: "+docName+"\n";
    results+="📁 路径: "+(isSaved?savePath.fsName:"未保存")+"\n\n";
    
    // 测试1: 直接保存
    results+="【测试1】直接保存 (executeAction save)\n";
    try{
        executeAction(charIDToTypeID('save'),undefined,DialogModes.NO);
        results+="✅ 成功\n\n";
    }catch(e){
        results+="❌ 失败: "+e.message+"\n\n";
    }
    
    // 测试2: TIF - saveAs方法
    results+="【测试2】TIF - doc.saveAs()\n";
    try{
        var tifFile1=new File(savePath+'/'+baseName+'_test_saveAs.tif');
        var tiffOpts=new TiffSaveOptions();
        tiffOpts.imageCompression=TIFFEncoding.TIFFLZW;
        tiffOpts.layers=true;
        doc.saveAs(tifFile1,tiffOpts,true);
        results+="✅ 成功: "+tifFile1.fsName+"\n\n";
    }catch(e){
        results+="❌ 失败: "+e.message+"\n\n";
    }
    
    // 测试3: TIF - ActionManager简单方法
    results+="【测试3】TIF - ActionManager简单\n";
    try{
        var tifFile2=new File(savePath+'/'+baseName+'_test_AM_simple.tif');
        var desc=new ActionDescriptor();
        desc.putPath(charIDToTypeID('In  '),tifFile2);
        desc.putClass(charIDToTypeID('As  '),charIDToTypeID('TIFF'));
        executeAction(charIDToTypeID('save'),desc,DialogModes.NO);
        results+="✅ 成功: "+tifFile2.fsName+"\n\n";
    }catch(e){
        results+="❌ 失败: "+e.message+"\n\n";
    }
    
    // 测试4: TIF - ActionManager完整方法
    results+="【测试4】TIF - ActionManager完整\n";
    try{
        var tifFile3=new File(savePath+'/'+baseName+'_test_AM_full.tif');
        var desc1=new ActionDescriptor();
        desc1.putPath(charIDToTypeID('In  '),tifFile3);
        var desc2=new ActionDescriptor();
        desc2.putEnumerated(charIDToTypeID('Inte'),charIDToTypeID('Inte'),charIDToTypeID('Prtr'));
        desc2.putEnumerated(charIDToTypeID('Cmpr'),charIDToTypeID('TIFFEncoding'),charIDToTypeID('TIFFLZW'));
        desc2.putBoolean(charIDToTypeID('LyrC'),true);
        desc1.putObject(charIDToTypeID('As  '),charIDToTypeID('TIFF'),desc2);
        desc1.putBoolean(charIDToTypeID('LwCs'),true);
        executeAction(charIDToTypeID('save'),desc1,DialogModes.NO);
        results+="✅ 成功: "+tifFile3.fsName+"\n\n";
    }catch(e){
        results+="❌ 失败: "+e.message+"\n\n";
    }
    
    // 测试5: JPG - saveAs方法（质量8）
    results+="【测试5】JPG - doc.saveAs() 质量8\n";
    try{
        var jpgFile1=new File(savePath+'/'+baseName+'_test_q8.jpg');
        var jpgOpts=new JPEGSaveOptions();
        jpgOpts.quality=8;
        jpgOpts.embedColorProfile=true;
        doc.saveAs(jpgFile1,jpgOpts,true);
        results+="✅ 成功: "+jpgFile1.fsName+"\n\n";
    }catch(e){
        results+="❌ 失败: "+e.message+"\n\n";
    }
    
    // 测试6: JPG - saveAs方法（质量12）
    results+="【测试6】JPG - doc.saveAs() 质量12\n";
    try{
        var jpgFile2=new File(savePath+'/'+baseName+'_test_q12.jpg');
        var jpgOpts2=new JPEGSaveOptions();
        jpgOpts2.quality=12;
        jpgOpts2.embedColorProfile=true;
        doc.saveAs(jpgFile2,jpgOpts2,true);
        results+="✅ 成功: "+jpgFile2.fsName+"\n\n";
    }catch(e){
        results+="❌ 失败: "+e.message+"\n\n";
    }
    
    alert("🧪 保存方法测试报告\n\n"+results);
}

