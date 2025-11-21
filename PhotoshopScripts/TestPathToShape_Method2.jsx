// 测试：路径转形状（模拟点击"形状"按钮）
// 基于Adobe官方API

alert('开始测试：路径转形状');

try {
    if (app.documents.length === 0) {
        alert('❌ 请先打开文档');
    } else if (app.activeDocument.pathItems.length === 0) {
        alert('❌ 请先用钢笔工具创建路径');
    } else {
        alert('[1/2] 将路径转换为形状图层...');
        
        // 方法：模拟点击顶部"形状"按钮
        // 这会将当前工作路径转换为形状图层
        var desc1 = new ActionDescriptor();
        var ref1 = new ActionReference();
        ref1.putClass(stringIDToTypeID('contentLayer'));
        desc1.putReference(charIDToTypeID('null'), ref1);
        
        var desc2 = new ActionDescriptor();
        var desc3 = new ActionDescriptor();
        desc3.putDouble(charIDToTypeID('Rd  '), 0);
        desc3.putDouble(charIDToTypeID('Grn '), 0);
        desc3.putDouble(charIDToTypeID('Bl  '), 0);
        desc2.putObject(charIDToTypeID('Clr '), charIDToTypeID('RGBC'), desc3);
        desc1.putObject(charIDToTypeID('Type'), stringIDToTypeID('solidColorLayer'), desc2);
        
        executeAction(charIDToTypeID('Mk  '), desc1, DialogModes.NO);
        
        alert('[2/2] 设置羽化值为0.5...');
        
        // 设置形状图层的羽化
        var desc4 = new ActionDescriptor();
        var ref2 = new ActionReference();
        ref2.putEnumerated(charIDToTypeID('Lyr '), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
        desc4.putReference(charIDToTypeID('null'), ref2);
        
        var desc5 = new ActionDescriptor();
        desc5.putUnitDouble(stringIDToTypeID('featherRadius'), charIDToTypeID('#Pxl'), 0.5);
        desc4.putObject(charIDToTypeID('T   '), charIDToTypeID('Lyr '), desc5);
        
        executeAction(charIDToTypeID('setd'), desc4, DialogModes.NO);
        
        alert('✅ 完成！\n已创建形状图层并设置羽化0.5');
    }
} catch(e) {
    alert('❌ 错误：' + e.message + '\n行号：' + e.line);
}

