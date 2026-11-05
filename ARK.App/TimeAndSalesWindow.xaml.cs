using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ARK.App
{
    public partial class TimeAndSalesWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private string _currentSymbol = "ES";

        public TimeAndSalesWindow(string? initialSymbol = null)
        {
            InitializeComponent();
            SourceInitialized += TimeAndSalesWindow_SourceInitialized;
            LoadSymbol(initialSymbol ?? _currentSymbol);
        }

        private void LoadSymbol(string symbol)
        {
            _currentSymbol = symbol;
            CurrentSymbolLabel.Text = symbol;
            TimeAndSalesGrid.ItemsSource = TimeAndSalesEntry.GetSampleData();
        }

        private void FindSymbolButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ARK.Charting.SymbolDialog(showCrossTrading: false)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && dialog.SelectedSymbol != null)
            {
                LoadSymbol(dialog.SelectedSymbol);
            }
        }

        private void TimeAndSalesWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int useDarkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
        }
    }
}