using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ARK.Core;

namespace ARK.App
{
    public partial class WatchlistWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private readonly ObservableCollection<WatchlistQuote> _quotes;

        public WatchlistWindow()
        {
            InitializeComponent();
            SourceInitialized += WatchlistWindow_SourceInitialized;

            _quotes = new ObservableCollection<WatchlistQuote>(WatchlistQuote.GetSampleData());
            WatchlistGrid.ItemsSource = _quotes;
        }

        private void WatchlistWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int useDarkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
        }

        private void FindSymbolButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ARK.Charting.SymbolDialog(showCrossTrading: false)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && dialog.SelectedSymbol != null)
            {
                AddSymbol(dialog.SelectedSymbol);
            }
        }

        private void AddSymbol(string symbol)
        {
            if (_quotes.Any(q => q.Symbol == symbol))
                return;

            double basePrice = SampleData.RandomDouble(20, 800);
            double change = SampleData.RandomDouble(-15, 15);
            double last = basePrice + change;
            double percentChange = basePrice != 0 ? (change / basePrice) * 100 : 0;
            double high = last + SampleData.RandomDouble(0, 10);
            double low = last - SampleData.RandomDouble(0, 10);

            _quotes.Add(new WatchlistQuote
            {
                Symbol = symbol,
                Last = Math.Round(last, 2),
                Change = Math.Round(change, 2),
                PercentChange = Math.Round(percentChange, 2),
                Volume = SampleData.RandomInt(10000, 5000000),
                High = Math.Round(high, 2),
                Low = Math.Round(low, 2)
            });
        }

        private void RemoveSymbol_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is WatchlistQuote quote)
            {
                _quotes.Remove(quote);
            }
        }
    }
}