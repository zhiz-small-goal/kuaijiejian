// 测试：路径转形状的不同方法
// 请在Photoshop中先用钢笔工具创建一个路径

alert('测试开始\n请确保已经用钢笔工具创建了路径');

try {
    var doc = app.activeDocument;
    
    // 检查是否有路径
    if (doc.pathItems.length === 0) {
        alert('❌ 未找到路径\n请先使用钢笔工具创建路径');
    } else {
        alert('✅ 找到 ' + doc.pathItems.length + ' 个路径');
        
        // 方法：将路径转换为选区（带羽化）
        alert('[测试] 将路径转为羽化选区...');
        
        var desc = new ActionDescriptor();
        var ref = new ActionReference();
        ref.putProperty(charIDToTypeID('Path'), charIDToTypeID('WrPh'));
        desc.putReference(charIDToTypeID('null'), ref);
        desc.putUnitDouble(charIDToTypeID('Fthr'), charIDToTypeID('#Pxl'), 0.5);
        desc.putBoolean(charIDToTypeID('AntA'), true);
        executeAction(charIDToTypeID('setd'), desc, DialogModes.NO);
        
        alert('✅ 已转为选区（羽化0.5）\n\n接下来测试填充为形状...');
        
        // 方法：用当前选区创建形状图层
        var desc2 = new ActionDescriptor();
        var ref2 = new ActionReference();
        ref2.putClass(stringIDToTypeID('contentLayer'));
        desc2.putReference(charIDToTypeID('null'), ref2);
        
        var desc3 = new ActionDescriptor();
        desc3.putString(charIDToTypeID('Nm  '), '形状图层');
        
        var desc4 = new ActionDescriptor();
        var desc5 = new ActionDescriptor();
        desc5.putDouble(charIDToTypeID('Rd  '), 255);
        desc5.putDouble(charIDToTypeID('Grn '), 0);
        desc5.putDouble(charIDToTypeID('Bl  '), 0);
        desc4.putObject(charIDToTypeID('Clr '), charIDToTypeID('RGBC'), desc5);
        desc3.putObject(charIDToTypeID('Type'), stringIDToTypeID('solidColorLayer'), desc4);
        
        desc2.putObject(charIDToTypeID('Usng'), stringIDToTypeID('contentLayer'), desc3);
        executeAction(charIDToTypeID('Mk  '), desc2, DialogModes.NO);
        
        alert('✅ 完成！\n已创建形状图层（基于羽化选区）');
        
    }
} catch(e) {
    alert('❌ 错误：\n' + e.message + '\n\n行号：' + e.line + '\n\n这个方法可能不正确，需要使用ScriptListener录制官方操作');
}

