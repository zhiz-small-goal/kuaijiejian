// 智能保存 V2 - 使用多种方法尝试
// 方法1: 直接保存 → 方法2: saveAs with TiffSaveOptions → 方法3: 纯DOM API

if(app.documents.length>0){
    var success=false;
    
    // 方法1: 尝试直接保存（已保存过的文档）
    try{
        var desc=new ActionDescriptor();
        executeAction(charIDToTypeID('save'),desc,DialogModes.NO);
        success=true;
    }catch(e1){
        // 方法2: 尝试使用TiffSaveOptions
        try{
            var doc=app.activeDocument;
            var docName=doc.name;
            var baseName=docName.replace(/\.[^\.]+$/,'');
            var savePath;
            try{savePath=doc.path;}catch(pe){savePath=Folder.desktop;}
            var tifFile=new File(savePath+'/'+baseName+'.tif');
            
            var tiffOpts=new TiffSaveOptions();
            tiffOpts.imageCompression=TIFFEncoding.TIFFLZW;
            tiffOpts.layers=true;
            doc.saveAs(tifFile,tiffOpts,true);
            success=true;
        }catch(e2){
            // 方法3: 使用JPEG作为后备（如果TIF失败）
            try{
                var doc=app.activeDocument;
                var docName=doc.name;
                var baseName=docName.replace(/\.[^\.]+$/,'');
                var savePath;
                try{savePath=doc.path;}catch(pe){savePath=Folder.desktop;}
                var jpgFile=new File(savePath+'/'+baseName+'.jpg');
                
                var jpgOpts=new JPEGSaveOptions();
                jpgOpts.quality=10;
                jpgOpts.embedColorProfile=true;
                jpgOpts.formatOptions=FormatOptions.STANDARDBASELINE;
                doc.saveAs(jpgFile,jpgOpts,true);
                success=true;
            }catch(e3){}
        }
    }
}

