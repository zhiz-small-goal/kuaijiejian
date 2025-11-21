// 测试不同的羽化设置方法
alert('开始测试羽化设置');

try {
    var doc = app.activeDocument;
    var layer = doc.activeLayer;
    
    alert('当前图层类型：' + layer.typename + '\n图层名：' + layer.name);
    
    // 方法1：尝试直接设置属性（DOM API）
    alert('[方法1] 尝试 layer.featherEdges');
    try {
        if (typeof layer.featherEdges !== 'undefined') {
            layer.featherEdges = 0.5;
            alert('✅ 方法1成功');
        } else {
            alert('❌ 方法1：属性不存在');
        }
    } catch(e1) {
        alert('❌ 方法1失败：' + e1.message);
    }
    
    // 方法2：ActionManager - 使用 shapingFeatherRadius
    alert('[方法2] ActionManager - shapingFeatherRadius');
    try {
        var desc1 = new ActionDescriptor();
        var ref1 = new ActionReference();
        ref1.putEnumerated(charIDToTypeID('Lyr '), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
        desc1.putReference(charIDToTypeID('null'), ref1);
        
        var desc2 = new ActionDescriptor();
        desc2.putUnitDouble(stringIDToTypeID('shapingFeatherRadius'), charIDToTypeID('#Pxl'), 0.5);
        desc1.putObject(charIDToTypeID('T   '), charIDToTypeID('Lyr '), desc2);
        
        executeAction(charIDToTypeID('setd'), desc1, DialogModes.NO);
        alert('✅ 方法2执行完成（请检查图层属性）');
    } catch(e2) {
        alert('❌ 方法2失败：' + e2.message);
    }
    
    // 方法3：ActionManager - 使用 feather
    alert('[方法3] ActionManager - feather');
    try {
        var desc3 = new ActionDescriptor();
        var ref2 = new ActionReference();
        ref2.putEnumerated(charIDToTypeID('Lyr '), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
        desc3.putReference(charIDToTypeID('null'), ref2);
        
        var desc4 = new ActionDescriptor();
        desc4.putUnitDouble(charIDToTypeID('Fthr'), charIDToTypeID('#Pxl'), 0.5);
        desc3.putObject(charIDToTypeID('T   '), charIDToTypeID('Lyr '), desc4);
        
        executeAction(charIDToTypeID('setd'), desc3, DialogModes.NO);
        alert('✅ 方法3执行完成（请检查图层属性）');
    } catch(e3) {
        alert('❌ 方法3失败：' + e3.message);
    }
    
    alert('测试完成！\n\n请手动检查形状图层的属性面板\n看羽化值是否为0.5');
    
} catch(e) {
    alert('❌ 总体错误：' + e.message + '\n行号：' + e.line);
}

