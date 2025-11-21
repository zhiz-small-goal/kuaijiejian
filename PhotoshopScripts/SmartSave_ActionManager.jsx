// 智能保存 - 纯ActionManager方法（最快）
// 使用ScriptingListener录制的标准保存代码

if(app.documents.length>0){
    try{
        // 方法1: 直接保存
        executeAction(charIDToTypeID('save'),undefined,DialogModes.NO);
    }catch(e){
        // 方法2: Save As TIF using Action Manager
        try{
            var doc=app.activeDocument;
            var docName=doc.name;
            var baseName=docName.replace(/\.[^\.]+$/,'');
            var savePath;
            try{savePath=doc.path;}catch(pe){savePath=Folder.desktop;}
            var tifFile=new File(savePath+'/'+baseName+'.tif');
            
            // 使用Action Manager的标准Save As方法
            var desc1=new ActionDescriptor();
            desc1.putPath(charIDToTypeID('In  '),tifFile);
            
            // TIF格式描述符
            var desc2=new ActionDescriptor();
            desc2.putEnumerated(charIDToTypeID('Inte'),charIDToTypeID('Inte'),charIDToTypeID('Prtr'));
            desc2.putEnumerated(charIDToTypeID('Cmpr'),charIDToTypeID('TIFFEncoding'),charIDToTypeID('TIFFLZW'));
            desc2.putBoolean(charIDToTypeID('LyrC'),true);
            desc2.putBoolean(stringIDToTypeID('saveImagePyramid'),false);
            desc1.putObject(charIDToTypeID('As  '),charIDToTypeID('TIFF'),desc2);
            
            desc1.putBoolean(charIDToTypeID('LwCs'),true);
            desc1.putBoolean(charIDToTypeID('Cpy '),false);
            executeAction(charIDToTypeID('save'),desc1,DialogModes.NO);
        }catch(e2){}
    }
}

