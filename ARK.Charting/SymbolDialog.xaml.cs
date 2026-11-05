using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace ARK.Charting
{
    public partial class SymbolDialog : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private static readonly List<string> AllSymbols = new()
        {
            "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA", "NVDA", "META",
            "SPY", "QQQ", "ES", "NQ", "YM", "RTY", "CL", "GC", "SI"
        };

        public string? SelectedSymbol { get; private set; }
        public bool IsCrossTradingEnabled { get; private set; }
        public string? CrossTradeSymbol { get; private set; }

        public SymbolDialog(bool showCrossTrading = true)
        {
            InitializeComponent();
            SourceInitialized += SymbolDialog_SourceInitialized;
            ResultsList.ItemsSource = AllSymbols;
            CrossTradeSymbolList.ItemsSource = AllSymbols;
            CrossTradingCheckBox.Checked += CrossTradingCheckBox_Toggled;
            CrossTradingCheckBox.Unchecked += CrossTradingCheckBox_Toggled;

            if (!showCrossTrading)
            {
                CrossTradingPanel.Visibility = Visibility.Collapsed;
                ChartSymbolColumn.Width = new GridLength(1, GridUnitType.Star);
                CrossTradeColumn.Width = new GridLength(0);
                SpacerColumn.Width = new GridLength(0);
            }
        }

        private void CrossTradingCheckBox_Toggled(object sender, RoutedEventArgs e)
        {
            bool enabled = CrossTradingCheckBox.IsChecked == true;
            CrossTradeOverlay.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SymbolDialog_SourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int useDarkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var query = SearchBox.Text.Trim();
            ResultsList.ItemsSource = string.IsNullOrEmpty(query)
                ? AllSymbols
                : AllSymbols.Where(s => s.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void ResultsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ConfirmSelection();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (ResultsList.SelectedItem is string symbol)
            {
                SelectedSymbol = symbol;
                IsCrossTradingEnabled = CrossTradingCheckBox.IsChecked == true;
                CrossTradeSymbol = CrossTradeSymbolList.SelectedItem as string;
                DialogResult = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}