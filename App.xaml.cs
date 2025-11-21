using System;
using System.Windows;
using System.Windows.Threading;

namespace Kuaijiejian
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 全局异常处理
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 释放缓存的COM资源
            // 性能优化最佳实践：程序退出时清理
            PhotoshopHelper.ReleaseCachedResources();
            
            // 强制垃圾回收，确保COM对象被释放
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"应用程序发生错误：\n\n{e.Exception.Message}\n\n{e.Exception.StackTrace}", 
                "错误", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show(
                    $"应用程序发生严重错误：\n\n{ex.Message}\n\n{ex.StackTrace}", 
                    "严重错误", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
    }
}

