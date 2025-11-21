// 直接测试bin配置文件里的JPG代码
// 这是从 bin/Debug/net8.0-windows/functions_config.json 第29行复制的

alert("开始测试bin配置里的JPG代码...");

try{
    var d=app.activeDocument;
    var n=d.name;
    
    alert("文档名: " + n);
    
    var b=n.replace(/\\.[^\\.]+$/,'');
    
    alert("去扩展名后: " + b);
    
    var p;
    try{
        p=d.path;
        alert("路径: " + p.fsName);
    }catch(e){
        p=Folder.desktop;
        alert("使用桌面: " + p.fsName);
    }
    
    var f,c=0;
    do{
        var fn=b+(c>0?'_'+c:'')+'.jpg';
        f=new File(p+'/'+fn);
        if(!f.exists)break;
        c++;
    }while(c<100);
    
    alert("目标文件: " + f.fsName);
    
    var o=new JPEGSaveOptions();
    o.quality=12;
    o.embedColorProfile=true;
    o.formatOptions=FormatOptions.STANDARDBASELINE;
    o.matte=MatteType.NONE;
    
    d.saveAs(f,o,true);
    
    alert("✅ 保存成功！");
}catch(e){
    alert("❌ 错误: " + e.message + "\n行号: " + e.line);
}

