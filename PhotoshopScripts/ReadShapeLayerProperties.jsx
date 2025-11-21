// 诊断：读取形状图层的所有属性
// 用于找出羽化属性的真实名称

alert('开始读取形状图层属性');

try {
    var doc = app.activeDocument;
    var layer = doc.activeLayer;
    
    alert('图层类型：' + layer.typename + '\n图层名：' + layer.name);
    
    // 使用 ActionManager 获取图层描述符
    var ref = new ActionReference();
    ref.putEnumerated(charIDToTypeID('Lyr '), charIDToTypeID('Ordn'), charIDToTypeID('Trgt'));
    var desc = executeActionGet(ref);
    
    var output = '=== 图层属性列表 ===\n\n';
    
    // 遍历所有属性
    for (var i = 0; i < desc.count; i++) {
        try {
            var key = desc.getKey(i);
            var keyStr = typeIDToStringID(key);
            var keyChar = typeIDToCharID(key);
            var type = desc.getType(key);
            var typeStr = '';
            var value = '';
            
            // 根据类型获取值
            switch(type) {
                case DescValueType.INTEGERTYPE:
                    typeStr = 'INTEGER';
                    value = desc.getInteger(key);
                    break;
                case DescValueType.DOUBLETYPE:
                    typeStr = 'DOUBLE';
                    value = desc.getDouble(key);
                    break;
                case DescValueType.UNITDOUBLE:
                    typeStr = 'UNITDOUBLE';
                    value = desc.getUnitDoubleValue(key);
                    break;
                case DescValueType.STRINGTYPE:
                    typeStr = 'STRING';
                    value = desc.getString(key);
                    break;
                case DescValueType.BOOLEANTYPE:
                    typeStr = 'BOOLEAN';
                    value = desc.getBoolean(key);
                    break;
                case DescValueType.OBJECTTYPE:
                    typeStr = 'OBJECT';
                    value = '[Object]';
                    break;
                case DescValueType.LISTTYPE:
                    typeStr = 'LIST';
                    value = '[List]';
                    break;
                default:
                    typeStr = 'UNKNOWN';
                    value = '?';
            }
            
            output += '[' + i + '] ' + keyStr + ' (' + keyChar + ')\n';
            output += '    类型: ' + typeStr + '\n';
            output += '    值: ' + value + '\n\n';
            
            // 特别关注包含 feather、羽化、shape 的属性
            if (keyStr.toLowerCase().indexOf('feather') !== -1 || 
                keyStr.toLowerCase().indexOf('shape') !== -1) {
                output += '⭐⭐⭐ 可能是羽化相关！\n\n';
            }
            
        } catch(e) {
            output += '[' + i + '] 读取失败\n\n';
        }
    }
    
    // 将输出保存到桌面
    var file = new File(Folder.desktop + '/ShapeLayerProperties.txt');
    file.open('w');
    file.write(output);
    file.close();
    
    alert('✅ 完成！\n\n属性列表已保存到桌面：\nShapeLayerProperties.txt\n\n请查看文件，找到包含"feather"或"shape"的属性');
    
} catch(e) {
    alert('❌ 错误：' + e.message + '\n行号：' + e.line);
}

