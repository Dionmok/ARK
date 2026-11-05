using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace ARK.Desktop;

internal sealed class MainWindow : Window
{
    private static readonly string[] Symbols =
    [
        "ES", "NQ", "YM", "RTY", "CL", "GC", "SI", "AAPL", "MSFT",
        "GOOGL", "AMZN", "TSLA", "NVDA", "META", "SPY", "QQQ"
    ];

    public MainWindow()
    {
        Title = "ARK";
        Width = 1000;
        Height = 650;
        MinWidth = 360;
        MinHeight = 260;

        var chart = new CandlestickChart();
        var symbols = new ComboBox { ItemsSource = Symbols, MinWidth = 100 };
        symbols.SelectionChanged += (_, _) =>
        {
            if (symbols.SelectedItem is string symbol)
                chart.SetSymbol(symbol);
        };
        symbols.SelectedIndex = 0;

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 0, 0, 12),
            Children =
            {
                new TextBlock { Text = "Symbol", VerticalAlignment = VerticalAlignment.Center },
                symbols,
                new TextBlock { Text = "Sample data", VerticalAlignment = VerticalAlignment.Center }
            }
        };

        var layout = new DockPanel { Margin = new Thickness(16) };
        DockPanel.SetDock(toolbar, Dock.Top);
        layout.Children.Add(toolbar);
        layout.Children.Add(chart);
        Content = layout;
    }
}
