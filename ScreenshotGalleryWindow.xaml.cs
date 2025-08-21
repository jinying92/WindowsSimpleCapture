using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Globalization;

namespace WindowsSimpleCapture
{
    public partial class ScreenshotGalleryWindow : Window
    {
        private List<ScreenshotInfo> allScreenshots = new List<ScreenshotInfo>();
        private List<ScreenshotInfo> selectedScreenshots = new List<ScreenshotInfo>();
        private string currentCategory = "date";
        private string screenshotFolder;

        public ScreenshotGalleryWindow()
        {
            InitializeComponent();
            screenshotFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Screenshots");
            LoadScreenshots();
        }

        private async void LoadScreenshots()
        {
            StatusText.Text = "正在加载截图...";
            
            try
            {
                await Task.Run(() =>
                {
                    allScreenshots.Clear();
                    
                    if (!Directory.Exists(screenshotFolder))
                    {
                        Directory.CreateDirectory(screenshotFolder);
                        return;
                    }
                    
                    var imageFiles = Directory.GetFiles(screenshotFolder, "*.png")
                        .Concat(Directory.GetFiles(screenshotFolder, "*.jpg"))
                        .Concat(Directory.GetFiles(screenshotFolder, "*.jpeg"))
                        .ToArray();
                    
                    foreach (var file in imageFiles)
                    {
                        var info = ParseScreenshotInfo(file);
                        if (info != null)
                        {
                            allScreenshots.Add(info);
                        }
                    }
                });
                
                TotalCountText.Text = $"(共 {allScreenshots.Count} 张截图)";
                RefreshDisplay();
                StatusText.Text = "加载完成";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"加载失败: {ex.Message}";
            }
        }

        private ScreenshotInfo ParseScreenshotInfo(string filePath)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var fileInfo = new FileInfo(filePath);
                
                // 解析文件名格式: Screenshot_窗口名_20231201_143022.png
                var parts = fileName.Split('_');
                
                var info = new ScreenshotInfo
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    CreatedTime = fileInfo.CreationTime,
                    FileSize = fileInfo.Length,
                    ComputerName = Environment.MachineName
                };
                
                if (parts.Length >= 2)
                {
                    info.WindowName = parts[1] == "全屏" ? "全屏截图" : parts[1];
                }
                else
                {
                    info.WindowName = "未知窗口";
                }
                
