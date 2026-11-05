using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ARK.App
{
    public partial class OrdersBySymbolWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private readonly System.Collections.Generic.List<Position> _workingOrders;

        public OrdersBySymbolWindow()
        {
            InitializeComponent();
            SourceInitialized += OrdersBySymbolWindow_SourceInitialized;

            _workingOrders = Position.GetSampleData()
                .Where(p => p.Status == "Working")
                .ToList();

            SymbolComboBox.Items.Add("All Symbols");
            foreach (var symbol in _workingOrders.Select(o => o.Symbol).Distinct().OrderBy(s => s))
            {
                SymbolComboBox.Items.Add(symbol);
            }
            SymbolComboBox.SelectedIndex = 0;

            RefreshGrid();
        }

        private void SymbolComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            string selected = SymbolComboBox.SelectedItem as string ?? "All Symbols";

            OrdersGrid.ItemsSource = selected == "All Symbols"
                ? _workingOrders
                : _workingOrders.Where(o => o.Symbol == selected).ToList();
        }

        private void OrdersBySymbolWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int useDarkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
        }
    }
}