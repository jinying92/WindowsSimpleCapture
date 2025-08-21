using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WindowsSimpleCapture
{
    public partial class BatchDeleteDialog : Window
    {
        private List<ScreenshotInfo> allScreenshots;
        private List<ScreenshotInfo> filteredScreenshots = new List<ScreenshotInfo>();
        
        public bool IsConfirmed { get; private set; } = false;
        public List<ScreenshotInfo> ScreenshotsToDelete { get; private set; } = new List<ScreenshotInfo>();
        
        public BatchDeleteDialog(List<ScreenshotInfo> screenshots)
        {
            try
            {
                InitializeComponent();
                allScreenshots = screenshots ?? new List<ScreenshotInfo>();
                InitializeFilters();
                UpdatePreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"BatchDeleteDialog初始化失败: {ex.Message}\n\n{ex.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
        
        private void InitializeFilters()
        {
            // 默认选择全部，但不触发事件
            DeleteAllRadio.Checked -= FilterChanged;
            DeleteAllRadio.IsChecked = true;
            DeleteAllRadio.Checked += FilterChanged;
            
            // 初始化筛选状态
            FilterOptionsPanel.Visibility = Visibility.Collapsed;
            filteredScreenshots = allScreenshots.ToList();
        }
        
        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            // 确保所有控件都已初始化
            if (DeleteAllRadio == null || FilterOptionsPanel == null || allScreenshots == null)
                return;
                
            if (DeleteAllRadio.IsChecked == true)
            {
                FilterOptionsPanel.Visibility = Visibility.Collapsed;
                filteredScreenshots = allScreenshots.ToList();
            }
            else if (DeleteByDateRadio?.IsChecked == true)
            {
                ShowDateFilter();
            }
            else if (DeleteByComputerRadio?.IsChecked == true)
            {
                ShowComputerFilter();
            }
            else if (DeleteByWindowRadio?.IsChecked == true)
            {
                ShowWindowFilter();
            }
            
            UpdatePreview();
        }
        
        private void ShowDateFilter()
        {
            FilterOptionsPanel.Visibility = Visibility.Visible;
            FilterLabel.Text = "选择日期:";
            
            var dates = allScreenshots
                .Select(s => s.CreatedTime.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .Select(d => new { Display = d.ToString("yyyy年MM月dd日"), Value = d })
                .ToList();
            
            FilterComboBox.ItemsSource = dates;
            FilterComboBox.DisplayMemberPath = "Display";
            FilterComboBox.SelectedValuePath = "Value";
            
            if (dates.Any())
            {
                FilterComboBox.SelectedIndex = 0;
            }
        }
        
        private void ShowComputerFilter()
        {
            FilterOptionsPanel.Visibility = Visibility.Visible;
            FilterLabel.Text = "选择计算机:";
            
            var computers = allScreenshots
                .Select(s => s.ComputerName)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            
            FilterComboBox.ItemsSource = computers;
            FilterComboBox.DisplayMemberPath = null;
            FilterComboBox.SelectedValuePath = null;
            
            if (computers.Any())
            {
                FilterComboBox.SelectedIndex = 0;
            }
        }
        
        private void ShowWindowFilter()
        {
            FilterOptionsPanel.Visibility = Visibility.Visible;
            FilterLabel.Text = "选择窗口:";
            
            var windows = allScreenshots
                .Select(s => s.WindowName)
                .Distinct()
                .OrderBy(w => w)
                .ToList();
            
            FilterComboBox.ItemsSource = windows;
            FilterComboBox.DisplayMemberPath = null;
            FilterComboBox.SelectedValuePath = null;
            
            if (windows.Any())
            {
                FilterComboBox.SelectedIndex = 0;
            }
        }
        
        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterComboBox.SelectedItem == null)
            {
                filteredScreenshots.Clear();
                UpdatePreview();
                return;
            }
            
            if (DeleteByDateRadio.IsChecked == true)
            {
                var selectedDate = (DateTime)FilterComboBox.SelectedValue;
                filteredScreenshots = allScreenshots
                    .Where(s => s.CreatedTime.Date == selectedDate)
                    .ToList();
            }
            else if (DeleteByComputerRadio.IsChecked == true)
            {
                var selectedComputer = FilterComboBox.SelectedItem.ToString();
                filteredScreenshots = allScreenshots
                    .Where(s => s.ComputerName == selectedComputer)
                    .ToList();
            }
            else if (DeleteByWindowRadio.IsChecked == true)
            {
                var selectedWindow = FilterComboBox.SelectedItem.ToString();
                filteredScreenshots = allScreenshots
                    .Where(s => s.WindowName == selectedWindow)
                    .ToList();
            }
            
            UpdatePreview();
        }
        
        private void UpdatePreview()
        {
            PreviewListBox.ItemsSource = filteredScreenshots.OrderByDescending(s => s.CreatedTime);
            CountLabel.Text = $"共 {filteredScreenshots.Count} 张截图";
            DeleteBtn.IsEnabled = filteredScreenshots.Count > 0;
            
            if (filteredScreenshots.Count == 0)
            {
                PreviewLabel.Text = "没有符合条件的截图";
            }
            else
            {
                PreviewLabel.Text = "将要删除的截图:";
            }
        }
        
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (filteredScreenshots.Count == 0)
            {
                MessageBox.Show("没有可删除的截图！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var result = MessageBox.Show(
                $"确定要删除选中的 {filteredScreenshots.Count} 张截图吗？\n\n此操作不可撤销！",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                ScreenshotsToDelete = filteredScreenshots.ToList();
                IsConfirmed = true;
                DialogResult = true;
                Close();
            }
        }
        
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }
    }
}