                return info;
            }
            catch
            {
                return null;
            }
        }

        private void RefreshDisplay()
        {
            ContentPanel.Children.Clear();
            selectedScreenshots.Clear();
            UpdateSelectedCount();
            
            if (allScreenshots.Count == 0)
            {
                var emptyText = new TextBlock
                {
                    Text = "📷 暂无截图\n\n点击主窗口的截图按钮开始截图吧！",
                    FontSize = 16,
                    Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 100, 0, 0)
                };
                ContentPanel.Children.Add(emptyText);
                return;
            }
            
            switch (currentCategory)
            {
                case "date":
                    DisplayByDate();
                    break;
                case "computer":
                    DisplayByComputer();
                    break;
                case "window":
                    DisplayByWindow();
                    break;
            }
        }

        private void DisplayByDate()
        {
            var groupedByDate = allScreenshots
                .GroupBy(s => s.CreatedTime.Date)
                .OrderByDescending(g => g.Key)
                .ToList();
            
            foreach (var group in groupedByDate)
            {
                CreateCategorySection(group.Key.ToString("yyyy年MM月dd日"), group.ToList());
            }
        }

        private void DisplayByComputer()
        {
            var groupedByComputer = allScreenshots
                .GroupBy(s => s.ComputerName)
                .OrderBy(g => g.Key)
                .ToList();
            
            foreach (var group in groupedByComputer)
            {
                CreateCategorySection($"💻 {group.Key}", group.ToList());
            }
        }

        private void DisplayByWindow()
        {
            var groupedByWindow = allScreenshots
                .GroupBy(s => s.WindowName)
                .OrderBy(g => g.Key)
                .ToList();
            
            foreach (var group in groupedByWindow)
            {
                CreateCategorySection($"🪟 {group.Key}", group.ToList());
            }
        }

        private void CreateCategorySection(string categoryName, List<ScreenshotInfo> screenshots)
        {
            // 分类标题
            var titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 20, 0, 10)
            };
            
            var titleText = new TextBlock
            {
                Text = categoryName,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                VerticalAlignment = VerticalAlignment.Center
            };
            
            var countText = new TextBlock
            {
                Text = $"({screenshots.Count} 张)",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            
            titlePanel.Children.Add(titleText);
            titlePanel.Children.Add(countText);
            ContentPanel.Children.Add(titlePanel);
            
            // 缩略图网格
            var thumbnailPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 20)
            };
            
            foreach (var screenshot in screenshots.OrderByDescending(s => s.CreatedTime))
            {
                CreateThumbnail(screenshot, thumbnailPanel);
            }
            
            ContentPanel.Children.Add(thumbnailPanel);
        }

        private void CreateThumbnail(ScreenshotInfo screenshot, WrapPanel parent)
        {
            var border = new Border
            {
                Style = (Style)FindResource("ThumbnailStyle"),
                Width = 200,
                Height = 160,
                Tag = screenshot
            };
            
            var grid = new Grid();
            
            // 缩略图图片
            var image = new Image
            {
                Width = 180,
                Height = 100,
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };
            
            // 异步加载缩略图
            LoadThumbnailAsync(image, screenshot.FilePath);
            
            // 信息面板
            var infoPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 5, 0, 0)
            };
            
            var nameText = new TextBlock
            {
                Text = screenshot.WindowName,
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                TextTrimming = TextTrimming.CharacterEllipsis,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            
            var timeText = new TextBlock
            {
                Text = screenshot.CreatedTime.ToString("MM-dd HH:mm"),
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            
            infoPanel.Children.Add(nameText);
            infoPanel.Children.Add(timeText);
            
            // 选择框
            var checkBox = new CheckBox
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 5, 5, 0),
                Tag = screenshot
            };
            
            checkBox.Checked += (s, e) =>
            {
                if (!selectedScreenshots.Contains(screenshot))
                {
                    selectedScreenshots.Add(screenshot);
                    UpdateSelectedCount();
                }
            };
            
            checkBox.Unchecked += (s, e) =>
            {
                selectedScreenshots.Remove(screenshot);
                UpdateSelectedCount();
            };
            
            grid.Children.Add(image);
            grid.Children.Add(infoPanel);
            grid.Children.Add(checkBox);
            
            border.Child = grid;
            
            // 点击事件
            border.MouseLeftButtonUp += (s, e) =>
            {
                if (e.OriginalSource is CheckBox) return;
                ShowFullImage(screenshot);
            };
            
            // 悬停效果
            border.MouseEnter += (s, e) =>
            {
                var animation = new DoubleAnimation
                {
                    To = 1.05,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                
                var scaleTransform = new ScaleTransform(1, 1);
                border.RenderTransform = scaleTransform;
                border.RenderTransformOrigin = new Point(0.5, 0.5);
                
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            };
            
            border.MouseLeave += (s, e) =>
            {
                var animation = new DoubleAnimation
                {
                    To = 1.0,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                
                if (border.RenderTransform is ScaleTransform scaleTransform)
                {
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
                }
            };
            
            parent.Children.Add(border);
        }

        private async void LoadThumbnailAsync(Image image, string filePath)
        {
            try
            {
                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(filePath);
                        bitmap.DecodePixelWidth = 180;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        
                        image.Source = bitmap;
                    });
                });
            }
            catch
            {
                // 加载失败时显示占位符
                image.Source = null;
            }
        }

        private void ShowFullImage(ScreenshotInfo screenshot)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = screenshot.FilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                StatusText.Text = $"打开图片失败: {ex.Message}";
            }
        }

        private void UpdateSelectedCount()
        {
            SelectedCountText.Text = $"已选择: {selectedScreenshots.Count} 张";
            CopySelectedBtn.IsEnabled = selectedScreenshots.Count > 0;
            DeleteSelectedBtn.IsEnabled = selectedScreenshots.Count > 0;
        }

        // 事件处理方法
        private void DateCategory_Click(object sender, RoutedEventArgs e)
        {
            currentCategory = "date";
            RefreshDisplay();
        }

        private void ComputerCategory_Click(object sender, RoutedEventArgs e)
        {
            currentCategory = "computer";
            RefreshDisplay();
        }

        private void WindowCategory_Click(object sender, RoutedEventArgs e)
        {
            currentCategory = "window";
            RefreshDisplay();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadScreenshots();
        }

        private void CopySelected_Click(object sender, RoutedEventArgs e)
        {
            if (selectedScreenshots.Count == 0) return;
            
            try
            {
                var files = selectedScreenshots.Select(s => s.FilePath).ToArray();
                var fileCollection = new System.Collections.Specialized.StringCollection();
                fileCollection.AddRange(files);
                System.Windows.Forms.Clipboard.SetFileDropList(fileCollection);
                
                StatusText.Text = $"已复制 {selectedScreenshots.Count} 张截图到剪贴板";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"复制失败: {ex.Message}";
            }
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (selectedScreenshots.Count == 0) return;
            
            var result = MessageBox.Show(
                $"确定要删除选中的 {selectedScreenshots.Count} 张截图吗？\n\n此操作不可撤销！",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    foreach (var screenshot in selectedScreenshots.ToList())
                    {
                        File.Delete(screenshot.FilePath);
                        allScreenshots.Remove(screenshot);
                    }
                    
                    selectedScreenshots.Clear();
                    TotalCountText.Text = $"(共 {allScreenshots.Count} 张截图)";
                    RefreshDisplay();
                    StatusText.Text = "删除完成";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"删除失败: {ex.Message}";
                }
            }
        }
    }

    // 截图信息类
    public class ScreenshotInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string WindowName { get; set; } = string.Empty;
        public string ComputerName { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public long FileSize { get; set; }
    }
}