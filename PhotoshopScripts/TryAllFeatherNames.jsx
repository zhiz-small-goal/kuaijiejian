// 尝试所有可能的羽化属性名称
alert('测试所有可能的羽化属性名');

try {
    var doc = app.activeDocument;
    
    // 可能的羽化属性名称列表
    var featherNames = [
        'featherRadius',
        'shapingFeatherRadius', 
        'feather',
        'Fthr',
        'vectorMaskFeather',
        'shapeFeather',
        'layerFeather',
        'pathFeather',
        'maskFeather',
        'edgeFeather',
        'softEdge'
    ];
    
    var results = '测试结果：\n\n';
    var successCount = 0;
    
    for (var i = 0; i < featherNames.length; i++) {
        var name = featherNames[i];
        
        try {
            var desc1 = new ActionDescriptor();
            var ref1 = new ActionReference();
            ref1.putEnumerated(charIDToTypeID('Lyr '), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
            desc1.putReference(charIDToTypeID('null'), ref1);
            
            var desc2 = new ActionDescriptor();
            desc2.putUnitDouble(stringIDToTypeID(name), charIDToTypeID('#Pxl'), 0.5);
            desc1.putObject(charIDToTypeID('T   '), charIDToTypeID('Lyr '), desc2);
            
            executeAction(charIDToTypeID('setd'), desc1, DialogModes.NO);
            
            results += '✅ [' + (i+1) + '] ' + name + ' - 执行成功\n';
            successCount++;
            
        } catch(e) {
            results += '❌ [' + (i+1) + '] ' + name + ' - 失败: ' + e.message.substr(0, 30) + '\n';
        }
    }
    
    results += '\n成功: ' + successCount + '/' + featherNames.length;
    results += '\n\n请检查属性面板，看羽化值是否变为0.5';
    
    alert(results);
    
    // 保存结果到桌面
    var file = new File(Folder.desktop + '/FeatherTest_Results.txt');
    file.open('w');
    file.write(results);
    file.close();
    
} catch(e) {
    alert('❌ 总体错误：' + e.message + '\n行号：' + e.line);
}

