using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace ARK.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += MainWindow_SourceInitialized;
            BuildRegistryMenus();
        }

        private void BuildRegistryMenus()
        {
            var parentMenus = new Dictionary<string, MenuItem>
            {
                { "File", FileMenu },
                { "View", ViewMenu },
                { "Trade", TradeMenu },
                { "Main Settings", MainSettingsMenu },
                { "Chartbooks", ChartbooksMenu }
            };

            foreach (var def in MenuRegistry.Items)
            {
                if (def.Scope != MenuScope.Shared && def.Scope != MenuScope.MainOnly)
                    continue;

                if (!parentMenus.TryGetValue(def.ParentMenu, out var parentMenu))
                    continue;

                var menuItem = new MenuItem { Header = def.Header };
                menuItem.Click += (s, e) => def.Action(this);

                int insertIndex = def.ParentMenu == "Trade" ? 2 : 0;
                parentMenu.Items.Insert(insertIndex, menuItem);
            }
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int useDarkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NotImplemented_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This feature isn't built yet.", "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TimeAndSales_Click(object sender, RoutedEventArgs e)
        {
            AppActions.OpenTimeAndSales();
        }
    }
}
