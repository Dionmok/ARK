using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ARK.App
{
    public static class AppActions
    {
        public static void OpenWatchlist()
        {
            new WatchlistWindow().Show();
        }

        public static void OpenQuoteboard()
        {
            new QuoteboardWindow().Show();
        }

        public static void OpenTimeAndSales()
        {
            new TimeAndSalesWindow().Show();
        }

        public static void OpenTimeAndSales(string symbol)
        {
            new TimeAndSalesWindow(symbol).Show();
        }

        public static void OpenNewChart()
        {
            new ChartWindow().Show();
        }

        public static void OpenPositions()
        {
            new PositionsWindow().Show();
        }

        public static void OpenTradeBook()
        {
            new TradeBookWindow().Show();
        }

        public static void OpenGlobalOrders()
        {
            new GlobalOrdersWindow().Show();
        }

        public static void OpenOrdersBySymbol()
        {
            new OrdersBySymbolWindow().Show();
        }

        public static void OpenFindSymbol(Window owner)
        {
            var dialog = new ARK.Charting.SymbolDialog
            {
                Owner = owner
            };
            dialog.ShowDialog();
        }

        public static void OpenChartbookManager(Window owner)
        {
            var chartbookManagerWindow = new ChartbookManagerWindow
            {
                Owner = owner
            };
            chartbookManagerWindow.ShowDialog();
        }

        public static void OpenGeneralSettings(Window owner)
        {
            OpenSettings(owner, "General", new TextBlock
            {
                Text = "General settings will go here.",
                Margin = new Thickness(4)
            });
        }

        public static void OpenDataFeedsSettings(Window owner)
        {
            OpenSettings(owner, "Data Feeds", new TextBlock
            {
                Text = "Data feed connection settings will go here.",
                Margin = new Thickness(4)
            });
        }

        public static void OpenHotkeysSettings(Window owner)
        {
            OpenSettings(owner, "Hotkeys", new TextBlock
            {
                Text = "Hotkey settings will go here.",
                Margin = new Thickness(4)
            });
        }

        public static void OpenAppearanceSettings(Window owner)
        {
            OpenSettings(owner, "Appearance", new SettingsPages.AppearanceSettingsPage());
        }

        public static void OpenChartSettings(Window owner)
        {
            var categories = new List<SettingsCategory>
            {
                new SettingsCategory
                {
                    Name = "Chart Settings",
                    Content = new TextBlock
                    {
                        Text = "Chart scaling, update behavior, and time zone settings will go here.",
                        Margin = new Thickness(4)
                    }
                }
            };

            var settingsWindow = new SettingsWindow("Chart Settings", categories, "Chart Settings")
            {
                Owner = owner
            };
            settingsWindow.ShowDialog();
        }

        private static void OpenSettings(Window owner, string categoryName, UIElement content)
        {
            var categories = new List<SettingsCategory>
            {
                new SettingsCategory { Name = categoryName, Content = content }
            };

            var settingsWindow = new SettingsWindow("Settings", categories, categoryName)
            {
                Owner = owner
            };
            settingsWindow.ShowDialog();
        }
    }
}
