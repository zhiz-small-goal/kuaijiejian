// 尝试通过矢量蒙版设置羽化
alert('测试：通过矢量蒙版设置羽化');

try {
    var doc = app.activeDocument;
    
    alert('[方法1] 修改矢量蒙版属性');
    try {
        var desc1 = new ActionDescriptor();
        var ref1 = new ActionReference();
        ref1.putEnumerated(stringIDToTypeID('path'), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
        desc1.putReference(charIDToTypeID('null'), ref1);
        
        var desc2 = new ActionDescriptor();
        desc2.putUnitDouble(stringIDToTypeID('featherRadius'), charIDToTypeID('#Pxl'), 0.5);
        desc1.putObject(charIDToTypeID('T   '), stringIDToTypeID('path'), desc2);
        
        executeAction(charIDToTypeID('setd'), desc1, DialogModes.NO);
        alert('✅ 方法1执行完成');
    } catch(e1) {
        alert('❌ 方法1失败：' + e1.message);
    }
    
    alert('[方法2] 通过 vectorMaskFeather');
    try {
        var desc3 = new ActionDescriptor();
        var ref2 = new ActionReference();
        ref2.putEnumerated(charIDToTypeID('Lyr '), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
        desc3.putReference(charIDToTypeID('null'), ref2);
        
        var desc4 = new ActionDescriptor();
        var desc5 = new ActionDescriptor();
        desc5.putUnitDouble(stringIDToTypeID('featherRadius'), charIDToTypeID('#Pxl'), 0.5);
        desc4.putObject(stringIDToTypeID('vectorMask'), stringIDToTypeID('vectorMask'), desc5);
        desc3.putObject(charIDToTypeID('T   '), charIDToTypeID('Lyr '), desc4);
        
        executeAction(charIDToTypeID('setd'), desc3, DialogModes.NO);
        alert('✅ 方法2执行完成');
    } catch(e2) {
        alert('❌ 方法2失败：' + e2.message);
    }
    
    alert('[方法3] 使用 shapeFeatherEdge');
    try {
        var desc6 = new ActionDescriptor();
        var ref3 = new ActionReference();
        ref3.putEnumerated(charIDToTypeID('Lyr '), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
        desc6.putReference(charIDToTypeID('null'), ref3);
        desc6.putUnitDouble(stringIDToTypeID('shapeFeatherEdge'), charIDToTypeID('#Pxl'), 0.5);
        
        executeAction(charIDToTypeID('setd'), desc6, DialogModes.NO);
        alert('✅ 方法3执行完成');
    } catch(e3) {
        alert('❌ 方法3失败：' + e3.message);
    }
    
    alert('测试完成！\n请检查属性面板的羽化值');
    
} catch(e) {
    alert('❌ 总体错误：' + e.message + '\n行号：' + e.line);
}

