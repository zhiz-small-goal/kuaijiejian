// 另存为JPG - 高速版本（质量8，速度快）

if(app.documents.length>0){
    try{
        var doc=app.activeDocument;
        var docName=doc.name;
        var baseName=docName.replace(/\.[^\.]+$/,'');
        var savePath;
        try{savePath=doc.path;}catch(pe){savePath=Folder.desktop;}
        
        var jpgFile,counter=0;
        do{
            var fileName=baseName+(counter>0?'_'+counter:'')+'.jpg';
            jpgFile=new File(savePath+'/'+fileName);
            if(!jpgFile.exists)break;
            counter++;
        }while(counter<100);
        
        // 使用质量8（平衡速度和质量）
        var jpgOpts=new JPEGSaveOptions();
        jpgOpts.quality=8;
        jpgOpts.embedColorProfile=true;
        jpgOpts.formatOptions=FormatOptions.STANDARDBASELINE;
        jpgOpts.matte=MatteType.NONE;
        doc.saveAs(jpgFile,jpgOpts,true);
    }catch(e){}
}

