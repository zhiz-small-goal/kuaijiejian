// 方法：在创建形状图层时直接设置羽化
alert('测试：创建形状图层时设置羽化');

try {
    var doc = app.activeDocument;
    
    alert('[执行] 路径→形状（带羽化0.5）');
    
    // 在创建contentLayer时就设置羽化
    var idMk = charIDToTypeID('Mk  ');
    var desc1 = new ActionDescriptor();
    var idnull = charIDToTypeID('null');
    var ref1 = new ActionReference();
    var idcontentLayer = stringIDToTypeID('contentLayer');
    ref1.putClass(idcontentLayer);
    desc1.putReference(idnull, ref1);
    
    var idUsng = charIDToTypeID('Usng');
    var desc2 = new ActionDescriptor();
    
    // 设置图层类型为纯色
    var idType = charIDToTypeID('Type');
    var desc3 = new ActionDescriptor();
    var idClr = charIDToTypeID('Clr ');
    var desc4 = new ActionDescriptor();
    desc4.putDouble(charIDToTypeID('Rd  '), 0);
    desc4.putDouble(charIDToTypeID('Grn '), 0);
    desc4.putDouble(charIDToTypeID('Bl  '), 0);
    desc3.putObject(idClr, charIDToTypeID('RGBC'), desc4);
    
    // ⭐ 关键：在这里设置羽化
    desc3.putUnitDouble(stringIDToTypeID('shapingFeatherRadius'), charIDToTypeID('#Pxl'), 0.5);
    
    desc2.putObject(idType, stringIDToTypeID('solidColorLayer'), desc3);
    desc1.putObject(idUsng, idcontentLayer, desc2);
    
    executeAction(idMk, desc1, DialogModes.NO);
    
    alert('✅ 完成！\n请检查属性面板的羽化值');
    
} catch(e) {
    alert('❌ 错误：' + e.message + '\n行号：' + e.line);
}

