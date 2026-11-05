using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace ARK.App
{
    public partial class TradeBookWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private readonly List<TradeRecord> _allTrades;
        private Border? _calendarHost;

        public TradeBookWindow()
        {
            InitializeComponent();
            SourceInitialized += TradeBookWindow_SourceInitialized;
            _allTrades = TradeRecord.GetSampleData(12);

            EndDatePicker.SelectedDate = DateTime.Today;
            StartDatePicker.SelectedDate = DateTime.Today.AddMonths(-6);
            ContinuousCheckBox.IsChecked = true;
            EndDatePicker.IsEnabled = false;

            AccountComboBox.Items.Add("Overall / Cumulative P&L");
            foreach (var account in TradeRecord.SampleAccounts)
            {
                AccountComboBox.Items.Add(account);
            }
            AccountComboBox.SelectedIndex = 0;

            ViewModeComboBox.Items.Add("Calendar");
            ViewModeComboBox.Items.Add("Bar Graph");
            ViewModeComboBox.SelectedIndex = 0;

            RefreshView();
        }

        private void ViewModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshView();
        }

        private void DateRange_Changed(object sender, SelectionChangedEventArgs e)
        {
            RefreshView();
        }

        private void ContinuousCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            EndDatePicker.IsEnabled = ContinuousCheckBox.IsChecked != true;
            RefreshView();
        }

        private void RefreshView()
        {
            if (StartDatePicker.SelectedDate == null) return;
            if (AccountComboBox.SelectedItem == null) return;

            DateTime start = StartDatePicker.SelectedDate.Value;
            DateTime end = ContinuousCheckBox.IsChecked == true
                ? DateTime.Today
                : (EndDatePicker.SelectedDate ?? DateTime.Today);

            string selectedAccount = (string)AccountComboBox.SelectedItem;

            var filtered = _allTrades
                .Where(t => t.Date.Date >= start.Date && t.Date.Date <= end.Date)
                .Where(t => selectedAccount == "Overall / Cumulative P&L" || t.Account == selectedAccount)
                .ToList();

            LoadStats(filtered);

            string viewMode = ViewModeComboBox.SelectedItem as string ?? "Calendar";
            if (viewMode == "Calendar")
            {
                LoadCalendar(filtered);
            }
            else if (viewMode == "Bar Graph")
            {
                LoadBarGraph(filtered);
            }

            LoadEquityCurve(filtered);
        }

        private void TradeBookWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int useDarkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
        }

        private void LoadStats(List<TradeRecord> trades)
        {
            var stats = TradeStats.Calculate(trades);

            StatsGrid.Children.Clear();
            AddStat("Win Rate", $"{stats.WinRate:0.0}%");
            AddStat("Average Win", $"${stats.AverageWin:0.00}");
            AddStat("Average Loss", $"${stats.AverageLoss:0.00}");
            AddStat("Biggest Win", $"${stats.BiggestWin:0.00}");
            AddStat("Biggest Loss", $"${stats.BiggestLoss:0.00}");
            AddStat("Max Favorable Excursion", $"${stats.MaxFavorableExcursion:0.00}");
            AddStat("Max Adverse Excursion", $"${stats.MaxAdverseExcursion:0.00}");
        }

        private void AddStat(string label, string value)
        {
            var panel = new StackPanel { Margin = new Thickness(4) };
            panel.Children.Add(new TextBlock
            {
                Text = label,
                Foreground = (Brush)FindResource("TextSecondaryBrush"),
                FontSize = 11
            });
            panel.Children.Add(new TextBlock
            {
                Text = value,
                Foreground = (Brush)FindResource("TextPrimaryBrush"),
                FontSize = 16,
                FontWeight = FontWeights.Bold
            });
            StatsGrid.Children.Add(panel);
        }

        private void LoadCalendar(List<TradeRecord> trades)
        {
            var byDay = trades
                .GroupBy(t => t.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.ProfitLoss));

            var months = trades
                .Select(t => new DateTime(t.Date.Year, t.Date.Month, 1))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var outerPanel = new StackPanel();

            foreach (var month in months)
            {
                outerPanel.Children.Add(BuildMonthBlock(month, byDay));
            }

            _calendarHost ??= (Border)CalendarGrid.Parent;
            _calendarHost.Child = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = outerPanel
            };
        }

        private UIElement BuildMonthBlock(DateTime month, Dictionary<DateTime, double> byDay)
        {
            var container = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };

            container.Children.Add(new TextBlock
            {
                Text = month.ToString("MMMM yyyy"),
                Foreground = (Brush)FindResource("TextPrimaryBrush"),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(2, 0, 0, 6)
            });

            var grid = new Grid();
            for (int i = 0; i < 7; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition());

            int firstDayOfWeek = (int)new DateTime(month.Year, month.Month, 1).DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
            int totalRows = (int)Math.Ceiling((firstDayOfWeek + daysInMonth) / 7.0);

            for (int r = 0; r < totalRows; r++)
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(month.Year, month.Month, day);
                int cellIndex = firstDayOfWeek + day - 1;
                int row = cellIndex / 7;
                int col = cellIndex % 7;

                double? pl = byDay.TryGetValue(date, out var value) ? value : null;

                var cell = BuildDayCell(day, pl);
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, col);
                grid.Children.Add(cell);
            }

            container.Children.Add(grid);
            return container;
        }

        private Border BuildDayCell(int day, double? profitLoss)
        {
            Brush background = (Brush)FindResource("Bg2Brush");
            if (profitLoss.HasValue)
            {
                background = profitLoss.Value >= 0
                    ? new SolidColorBrush(Color.FromRgb(0x2E, 0x5D, 0x34))
                    : new SolidColorBrush(Color.FromRgb(0x5D, 0x2E, 0x2E));
            }

            var stack = new StackPanel { Margin = new Thickness(2) };
            stack.Children.Add(new TextBlock
            {
                Text = day.ToString(),
                Foreground = (Brush)FindResource("TextSecondaryBrush"),
                FontSize = 10
            });

            if (profitLoss.HasValue)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = $"{(profitLoss.Value >= 0 ? "+" : "")}{profitLoss.Value:0.0}",
                    Foreground = (Brush)FindResource("TextPrimaryBrush"),
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold
                });
            }

            return new Border
            {
                Background = background,
                BorderBrush = (Brush)FindResource("Bg0Brush"),
                BorderThickness = new Thickness(1),
                Child = stack
            };
        }

        private void LoadBarGraph(List<TradeRecord> trades)
        {
            var byDay = trades
                .GroupBy(t => t.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.ProfitLoss));

            var months = trades
                .Select(t => new DateTime(t.Date.Year, t.Date.Month, 1))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            double maxAbs = byDay.Count > 0
                ? Math.Max(1, byDay.Values.Max(v => Math.Abs(v)))
                : 1;

            var outerPanel = new StackPanel();

            foreach (var month in months)
            {
                outerPanel.Children.Add(BuildMonthBarChart(month, byDay, maxAbs));
            }

            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = outerPanel
            };

            _calendarHost ??= (Border)CalendarGrid.Parent;
            _calendarHost.Child = scroll;
        }

        private ScrollViewer? FindParentScrollViewer(DependencyObject child)
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && parent is not ScrollViewer)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as ScrollViewer;
        }

        private UIElement BuildMonthBarChart(DateTime month, Dictionary<DateTime, double> byDay, double maxAbs)
        {
            const double chartHeight = 160;
            double halfHeight = chartHeight / 2;

            var container = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };

            container.Children.Add(new TextBlock
            {
                Text = month.ToString("MMMM yyyy"),
                Foreground = (Brush)FindResource("TextPrimaryBrush"),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(2, 0, 0, 6)
            });

            var outerGrid = new Grid();
            outerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            outerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var axisPanel = new StackPanel { Width = 50 };
            for (int i = 4; i >= -4; i--)
            {
                double value = maxAbs * i / 4;
                axisPanel.Children.Add(new TextBlock
                {
                    Text = value.ToString("0"),
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    FontSize = 10,
                    Height = chartHeight / 8,
                    TextAlignment = TextAlignment.Right,
                    Margin = new Thickness(0, 0, 6, 0)
                });
            }
            Grid.SetColumn(axisPanel, 0);
            outerGrid.Children.Add(axisPanel);

            var chartArea = new Grid();
            var barsColumn = new StackPanel();
            var barsRow = new StackPanel { Orientation = Orientation.Horizontal, Height = chartHeight };
            var labelsRow = new StackPanel { Orientation = Orientation.Horizontal };

            var zeroLine = new Border
            {
                Height = 1,
                Background = (Brush)FindResource("TextSecondaryBrush"),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, halfHeight, 0, 0),
                IsHitTestVisible = false
            };

            var tradedDaysThisMonth = byDay.Keys
                .Where(d => d.Year == month.Year && d.Month == month.Month)
                .OrderBy(d => d)
                .ToList();

            foreach (var date in tradedDaysThisMonth)
            {
                double pl = byDay[date];

                double barHeight = Math.Abs(pl) / maxAbs * halfHeight;
                barHeight = Math.Max(2, barHeight);

                var barContainer = new Grid { Width = 18, Height = chartHeight };

                var bar = new Border
                {
                    Width = 12,
                    Height = barHeight,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = pl >= 0
                        ? new Thickness(0, halfHeight - barHeight, 0, 0)
                        : new Thickness(0, halfHeight, 0, 0),
                    Background = pl >= 0
                        ? new SolidColorBrush(Color.FromRgb(0x3D, 0xA3, 0x5D))
                        : new SolidColorBrush(Color.FromRgb(0xC9, 0x4F, 0x4F)),
                    ToolTip = $"{date:MMM d}: {(pl >= 0 ? "+" : "")}{pl:0.0}"
                };
                barContainer.Children.Add(bar);

                barsRow.Children.Add(barContainer);

                labelsRow.Children.Add(new TextBlock
                {
                    Text = date.Day.ToString(),
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    FontSize = 9,
                    Width = 18,
                    TextAlignment = TextAlignment.Center
                });
            }

            chartArea.Children.Add(barsRow);
            chartArea.Children.Add(zeroLine);
            barsColumn.Children.Add(chartArea);
            barsColumn.Children.Add(new Border
            {
                Height = 1,
                Background = (Brush)FindResource("Bg2Brush"),
                Margin = new Thickness(0, 4, 0, 4)
            });
            barsColumn.Children.Add(labelsRow);

            var monthScroll = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = barsColumn
            };
            monthScroll.PreviewMouseWheel += (s, e) =>
            {
                e.Handled = true;
                var parentScroll = FindParentScrollViewer(monthScroll);
                parentScroll?.ScrollToVerticalOffset(parentScroll.VerticalOffset - e.Delta);
            };
            Grid.SetColumn(monthScroll, 1);
            outerGrid.Children.Add(monthScroll);

            container.Children.Add(outerGrid);
            return container;
        }

        private void LoadEquityCurve(List<TradeRecord> trades)
        {
            var byDay = trades
                .GroupBy(t => t.Date.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Date = g.Key, ProfitLoss = g.Sum(t => t.ProfitLoss) })
                .ToList();

            var points = new List<(DateTime Date, double Cumulative)>();
            double running = 0;
            foreach (var day in byDay)
            {
                running += day.ProfitLoss;
                points.Add((day.Date, running));
            }

            const double chartHeight = 260;
            const double chartWidth = 220;

            var canvas = new Canvas { Width = chartWidth, Height = chartHeight };

            if (points.Count == 0)
            {
                canvas.Children.Add(new TextBlock
                {
                    Text = "No trades in range.",
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    FontSize = 11
                });
                EquityCurveHost.Items.Clear();
                EquityCurveHost.Items.Add(canvas);
                return;
            }

            double minVal = Math.Min(0, points.Min(p => p.Cumulative));
            double maxVal = Math.Max(0, points.Max(p => p.Cumulative));
            double range = Math.Max(1, maxVal - minVal);

            double zeroY = chartHeight - ((0 - minVal) / range * chartHeight);

            var zeroLine = new System.Windows.Shapes.Line
            {
                X1 = 0,
                Y1 = zeroY,
                X2 = chartWidth,
                Y2 = zeroY,
                Stroke = (Brush)FindResource("TextSecondaryBrush"),
                StrokeThickness = 1
            };
            canvas.Children.Add(zeroLine);

            var polyline = new System.Windows.Shapes.Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(0x3D, 0xA3, 0xD9)),
                StrokeThickness = 2
            };

            double xStep = points.Count > 1 ? chartWidth / (points.Count - 1) : 0;

            for (int i = 0; i < points.Count; i++)
            {
                double x = i * xStep;
                double y = chartHeight - ((points[i].Cumulative - minVal) / range * chartHeight);
                polyline.Points.Add(new Point(x, y));
            }

            canvas.Children.Add(polyline);

            var endLabel = new TextBlock
            {
                Text = $"{(running >= 0 ? "+" : "")}{running:0.0}",
                Foreground = (Brush)FindResource("TextPrimaryBrush"),
                FontSize = 11,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(endLabel, 4);
            Canvas.SetTop(endLabel, 4);
            canvas.Children.Add(endLabel);

            var border = new Border
            {
                Background = (Brush)FindResource("Bg2Brush"),
                Child = canvas
            };

            EquityCurveHost.Items.Clear();
            EquityCurveHost.Items.Add(border);
        }
    }
}
