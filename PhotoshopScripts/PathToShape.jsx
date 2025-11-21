// 将路径转换为形状图层（羽化0.5）
try {
    if (app.documents.length === 0) {
        alert('请先打开一个文档');
    } else {
        var doc = app.activeDocument;
        
        // 检查是否有路径
        if (doc.pathItems.length === 0) {
            alert('当前文档没有路径\n请先使用钢笔工具创建路径');
        } else {
            // 从工作路径创建形状图层，设置羽化为0.5
            var desc = new ActionDescriptor();
            var ref = new ActionReference();
            ref.putProperty(charIDToTypeID('Path'), charIDToTypeID('WrPh')); // Work Path
            desc.putReference(charIDToTypeID('null'), ref);
            
            // 设置形状属性
            var shapeDesc = new ActionDescriptor();
            shapeDesc.putUnitDouble(stringIDToTypeID('featherRadius'), charIDToTypeID('#Pxl'), 0.5);
            desc.putObject(charIDToTypeID('T   '), stringIDToTypeID('shapeLayer'), shapeDesc);
            
            executeAction(charIDToTypeID('Mk  '), desc, DialogModes.NO);
            
            alert('✅ 成功创建羽化0.5的形状图层');
        }
    }
} catch(e) {
    alert('❌ 错误：' + e.message + '\n行号：' + e.line);
}

