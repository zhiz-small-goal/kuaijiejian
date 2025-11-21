using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HandyControl.Data;

namespace Kuaijiejian
{
    public partial class ColorPickerWindow : Window
    {
        public string? SelectedColor { get; private set; }
        public Action<string>? OnColorPreview { get; set; }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        public ColorPickerWindow()
        {
            InitializeComponent();
        }

        private void ColorPicker_ColorChanged(object sender, FunctionEventArgs<Color> e)
        {
            var color = e.Info;
            string hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            
            PreviewBorder.Background = new SolidColorBrush(color);
            PreviewHexText.Text = hexColor;
            
            var brightness = (color.R * 299 + color.G * 587 + color.B * 114) / 1000;
            PreviewHexText.Foreground = brightness > 128 ? Brushes.Black : Brushes.White;
            
            OnColorPreview?.Invoke(hexColor);
        }

        private void PresetColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Background is SolidColorBrush brush)
            {
                MainColorPicker.SelectedBrush = brush;
            }
        }

        private void ScreenColorPicker_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            
            System.Threading.Tasks.Task.Delay(200).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    var overlay = new Window
                    {
                        WindowStyle = WindowStyle.None,
                        ResizeMode = ResizeMode.NoResize,
                        WindowState = WindowState.Maximized,
                        Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
                        Topmost = true,
                        AllowsTransparency = true,
                        Cursor = Cursors.Cross,
                        ShowInTaskbar = false
                    };

                    var previewBorder = new Border
                    {
                        Width = 120,
                        Height = 60,
                        Background = Brushes.White,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(2),
                        CornerRadius = new CornerRadius(4)
                    };

                    var previewStack = new StackPanel
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    var previewText = new TextBlock
                    {
                        Text = "#FFFFFF",
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    var previewTip = new TextBlock
                    {
                        Text = "点击确定",
                        FontSize = 10,
                        Foreground = Brushes.Gray,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 4, 0, 0)
                    };

                    previewStack.Children.Add(previewText);
                    previewStack.Children.Add(previewTip);
                    previewBorder.Child = previewStack;

                    var canvas = new Canvas();
                    canvas.Children.Add(previewBorder);
                    overlay.Content = canvas;

                    var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
                    timer.Tick += (s, args) =>
                    {
                        GetCursorPos(out POINT point);
                        IntPtr hdc = GetDC(IntPtr.Zero);
                        uint pixel = GetPixel(hdc, point.X, point.Y);
                        ReleaseDC(IntPtr.Zero, hdc);

                        byte r = (byte)(pixel & 0x000000FF);
                        byte g = (byte)((pixel & 0x0000FF00) >> 8);
                        byte b = (byte)((pixel & 0x00FF0000) >> 16);

                        var color = Color.FromRgb(r, g, b);
                        var hexColor = $"#{r:X2}{g:X2}{b:X2}";

                        previewBorder.Background = new SolidColorBrush(color);
                        previewText.Text = hexColor;

                        var brightness = (r * 299 + g * 587 + b * 114) / 1000;
                        previewText.Foreground = brightness > 128 ? Brushes.Black : Brushes.White;
                        previewTip.Foreground = brightness > 128 ? Brushes.Gray : Brushes.LightGray;

                        Canvas.SetLeft(previewBorder, point.X + 20);
                        Canvas.SetTop(previewBorder, point.Y + 20);

                        MainColorPicker.SelectedBrush = new SolidColorBrush(color);
                    };
                    timer.Start();

                    overlay.MouseLeftButtonDown += (s, args) =>
                    {
                        timer.Stop();
                        overlay.Close();
                        this.WindowState = WindowState.Normal;
                    };

                    overlay.MouseRightButtonDown += (s, args) =>
                    {
                        timer.Stop();
                        overlay.Close();
                        this.WindowState = WindowState.Normal;
                        DialogResult = false;
                        Close();
                    };

                    overlay.KeyDown += (s, args) =>
                    {
                        if (args.Key == Key.Escape)
                        {
                            timer.Stop();
                            overlay.Close();
                            this.WindowState = WindowState.Normal;
                            DialogResult = false;
                            Close();
                        }
                    };

                    overlay.ShowDialog();
                });
            });
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (MainColorPicker.SelectedBrush is SolidColorBrush brush)
            {
                var color = brush.Color;
                SelectedColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
