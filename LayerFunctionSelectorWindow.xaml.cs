using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Kuaijiejian
{
    public partial class LayerFunctionSelectorWindow : Window
    {
        public List<LayerFunctionViewModel> SelectedFunctions { get; private set; }
        private List<LayerFunctionViewModel> _allFunctions = new(); // 存储所有功能用于搜索

        // 定义事件：当用户确认添加功能时触发
        public event EventHandler<List<LayerFunctionViewModel>>? FunctionsConfirmed;

        public LayerFunctionSelectorWindow()
        {
            InitializeComponent();
            LoadPredefinedFunctions();
            SelectedFunctions = new List<LayerFunctionViewModel>();
            
            // 窗口加载完成后自动聚焦搜索框
            Loaded += (s, e) => SearchBox.Focus();
        }

        /// <summary>
        /// 拖拽窗口：点击窗口任意位置可拖动
        /// </summary>
        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 生成创建调整图层的标准脚本
        /// 方法3：创建图层 + 刷新 + toggle面板
        /// </summary>
        private string CreateAdjustmentLayerScript(string typeId)
        {
            // 优化版本：恢复正确的API调用
            // 关键：必须使用 charIDToTypeID('AdjL') 而不是 stringIDToTypeID('adjustmentLayer')
            return $@"var d=new ActionDescriptor(),r=new ActionReference(),d2=new ActionDescriptor();r.putClass(charIDToTypeID('AdjL'));d.putReference(charIDToTypeID('null'),r);var desc3=new ActionDescriptor();desc3.putEnumerated(stringIDToTypeID('presetKind'),stringIDToTypeID('presetKind'),stringIDToTypeID('presetKindDefault'));d2.putObject(charIDToTypeID('Type'),stringIDToTypeID('{typeId}'),desc3);d.putObject(charIDToTypeID('Usng'),charIDToTypeID('AdjL'),d2);executeAction(charIDToTypeID('Mk  '),d,DialogModes.NO);";
        }

        /// <summary>
        /// 加载预定义的图层编辑功能
        /// 基于 Adobe ExtendScript 官方 API
        /// </summary>
        private void LoadPredefinedFunctions()
        {
            var functions = new List<LayerFunctionViewModel>
            {
                // === 文档保存 ===
                new LayerFunctionViewModel
                {
                    DisplayName = "智能保存",
                    Icon = "💾",
                    Description = "已保存文档直接保存，未保存文档自动保存为TIF（覆盖同名）",
                    Script = @"try{var desc=new ActionDescriptor();executeAction(charIDToTypeID('save'),desc,DialogModes.NO);}catch(e){try{var doc=app.activeDocument,docName=doc.name,baseName=docName.replace(/\.[^\.]+$/,''),savePath;try{savePath=doc.path;}catch(pe){savePath=Folder.desktop;}var tifFile=new File(savePath+'/'+baseName+'.tif'),saveDesc=new ActionDescriptor();saveDesc.putPath(charIDToTypeID('In  '),tifFile);saveDesc.putClass(charIDToTypeID('As  '),charIDToTypeID('TIFF'));executeAction(charIDToTypeID('save'),saveDesc,DialogModes.NO);}catch(se){}}"
                },
                
                new LayerFunctionViewModel
                {
                    DisplayName = "安全保存",
                    Icon = "🛡️",
                    Description = "已保存文档直接保存，未保存文档保存为TIF（自动避免覆盖）",
                    Script = @"try{var desc=new ActionDescriptor();executeAction(charIDToTypeID('save'),desc,DialogModes.NO);}catch(e){try{var doc=app.activeDocument,docName=doc.name,baseName=docName.replace(/\.[^\.]+$/,''),savePath;try{savePath=doc.path;}catch(pe){savePath=Folder.desktop;}var tifFile,counter=0;do{var fileName=baseName+(counter>0?'_'+counter:'')+'.tif';tifFile=new File(savePath+'/'+fileName);if(!tifFile.exists)break;counter++;}while(counter<100);var saveDesc=new ActionDescriptor();saveDesc.putPath(charIDToTypeID('In  '),tifFile);saveDesc.putClass(charIDToTypeID('As  '),charIDToTypeID('TIFF'));executeAction(charIDToTypeID('save'),saveDesc,DialogModes.NO);}catch(se){}}"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "另存为JPG",
                    Icon = "📷",
                    Description = "另存为JPG格式（防覆盖模式，静默无弹窗）",
                    Script = @"try{var oldDM=app.displayDialogs;app.displayDialogs=DialogModes.NO;var d=app.activeDocument,n=d.name,b=n.replace(/\.[^\.]+$/,''),p;try{p=d.path;}catch(e){p=Folder.desktop;}var f,c=0;do{var fn=b+(c>0?'_'+c:'')+'.jpg';f=new File(p+'/'+fn);if(!f.exists)break;c++;}while(c<100);var o=new JPEGSaveOptions();o.quality=12;o.embedColorProfile=true;o.formatOptions=FormatOptions.STANDARDBASELINE;o.matte=MatteType.NONE;d.saveAs(f,o,true);app.displayDialogs=oldDM;}catch(e){app.displayDialogs=DialogModes.ALL;}"
                },
                
                // === 面板控制 ===
                new LayerFunctionViewModel
                {
                    DisplayName = "打开属性面板",
                    Icon = "⚙️",
                    Description = "打开PS属性面板（首次使用时点一次即可，之后创建调整图层会自动更新）",
                    Script = @"try{app.runMenuItem(stringIDToTypeID('togglePropertiesPanel'));}catch(e){}"
                },
                
                // === 历史记录操作 ===
                new LayerFunctionViewModel
                {
                    DisplayName = "还原",
                    Icon = "↩️",
                    Description = "撤销/重做切换（Ctrl+Z）",
                    Script = @"try{executeAction(charIDToTypeID('undo'),undefined,DialogModes.NO);}catch(e){}"
                },
                
                new LayerFunctionViewModel
                {
                    DisplayName = "后退一步",
                    Icon = "⬅️",
                    Description = "后退一步历史记录（Alt+Ctrl+Z）",
                    Script = @"try{if(app.documents.length>0){var doc=app.activeDocument,currentIndex=-1;for(var i=0;i<doc.historyStates.length;i++){if(doc.historyStates[i]==doc.activeHistoryState){currentIndex=i;break;}}if(currentIndex>0){doc.activeHistoryState=doc.historyStates[currentIndex-1];}}}catch(e){}"
                },
                
                new LayerFunctionViewModel
                {
                    DisplayName = "前进一步",
                    Icon = "➡️",
                    Description = "前进一步历史记录（Shift+Ctrl+Z）",
                    Script = @"try{if(app.documents.length>0){var doc=app.activeDocument,currentIndex=-1;for(var i=0;i<doc.historyStates.length;i++){if(doc.historyStates[i]==doc.activeHistoryState){currentIndex=i;break;}}if(currentIndex<doc.historyStates.length-1){doc.activeHistoryState=doc.historyStates[currentIndex+1];}}}catch(e){}"
                },
                
                // === 工具切换 ===
                new LayerFunctionViewModel
                {
                    DisplayName = "裁剪工具",
                    Icon = "✂️",
                    Description = "切换到裁剪工具",
                    Script = @"try{var desc=new ActionDescriptor(),ref=new ActionReference();ref.putClass(stringIDToTypeID('cropTool'));desc.putReference(stringIDToTypeID('null'),ref);executeAction(stringIDToTypeID('select'),desc,DialogModes.NO);}catch(e){}"
                },
                
                new LayerFunctionViewModel
                {
                    DisplayName = "污点修复画笔",
                    Icon = "🩹",
                    Description = "切换到污点修复画笔工具",
                    Script = @"try{var desc=new ActionDescriptor(),ref=new ActionReference();ref.putClass(stringIDToTypeID('spotHealingBrushTool'));desc.putReference(stringIDToTypeID('null'),ref);executeAction(stringIDToTypeID('select'),desc,DialogModes.NO);}catch(e){}"
                },
                
                new LayerFunctionViewModel
                {
                    DisplayName = "修补工具",
                    Icon = "🔧",
                    Description = "切换到修补工具",
                    Script = @"try{var desc=new ActionDescriptor(),ref=new ActionReference();ref.putClass(stringIDToTypeID('patchSelection'));desc.putReference(stringIDToTypeID('null'),ref);executeAction(stringIDToTypeID('select'),desc,DialogModes.NO);}catch(e){}"
                },
                
                new LayerFunctionViewModel
                {
                    DisplayName = "混合画笔",
                    Icon = "🖌️",
                    Description = "切换到混合画笔工具",
                    Script = @"try{var desc=new ActionDescriptor(),ref=new ActionReference();ref.putClass(stringIDToTypeID('wetBrushTool'));desc.putReference(stringIDToTypeID('null'),ref);executeAction(stringIDToTypeID('select'),desc,DialogModes.NO);}catch(e){}"
                },
                
                new LayerFunctionViewModel
                {
                    DisplayName = "渐变工具",
                    Icon = "🌈",
                    Description = "切换到渐变工具",
                    Script = @"try{var desc=new ActionDescriptor(),ref=new ActionReference();ref.putClass(stringIDToTypeID('gradientTool'));desc.putReference(stringIDToTypeID('null'),ref);executeAction(stringIDToTypeID('select'),desc,DialogModes.NO);}catch(e){}"
                },
                
                // === 图层选择 ===
                new LayerFunctionViewModel
                {
                    DisplayName = "选择上一图层",
                    Icon = "⬆️",
                    Description = "选择图层面板中上方的图层（Alt+[）",
                    Script = @"try{var desc=new ActionDescriptor(),ref=new ActionReference();ref.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Bckw'));desc.putReference(charIDToTypeID('null'),ref);desc.putBoolean(charIDToTypeID('MkVs'),false);executeAction(charIDToTypeID('slct'),desc,DialogModes.NO);}catch(e){}"
                },
                
                new LayerFunctionViewModel
                {
                    DisplayName = "选择下一图层",
                    Icon = "⬇️",
                    Description = "选择图层面板中下方的图层（Alt+]）",
                    Script = @"try{var desc=new ActionDescriptor(),ref=new ActionReference();ref.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Frwr'));desc.putReference(charIDToTypeID('null'),ref);desc.putBoolean(charIDToTypeID('MkVs'),false);executeAction(charIDToTypeID('slct'),desc,DialogModes.NO);}catch(e){}"
                },
                
                // === 视图操作 ===
                new LayerFunctionViewModel
                {
                    DisplayName = "清除所有参考线",
                    Icon = "📏",
                    Description = "清除文档中的所有参考线",
                    Script = @"try{if(app.documents.length>0){var desc=new ActionDescriptor();executeAction(stringIDToTypeID('clearAllGuides'),desc,DialogModes.NO);}}catch(e){}"
                },
                
                // === 选区操作 ===
                new LayerFunctionViewModel
                {
                    DisplayName = "取消选区",
                    Icon = "🔲",
                    Description = "取消当前选区（Ctrl+D）",
                    Script = @"try{if(app.documents.length>0){var desc=new ActionDescriptor(),ref=new ActionReference();ref.putProperty(charIDToTypeID('Chnl'),charIDToTypeID('fsel'));desc.putReference(charIDToTypeID('null'),ref);desc.putEnumerated(charIDToTypeID('T   '),charIDToTypeID('Ordn'),charIDToTypeID('None'));executeAction(charIDToTypeID('setd'),desc,DialogModes.NO);}}catch(e){}"
                },
                
                // === 图层基础操作 ===
                new LayerFunctionViewModel
                {
                    DisplayName = "新建图层",
                    Icon = "新建",
                    Description = "在当前选中图层上方创建新图层（无弹窗）",
                    Script = @"
var d=new ActionDescriptor(),r=new ActionReference();r.putClass(charIDToTypeID('Lyr '));d.putReference(charIDToTypeID('null'),r);executeAction(charIDToTypeID('Mk  '),d,DialogModes.NO);"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "复制图层",
                    Icon = "复制",
                    Description = "复制当前选中的图层（无弹窗）",
                    Script = @"
var d=new ActionDescriptor(),r=new ActionReference();r.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Trgt'));d.putReference(charIDToTypeID('null'),r);executeAction(charIDToTypeID('Dplc'),d,DialogModes.NO);"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "删除图层",
                    Icon = "删除",
                    Description = "删除当前选中的图层（静默执行）",
                    Script = @"
try{if(app.documents.length>0&&app.activeDocument.activeLayer){var idDlt=charIDToTypeID('Dlt '),desc=new ActionDescriptor(),ref=new ActionReference();ref.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Trgt'));desc.putReference(charIDToTypeID('null'),ref);executeAction(idDlt,desc,DialogModes.NO);}}catch(e){}"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "合并图层",
                    Icon = "合并",
                    Description = "合并当前图层与下一图层（静默执行）",
                    Script = @"
try{if(app.documents.length>0){var idMrg2=charIDToTypeID('Mrg2');executeAction(idMrg2,undefined,DialogModes.NO);}}catch(e){}"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "合并可见图层",
                    Icon = "可见",
                    Description = "合并所有可见图层（静默执行）",
                    Script = @"
try{if(app.documents.length>0){var idMrgV=charIDToTypeID('MrgV');executeAction(idMrgV,undefined,DialogModes.NO);}}catch(e){}"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "拼合图像",
                    Icon = "拼合",
                    Description = "拼合所有图层为单一背景图层（静默执行）",
                    Script = @"
try{if(app.documents.length>0){var idFltI=charIDToTypeID('FltI');executeAction(idFltI,undefined,DialogModes.NO);}}catch(e){}"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "栅格化图层",
                    Icon = "栅格",
                    Description = "将当前图层栅格化（静默执行）",
                    Script = @"
try{if(app.documents.length>0&&app.activeDocument.activeLayer){app.activeDocument.activeLayer.rasterize(RasterizeType.ENTIRELAYER);}}catch(e){}"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "显示/隐藏图层",
                    Icon = "显隐",
                    Description = "切换当前图层的可见性（静默执行）",
                    Script = @"
try{if(app.documents.length>0&&app.activeDocument.activeLayer){var layer=app.activeDocument.activeLayer,idAction=layer.visible?charIDToTypeID('Hd  '):charIDToTypeID('Shw '),desc=new ActionDescriptor(),list=new ActionList(),ref=new ActionReference();ref.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Trgt'));list.putReference(ref);desc.putList(charIDToTypeID('null'),list);executeAction(idAction,desc,DialogModes.NO);}}catch(e){}"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "锁定/解锁图层",
                    Icon = "锁定",
                    Description = "切换当前图层的锁定状态",
                    Script = @"
if (app.documents.length > 0) {
    var doc = app.activeDocument;
    if (doc.activeLayer) {
        doc.activeLayer.allLocked = !doc.activeLayer.allLocked;
        doc.activeLayer.allLocked ? '图层已锁定' : '图层已解锁';
    } else {
        throw new Error('请先选择一个图层');
    }
} else {
    throw new Error('请先打开一个文档');
}"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "图层置顶",
                    Icon = "置顶",
                    Description = "将当前图层移动到最顶层（静默执行）",
                    Script = @"
try{if(app.documents.length>0&&app.activeDocument.activeLayer){var desc=new ActionDescriptor(),ref=new ActionReference(),ref2=new ActionReference();ref.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Trgt'));desc.putReference(charIDToTypeID('null'),ref);ref2.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Frnt'));desc.putReference(charIDToTypeID('T   '),ref2);executeAction(charIDToTypeID('move'),desc,DialogModes.NO);}}catch(e){}"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "图层置底",
                    Icon = "置底",
                    Description = "将当前图层移动到最底层（静默执行）",
                    Script = @"
try{if(app.documents.length>0&&app.activeDocument.activeLayer){var desc=new ActionDescriptor(),ref=new ActionReference(),ref2=new ActionReference();ref.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Trgt'));desc.putReference(charIDToTypeID('null'),ref);ref2.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Back'));desc.putReference(charIDToTypeID('T   '),ref2);executeAction(charIDToTypeID('move'),desc,DialogModes.NO);}}catch(e){}"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "清除图层",
                    Icon = "清除",
                    Description = "清除当前图层的所有内容",
                    Script = @"
if (app.documents.length > 0) {
    var doc = app.activeDocument;
    if (doc.activeLayer) {
        doc.activeLayer.clear();
        '已清除图层内容';
    } else {
        throw new Error('请先选择一个图层');
    }
} else {
    throw new Error('请先打开一个文档');
}"
                },

                // === 高级图层操作 ===
                new LayerFunctionViewModel
                {
                    DisplayName = "前移一层",
                    Icon = "⬆️",
                    Description = "将选中图层向前移动一层（Photoshop中的前移一层）",
                    Script = @"
var d=new ActionDescriptor(),r=new ActionReference(),r2=new ActionReference();r.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Trgt'));d.putReference(charIDToTypeID('null'),r);r2.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Frwr'));d.putReference(charIDToTypeID('T   '),r2);executeAction(charIDToTypeID('move'),d,DialogModes.NO);"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "后移一层",
                    Icon = "⬇️",
                    Description = "将选中图层向后移动一层（Photoshop中的后移一层）",
                    Script = @"
var d=new ActionDescriptor(),r=new ActionReference(),r2=new ActionReference();r.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Trgt'));d.putReference(charIDToTypeID('null'),r);r2.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Bckw'));d.putReference(charIDToTypeID('T   '),r2);executeAction(charIDToTypeID('move'),d,DialogModes.NO);"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "剪切蒙版",
                    Icon = "🔗",
                    Description = "创建/释放剪切蒙版（自动切换）",
                    Script = @"
if(app.documents.length>0&&app.activeDocument.activeLayer){var l=app.activeDocument.activeLayer,d=new ActionDescriptor(),r=new ActionReference();r.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Trgt'));d.putReference(charIDToTypeID('null'),r);if(l.grouped){executeAction(charIDToTypeID('Ungr'),d,DialogModes.NO);}else{executeAction(charIDToTypeID('GrpL'),d,DialogModes.NO);}}"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "创建新组",
                    Icon = "📁",
                    Description = "创建一个新的空图层组",
                    Script = @"
var d=new ActionDescriptor(),r=new ActionReference();r.putClass(stringIDToTypeID('layerSection'));d.putReference(charIDToTypeID('null'),r);executeAction(charIDToTypeID('Mk  '),d,DialogModes.NO);"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "图层编组",
                    Icon = "📂",
                    Description = "将选中的图层编组到新组中（Ctrl+G）",
                    Script = @"
var d=new ActionDescriptor(),r=new ActionReference();r.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Trgt'));d.putReference(charIDToTypeID('null'),r);executeAction(stringIDToTypeID('groupLayersEvent'),d,DialogModes.NO);"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "转智能对象",
                    Icon = "💎",
                    Description = "将选中图层转换为智能对象",
                    Script = @"
var d=new ActionDescriptor(),r=new ActionReference();r.putEnumerated(charIDToTypeID('Lyr '),charIDToTypeID('Ordn'),charIDToTypeID('Trgt'));d.putReference(charIDToTypeID('null'),r);executeAction(stringIDToTypeID('newPlacedLayer'),d,DialogModes.NO);"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "白色蒙版",
                    Icon = "⬜",
                    Description = "为选中图层添加显示全部的白色蒙版",
                    Script = @"
var d=new ActionDescriptor();d.putClass(charIDToTypeID('Nw  '),charIDToTypeID('Chnl'));var r=new ActionReference();r.putEnumerated(charIDToTypeID('Chnl'),charIDToTypeID('Chnl'),charIDToTypeID('Msk '));d.putReference(charIDToTypeID('At  '),r);d.putEnumerated(charIDToTypeID('Usng'),charIDToTypeID('UsrM'),charIDToTypeID('RvlA'));executeAction(charIDToTypeID('Mk  '),d,DialogModes.NO);"
                },

                new LayerFunctionViewModel
                {
                    DisplayName = "黑色蒙版",
                    Icon = "⬛",
                    Description = "为选中图层添加隐藏全部的黑色蒙版",
                    Script = @"
var d=new ActionDescriptor();d.putClass(charIDToTypeID('Nw  '),charIDToTypeID('Chnl'));var r=new ActionReference();r.putEnumerated(charIDToTypeID('Chnl'),charIDToTypeID('Chnl'),charIDToTypeID('Msk '));d.putReference(charIDToTypeID('At  '),r);d.putEnumerated(charIDToTypeID('Usng'),charIDToTypeID('UsrM'),charIDToTypeID('HdAl'));executeAction(charIDToTypeID('Mk  '),d,DialogModes.NO);"
                },

                // === 调整图层功能（使用统一的创建方法）===
                new LayerFunctionViewModel
                {
                    DisplayName = "色相/饱和度",
                    Icon = "🎨",
                    Description = "调整图像的色相、饱和度和明度",
                    Script = CreateAdjustmentLayerScript("hueSaturation")
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "曲线",
                    Icon = "📈",
                    Description = "精确调整图像的色调范围",
                    Script = CreateAdjustmentLayerScript("curves")
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "色阶",
                    Icon = "📊",
                    Description = "调整图像的高光、中间调和阴影",
                    Script = CreateAdjustmentLayerScript("levels")
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "亮度/对比度",
                    Icon = "☀️",
                    Description = "简单调整亮度和对比度",
                    Script = CreateAdjustmentLayerScript("brightnessContrast")
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "色彩平衡",
                    Icon = "⚖️",
                    Description = "调整阴影、中间调和高光的颜色平衡",
                    Script = @"var d=new ActionDescriptor(),r=new ActionReference(),d2=new ActionDescriptor();r.putClass(charIDToTypeID('AdjL'));d.putReference(charIDToTypeID('null'),r);d2.putClass(charIDToTypeID('Type'),charIDToTypeID('ClrB'));d.putObject(charIDToTypeID('Usng'),charIDToTypeID('AdjL'),d2);executeAction(charIDToTypeID('Mk  '),d,DialogModes.NO);"
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "黑白",
                    Icon = "⚫",
                    Description = "将彩色图像转换为黑白",
                    Script = CreateAdjustmentLayerScript("blackAndWhite")
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "自然饱和度",
                    Icon = "🌈",
                    Description = "以更自然的方式增强饱和度",
                    Script = CreateAdjustmentLayerScript("vibrance")
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "曝光度",
                    Icon = "💡",
                    Description = "调整图像的曝光度",
                    Script = CreateAdjustmentLayerScript("exposure")
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "照片滤镜",
                    Icon = "📷",
                    Description = "模拟彩色镜头滤镜效果",
                    Script = CreateAdjustmentLayerScript("photoFilter")
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "通道混合器",
                    Icon = "🔀",
                    Description = "混合颜色通道创建特殊效果",
                    Script = CreateAdjustmentLayerScript("channelMixer")
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "颜色查找",
                    Icon = "🎯",
                    Description = "应用预设的颜色查找表",
                    Script = CreateAdjustmentLayerScript("colorLookup")
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "反相",
                    Icon = "🔄",
                    Description = "反转图像的颜色",
                    Script = CreateAdjustmentLayerScript("invert")
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "色调分离",
                    Icon = "🎭",
                    Description = "减少图像中的色调数量",
                    Script = CreateAdjustmentLayerScript("posterization")
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "阈值",
                    Icon = "⚡",
                    Description = "将图像转换为高对比度黑白",
                    Script = CreateAdjustmentLayerScript("thresholdClassEvent")
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "渐变映射",
                    Icon = "🌅",
                    Description = "将渐变映射到图像的灰度范围",
                    Script = @"var d=new ActionDescriptor(),r=new ActionReference(),d2=new ActionDescriptor();r.putClass(charIDToTypeID('AdjL'));d.putReference(charIDToTypeID('null'),r);var desc3=new ActionDescriptor();desc3.putEnumerated(stringIDToTypeID('presetKind'),stringIDToTypeID('presetKind'),stringIDToTypeID('presetKindDefault'));d2.putObject(charIDToTypeID('Type'),charIDToTypeID('GdMp'),desc3);d.putObject(charIDToTypeID('Usng'),charIDToTypeID('AdjL'),d2);executeAction(charIDToTypeID('Mk  '),d,DialogModes.NO);"
                },
                new LayerFunctionViewModel
                {
                    DisplayName = "可选颜色",
                    Icon = "🎨",
                    Description = "选择性调整特定颜色",
                    Script = CreateAdjustmentLayerScript("selectiveColor")
                }
            };

            _allFunctions = functions; // 保存完整列表用于搜索
            FunctionsList.ItemsSource = functions;
            UpdateSelectionCount();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (FunctionsList.ItemsSource is List<LayerFunctionViewModel> list)
            {
                foreach (var item in list)
                {
                    item.IsSelected = true;
                }
            }
            UpdateSelectionCount();
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            if (FunctionsList.ItemsSource is List<LayerFunctionViewModel> list)
            {
                foreach (var item in list)
                {
                    item.IsSelected = false;
                }
            }
            UpdateSelectionCount();
        }

        private void UpdateSelectionCount()
        {
            if (FunctionsList.ItemsSource is List<LayerFunctionViewModel> functions)
            {
                int count = functions.Count(f => f.IsSelected);
                SelectionCountText.Text = $"已选择: {count} 个";
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (FunctionsList.ItemsSource is List<LayerFunctionViewModel> functions)
            {
                SelectedFunctions = functions.Where(f => f.IsSelected).ToList();
                
                if (SelectedFunctions.Count == 0)
                {
                    NotificationWindow.Show("💡 提示", "请至少选择一个功能", 0.5);
                    return;
                }

                // 触发事件通知主窗口
                FunctionsConfirmed?.Invoke(this, SelectedFunctions);
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 搜索框文本变化事件 - 实时过滤功能列表
        /// </summary>
        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_allFunctions == null) return;

            string searchText = SearchBox.Text.ToLower().Trim();
            
            // 控制清除按钮显示
            ClearSearchButton.Visibility = string.IsNullOrWhiteSpace(searchText) ? Visibility.Collapsed : Visibility.Visible;
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // 显示所有功能
                FunctionsList.ItemsSource = _allFunctions;
            }
            else
            {
                // 过滤功能：搜索名称和描述
                var filtered = _allFunctions.Where(f => 
                    f.DisplayName.ToLower().Contains(searchText) || 
                    (f.Description != null && f.Description.ToLower().Contains(searchText))
                ).ToList();
                
                FunctionsList.ItemsSource = filtered;
            }
            
            UpdateSelectionCount();
        }

        /// <summary>
        /// 清除搜索按钮点击事件
        /// </summary>
        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            SearchBox.Focus();
        }
    }

    /// <summary>
    /// 图层功能视图模型
    /// </summary>
    public class LayerFunctionViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string DisplayName { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Description { get; set; } = "";
        public string Script { get; set; } = "";

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}


