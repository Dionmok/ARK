using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ARK.App
{
    public class SettingsCategory
    {
        public string Name { get; set; }
        public UIElement Content { get; set; }
    }

    public partial class SettingsWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public SettingsWindow(string title, List<SettingsCategory> categories, string initialCategory = null)
        {
            InitializeComponent();
            Title = title;
            SourceInitialized += SettingsWindow_SourceInitialized;

            CategoryList.ItemsSource = categories;

            SettingsCategory toSelect = null;
            if (initialCategory != null)
            {
                foreach (var c in categories)
                {
                    if (c.Name == initialCategory)
                    {
                        toSelect = c;
                        break;
                    }
                }
            }

            CategoryList.SelectedItem = toSelect ?? (categories.Count > 0 ? categories[0] : null);
        }

        private void SettingsWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int useDarkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
        }

        private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryList.SelectedItem is SettingsCategory category)
            {
                CategoryContent.Content = category.Content;
            }
        }
    }
}