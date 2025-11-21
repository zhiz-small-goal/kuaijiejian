// 将路径转换为羽化0.5的选区并创建蒙版
try {
    if (app.documents.length === 0) {
        alert('请先打开一个文档');
    } else {
        var doc = app.activeDocument;
        
        // 检查是否有路径
        if (doc.pathItems.length === 0) {
            alert('当前文档没有路径\n请先使用钢笔工具创建路径');
        } else {
            // 获取工作路径
            var workPath = doc.pathItems.getByName("Work Path");
            
            if (!workPath) {
                alert('未找到工作路径\n请确保钢笔工具创建的路径名为"Work Path"');
            } else {
                // 将路径转为选区，羽化0.5
                var desc = new ActionDescriptor();
                var ref = new ActionReference();
                ref.putProperty(charIDToTypeID('Path'), charIDToTypeID('WrPh')); // Work Path
                desc.putReference(charIDToTypeID('null'), ref);
                desc.putUnitDouble(charIDToTypeID('Fthr'), charIDToTypeID('#Pxl'), 0.5); // 羽化0.5像素
                desc.putBoolean(charIDToTypeID('AntA'), true); // 消锯齿
                executeAction(charIDToTypeID('setd'), desc, DialogModes.NO);
                
                // 创建图层蒙版
                var maskDesc = new ActionDescriptor();
                maskDesc.putClass(charIDToTypeID('Nw  '), charIDToTypeID('Chnl'));
                var maskRef = new ActionReference();
                maskRef.putEnumerated(charIDToTypeID('Chnl'), charIDToTypeID('Chnl'), charIDToTypeID('Msk '));
                maskDesc.putReference(charIDToTypeID('At  '), maskRef);
                maskDesc.putEnumerated(charIDToTypeID('Usng'), charIDToTypeID('UsrM'), charIDToTypeID('RvlS')); // 显示选区
                executeAction(charIDToTypeID('Mk  '), maskDesc, DialogModes.NO);
                
                // 取消选区
                doc.selection.deselect();
                
                alert('✅ 成功创建羽化0.5的蒙版');
            }
        }
    }
} catch(e) {
    alert('❌ 错误：' + e.message + '\n行号：' + e.line);
}

