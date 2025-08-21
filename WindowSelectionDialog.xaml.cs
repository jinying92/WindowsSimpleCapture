using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WindowsSimpleCapture
{
    public partial class WindowSelectionDialog : Window
    {
        public MainWindow.WindowInfo SelectedWindow { get; private set; }
        private List<WindowDisplayInfo> windowDisplayInfos;

        public class WindowDisplayInfo
        {
            public MainWindow.WindowInfo WindowInfo { get; set; }
            public string Title { get; set; }
            public string SizeInfo { get; set; }
        }

        public WindowSelectionDialog(List<MainWindow.WindowInfo> windows)
        {
            InitializeComponent();
            
            // 转换为显示用的数据
            windowDisplayInfos = windows.Select(w => new WindowDisplayInfo
            {
                WindowInfo = w,
                Title = w.Title,
                SizeInfo = $"大小: {w.Rectangle.Right - w.Rectangle.Left} × {w.Rectangle.Bottom - w.Rectangle.Top}"
            }).ToList();
            
            WindowListBox.ItemsSource = windowDisplayInfos;
            
            // 默认选择第一个
            if (windowDisplayInfos.Count > 0)
            {
                WindowListBox.SelectedIndex = 0;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowListBox.SelectedItem is WindowDisplayInfo selectedItem)
            {
                SelectedWindow = selectedItem.WindowInfo;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("请选择一个窗口", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void WindowListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WindowListBox.SelectedItem is WindowDisplayInfo selectedItem)
            {
                SelectedWindow = selectedItem.WindowInfo;
                DialogResult = true;
            }
        }
    }
}