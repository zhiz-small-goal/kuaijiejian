// 对比脚本 - 找出差异

alert("=== 脚本A：测试成功版本 ===");

var scriptA = "var d = app.activeDocument;var n = d.name;var b = n.replace(/\\.[^\\.]+$/, '');alert('A: baseName=' + b);";

alert("=== 脚本B：配置文件版本 ===");

var scriptB = "var d=app.activeDocument,n=d.name,b=n.replace(/\\\\.[^\\\\.]+$/,'');alert('B: baseName=' + b);";

alert("=== 执行测试 ===\n\n请观察两个alert的输出是否一样");

try {
    eval(scriptA);
} catch(e) {
    alert("脚本A失败: " + e.message);
}

try {
    eval(scriptB);
} catch(e) {
    alert("脚本B失败: " + e.message);
}

