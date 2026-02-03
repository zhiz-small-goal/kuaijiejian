using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Kuaijiejian
{
    /// <summary>
    /// 批量删除窗口
    /// WPF 官方最佳实践：使用 MVVM 模式和数据绑定
    /// </summary>
    public partial class BatchDeleteWindow : Window
    {
        private ObservableCollection<FunctionItemViewModel> _items;
        /// <summary>
        /// 选中的功能项
        /// </summary>
        public List<FunctionItem> SelectedFunctions { get; private set; }

        public BatchDeleteWindow(ObservableCollection<FunctionItem> functions)
        {
            InitializeComponent();
            
            SelectedFunctions = new List<FunctionItem>();
            
            // 转换为 ViewModel
            _items = new ObservableCollection<FunctionItemViewModel>();
            foreach (var func in functions)
            {
                _items.Add(new FunctionItemViewModel
                {
                    Function = func,
                    IsSelected = false
                });
            }
            
            FunctionsListBox.ItemsSource = _items;
            UpdateCount();
        }

        /// <summary>
        /// 全选
        /// </summary>
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items)
            {
                item.IsSelected = true;
            }
            UpdateCount();
        }

        /// <summary>
        /// 取消全选
        /// </summary>
        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items)
            {
                item.IsSelected = false;
            }
            UpdateCount();
        }

        /// <summary>
        /// 复选框状态改变
        /// WPF官方最佳实践：通过自定义CheckBox模板扩展点击区域到整行
        /// </summary>
        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateCount();
        }

        /// <summary>
        /// 更新选中数量
        /// </summary>
        private void UpdateCount()
        {
            int count = _items.Count(i => i.IsSelected);
            CountTextBlock.Text = $"已选择: {count}";
        }

        /// <summary>
        /// 删除按钮
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedFunctions = _items
                .Where(i => i.IsSelected)
                .Select(i => i.Function)
                .ToList();

            if (SelectedFunctions.Count == 0)
            {
                return;
            }

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// 取消按钮
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// 关闭按钮
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    /// <summary>
    /// 功能项 ViewModel
    /// </summary>
    public class FunctionItemViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public FunctionItem Function { get; set; } = new FunctionItem();

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public string DisplayText => $"{Function.Icon} {Function.Name}";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


