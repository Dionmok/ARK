using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ARK.Core;

namespace ARK.App
{
    public partial class QuoteboardWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private readonly ObservableCollection<Quote> _quotes;

        public QuoteboardWindow()
        {
            InitializeComponent();
            SourceInitialized += QuoteboardWindow_SourceInitialized;

            _quotes = new ObservableCollection<Quote>(Quote.GetSampleData());
            QuoteboardGrid.ItemsSource = _quotes;
        }

        private void QuoteboardWindow_SourceInitialized(object? sender, EventArgs e)
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

            double basePrice = SampleData.RandomDouble(50, 5000);
            double change = SampleData.RandomDouble(-50, 50);
            double last = basePrice + change;
            double spread = SampleData.RandomDouble(0.25, 2);

            _quotes.Add(new Quote
            {
                Symbol = symbol,
                Last = Math.Round(last, 2),
                Bid = Math.Round(last - spread / 2, 2),
                Ask = Math.Round(last + spread / 2, 2),
                ContractVolume = SampleData.RandomInt(100000, 3000000)
            });
        }

        private void RemoveSymbol_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is Quote quote)
            {
                _quotes.Remove(quote);
            }
        }
    }
}