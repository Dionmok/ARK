using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using ARK.Charting;

namespace ARK.App
{
    public partial class ChartWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public ChartWindow()
        {
            InitializeComponent();
            SourceInitialized += ChartWindow_SourceInitialized;
            ChartControl.WindowActionRequested += ChartControl_WindowActionRequested;
            ChartControl.SymbolChanged += ChartControl_SymbolChanged;
            BuildRegistryMenus();
            UpdateTitle();
        }

        private void ChartControl_SymbolChanged(string symbol)
        {
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            Title = $"Chart — {ChartControl.CurrentSymbol}";
        }

        private void BuildRegistryMenus()
        {
            var parentMenus = new Dictionary<string, MenuItem>
            {
                { "File", ChartControl.GetMenu("File") },
                { "View", ChartControl.GetMenu("View") },
                { "Trade", ChartControl.GetMenu("Trade") }
            };

            foreach (var def in MenuRegistry.Items)
            {
                if (def.Scope != MenuScope.Shared && def.Scope != MenuScope.ChartOnly)
                    continue;

                if (!parentMenus.TryGetValue(def.ParentMenu, out var parentMenu))
                    continue;

                var menuItem = new MenuItem { Header = def.Header };
                menuItem.Click += (s, e) => def.Action(this);

                int insertIndex = def.ParentMenu == "Trade" ? 2 : 0;
                parentMenu.Items.Insert(insertIndex, menuItem);
            }
        }

        private void ChartWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int useDarkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
        }

        private void ChartControl_WindowActionRequested(WindowAction action)
        {
            switch (action)
            {
                case WindowAction.Watchlist:
                    AppActions.OpenWatchlist();
                    break;

                case WindowAction.Quoteboard:
                    AppActions.OpenQuoteboard();
                    break;

                case WindowAction.TimeAndSales:
                    AppActions.OpenTimeAndSales(ChartControl.CurrentSymbol);
                    break;

                case WindowAction.DuplicateChart:
                    DuplicateChart();
                    break;

                case WindowAction.ChartSettings:
                    AppActions.OpenChartSettings(this);
                    break;
            }
        }

        private void DuplicateChart()
        {
            var newChart = new ChartWindow();
            newChart.ChartControl.SetSymbol(ChartControl.CurrentSymbol);
            newChart.Show();
        }
    }
}
