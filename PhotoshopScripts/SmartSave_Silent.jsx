// 智能保存 - 静默版本（无弹窗）
try{var desc=new ActionDescriptor();executeAction(charIDToTypeID('save'),desc,DialogModes.NO);}catch(e){try{var doc=app.activeDocument,docName=doc.name,baseName=docName.replace(/\.[^\.]+$/,''),savePath;try{savePath=doc.path;}catch(pe){savePath=Folder.desktop;}var tifFile=new File(savePath+'/'+baseName+'.tif'),saveDesc=new ActionDescriptor();saveDesc.putPath(charIDToTypeID('In  '),tifFile);saveDesc.putClass(charIDToTypeID('As  '),charIDToTypeID('TIFF'));executeAction(charIDToTypeID('save'),saveDesc,DialogModes.NO);}catch(se){}}

