// Adobe Photoshop 脚本 - 创建新文档
// 基于 Adobe Photoshop Scripting Guide 官方文档
// 官方参考：Documents.add(width, height, resolution, name)

#target photoshop

try {
    // 文档参数
    var docWidth = 1920;
    var docHeight = 1080;
    var docResolution = 72;
    var docName = "新文档";
    
    // 创建新文档 - Adobe 官方方法
    var doc = app.documents.add(docWidth, docHeight, docResolution, docName);
    
    alert('成功创建新文档: ' + docWidth + 'x' + docHeight + ' 像素');
} catch (e) {
    alert('创建文档失败: ' + e.message);
}

