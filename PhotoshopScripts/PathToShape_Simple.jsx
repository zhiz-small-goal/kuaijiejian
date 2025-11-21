// 简化版：路径转形状（带羽化）
alert('开始：路径→形状');

try {
    var doc = app.activeDocument;
    
    // Step 1: 将路径转为形状图层（模拟点击"形状"按钮）
    alert('[1/2] 转换为形状图层');
    var idMk = charIDToTypeID('Mk  ');
    var desc1 = new ActionDescriptor();
    var idnull = charIDToTypeID('null');
    var ref1 = new ActionReference();
    var idcontentLayer = stringIDToTypeID('contentLayer');
    ref1.putClass(idcontentLayer);
    desc1.putReference(idnull, ref1);
    var idUsng = charIDToTypeID('Usng');
    var desc2 = new ActionDescriptor();
    var idType = charIDToTypeID('Type');
    var desc3 = new ActionDescriptor();
    var idClr = charIDToTypeID('Clr ');
    var desc4 = new ActionDescriptor();
    var idRd = charIDToTypeID('Rd  ');
    desc4.putDouble(idRd, 0.000000);
    var idGrn = charIDToTypeID('Grn ');
    desc4.putDouble(idGrn, 0.000000);
    var idBl = charIDToTypeID('Bl  ');
    desc4.putDouble(idBl, 0.000000);
    var idRGBC = charIDToTypeID('RGBC');
    desc3.putObject(idClr, idRGBC, desc4);
    var idsolidColorLayer = stringIDToTypeID('solidColorLayer');
    desc2.putObject(idType, idsolidColorLayer, desc3);
    desc1.putObject(idUsng, idcontentLayer, desc2);
    executeAction(idMk, desc1, DialogModes.NO);
    
    // Step 2: 设置羽化为0.5
    alert('[2/2] 设置羽化0.5');
    var idsetd = charIDToTypeID('setd');
    var desc5 = new ActionDescriptor();
    var ref2 = new ActionReference();
    var idLyr = charIDToTypeID('Lyr ');
    var idOrdn = charIDToTypeID('Ordn');
    var idTrgt = charIDToTypeID('Trgt');
    ref2.putEnumerated(idLyr, idOrdn, idTrgt);
    desc5.putReference(idnull, ref2);
    var idT = charIDToTypeID('T   ');
    var desc6 = new ActionDescriptor();
    var idfeatherRadius = stringIDToTypeID('featherRadius');
    var idPxl = charIDToTypeID('#Pxl');
    desc6.putUnitDouble(idfeatherRadius, idPxl, 0.500000);
    desc5.putObject(idT, idLyr, desc6);
    executeAction(idsetd, desc5, DialogModes.NO);
    
    alert('✅ 完成！');
    
} catch(e) {
    alert('❌ 错误：' + e.message + '\n行号：' + e.line);
}

