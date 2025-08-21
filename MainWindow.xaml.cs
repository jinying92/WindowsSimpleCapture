using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace WindowsSimpleCapture
{
    public partial class MainWindow : Window
    {
        // Windows API 声明
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, ref RECT rectangle);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private List<WindowInfo> availableWindows = new List<WindowInfo>();

        public class WindowInfo
        {
            public IntPtr Handle { get; set; }
            public string Title { get; set; }
            public RECT Rectangle { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            UpdateStatus("准备就绪");
        }

        private void TopMostCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            UpdateStatus("窗口已置于顶层");
        }

        private void TopMostCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
            UpdateStatus("窗口置顶已取消");
        }

        private async void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("准备截取全屏...");
            await Task.Delay(100); // 短暂延迟确保状态更新
            
            // 隐藏窗口
            this.Hide();
            
            // 等待窗口完全隐藏
            await Task.Delay(500);
            
            try
            {
                CaptureFullScreen();
                UpdateStatus("全屏截图已保存");
            }
            catch (Exception ex)
            {
                UpdateStatus($"截图失败: {ex.Message}");
            }
            finally
            {
                // 显示窗口
                this.Show();
                this.Activate();
            }
        }

        private async void WindowButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("选择要截取的窗口...");
            
            // 获取所有可见窗口
            GetAvailableWindows();
            
            if (availableWindows.Count == 0)
            {
                UpdateStatus("未找到可截取的窗口");
                return;
            }

            // 显示窗口选择对话框
            var selectedWindow = ShowWindowSelectionDialog();
            if (selectedWindow == null)
            {
                UpdateStatus("已取消窗口截图");
                return;
            }

            UpdateStatus("准备截取窗口...");
            await Task.Delay(100);
            
            // 隐藏窗口
            this.Hide();
            
            // 等待窗口完全隐藏
            await Task.Delay(500);
            
            try
            {
                CaptureWindow(selectedWindow);
                UpdateStatus("窗口截图已保存");
            }
            catch (Exception ex)
            {
                UpdateStatus($"截图失败: {ex.Message}");
            }
            finally
            {
                // 显示窗口
                this.Show();
                this.Activate();
            }
        }

        private void CaptureFullScreen()
        {
            var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            var screenHeight = (int)SystemParameters.PrimaryScreenHeight;
            using (var bitmap = new Bitmap(screenWidth, screenHeight))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));
                }
                
                var fileName = GenerateFileName("全屏");
                var screenshotFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Screenshots");
                if (!Directory.Exists(screenshotFolder))
                {
                    Directory.CreateDirectory(screenshotFolder);
                }
                var filePath = Path.Combine(screenshotFolder, fileName);
                bitmap.Save(filePath, ImageFormat.Png);
                
                // 根据复选框状态复制到剪贴板
                if (CopyToClipboardCheckBox.IsChecked == true)
                {
                    CopyBitmapToClipboard(bitmap);
                    UpdateStatus($"✅ 全屏截图已保存并复制到剪贴板: {fileName}");
                }
                else
                {
                    UpdateStatus($"✅ 全屏截图已保存: {fileName}");
                }
            }
        }

        private void CaptureWindow(WindowInfo windowInfo)
        {
            var rect = windowInfo.Rectangle;
            var width = rect.Right - rect.Left;
            var height = rect.Bottom - rect.Top;
            
            using (var bitmap = new Bitmap(width, height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, new System.Drawing.Size(width, height));
                }
                
                var fileName = GenerateFileName(windowInfo.Title);
                var screenshotFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Screenshots");
                if (!Directory.Exists(screenshotFolder))
                {
                    Directory.CreateDirectory(screenshotFolder);
                }
                var filePath = Path.Combine(screenshotFolder, fileName);
                bitmap.Save(filePath, ImageFormat.Png);
                
                // 根据复选框状态复制到剪贴板
                if (CopyToClipboardCheckBox.IsChecked == true)
                {
                    CopyBitmapToClipboard(bitmap);
                    UpdateStatus($"✅ 窗口截图已保存并复制到剪贴板: {fileName}");
                }
                else
                {
                    UpdateStatus($"✅ 窗口截图已保存: {fileName}");
                }
            }
        }

        private string GenerateFileName(string windowTitle)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            if (windowTitle == "全屏")
            {
                return $"Screenshot_全屏_{timestamp}.png";
            }
            else
            {
                // 清理窗口标题中的非法字符
                var cleanTitle = string.Join("_", windowTitle.Split(Path.GetInvalidFileNameChars()));
                if (cleanTitle.Length > 50) // 限制长度
                {
                    cleanTitle = cleanTitle.Substring(0, 50);
                }
                return $"Screenshot_{cleanTitle}_{timestamp}.png";
            }
        }

        private void GetAvailableWindows()
        {
            availableWindows.Clear();
            EnumWindows(EnumWindowCallback, IntPtr.Zero);
        }

        private bool EnumWindowCallback(IntPtr hWnd, IntPtr lparam)
        {
            if (!IsWindowVisible(hWnd))
                return true;

            var title = GetWindowTitle(hWnd);
            if (string.IsNullOrEmpty(title) || title.Length < 3)
                return true;

            // 排除自己的窗口
            if (title.Contains("Windows Simple Capture"))
                return true;

            var rect = new RECT();
            if (GetWindowRect(hWnd, ref rect))
            {
                // 排除太小的窗口
                if (rect.Right - rect.Left > 100 && rect.Bottom - rect.Top > 100)
                {
                    availableWindows.Add(new WindowInfo
                    {
                        Handle = hWnd,
                        Title = title,
                        Rectangle = rect
                    });
                }
            }

            return true;
        }

        private string GetWindowTitle(IntPtr hWnd)
        {
            var text = new StringBuilder(256);
            GetWindowText(hWnd, text, text.Capacity);
            return text.ToString();
        }

        private WindowInfo ShowWindowSelectionDialog()
        {
            var dialog = new WindowSelectionDialog(availableWindows);
            if (dialog.ShowDialog() == true)
            {
                return dialog.SelectedWindow;
            }
            return null;
        }

        private void CopyBitmapToClipboard(Bitmap bitmap)
        {
            try
            {
                // 将Bitmap复制到剪贴板
                System.Windows.Clipboard.SetImage(ConvertBitmapToBitmapSource(bitmap));
            }
            catch (Exception ex)
            {
                UpdateStatus($"复制到剪贴板失败: {ex.Message}");
            }
        }

        private BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private void UpdateStatus(string message)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = message;
            });
        }

        private void OpenFolderLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var galleryWindow = new ScreenshotGalleryWindow();
                galleryWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开截图画廊: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}