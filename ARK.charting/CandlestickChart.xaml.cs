using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace ARK.Charting
{
    public partial class CandlestickChart : UserControl
    {
        public ChartViewSettings ViewSettings { get; set; } = new ChartViewSettings();

        public string CurrentSymbol { get; private set; } = "ES";
        public bool IsCrossTradingEnabled { get; private set; }
        public string? CrossTradeSymbol { get; private set; }

        public event Action<WindowAction>? WindowActionRequested;
        public event Action<string>? SymbolChanged;

        public MenuItem GetMenu(string name)
        {
            return name switch
            {
                "File" => (MenuItem)FindName("FileMenu"),
                "View" => (MenuItem)FindName("ViewMenu"),
                "Trade" => (MenuItem)FindName("TradeMenu"),
                _ => throw new ArgumentException($"Unknown menu: {name}")
            };
        }

        public void SetSymbol(string symbol)
        {
            CurrentSymbol = symbol;
            LoadBarsForSymbol(symbol);
            SymbolChanged?.Invoke(CurrentSymbol);
        }

        public static readonly DependencyProperty MenuVisibilityProperty =
            DependencyProperty.Register(nameof(MenuVisibility), typeof(Visibility), typeof(CandlestickChart),
                new PropertyMetadata(Visibility.Visible));

        public Visibility MenuVisibility
        {
            get => (Visibility)GetValue(MenuVisibilityProperty);
            set => SetValue(MenuVisibilityProperty, value);
        }

        private List<Bar> _bars;

        // CORE STATE - just these two things define what's visible
        private double _startIndex;
        private double _visibleBarCount = 60;

        private double _verticalZoomFactor = 1.0;
        private float _verticalPanOffset = 0f;
        private float _lastPricePerPixel = 0f;

        private bool _isDragging;
        private bool _isDraggingPriceAxis;
        private bool _isDraggingTimeAxis;
        private double _lockedFillSpaceBars;
        private Point _lastMousePos;
        private double _axisDragStartVerticalZoom;
        private Point _axisDragStartPos;
        private double _axisDragStartVisibleBarCount;
        private double _axisDragStartRightEdge;

        private const float BottomAxisHeight = 24f;
        private const float TopMargin = 10f;
        private const float LeftMargin = 8f;

        private float _plotRight;
        private float _plotBottom;
        private double _plotRightFraction;
        private double _plotBottomFraction;
        private float _lastPlotWidth;

        public CandlestickChart()
        {
            InitializeComponent();
            _bars = new List<Bar>(GenerateSampleBars(450, CurrentSymbol));
            _startIndex = Math.Max(0, _bars.Count - _visibleBarCount);
            UpdateScrollBar();
        }

        private void LoadBarsForSymbol(string symbol)
        {
            _bars.Clear();
            _bars.AddRange(GenerateSampleBars(450, symbol));
            _startIndex = Math.Max(0, _bars.Count - _visibleBarCount);
            _verticalPanOffset = 0f;
            _verticalZoomFactor = 1.0;
            UpdateScrollBar();
            SkCanvas.InvalidateVisual();
        }

        private void NotImplemented_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This feature isn't built yet.", "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LockHandPanning_Click(object sender, RoutedEventArgs e)
        {
            ViewSettings.LockPanning = LockHandPanningItem.IsChecked;
        }

        private void LockFillSpace_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[LockFillSpace_Click ENTERED] sender={sender}, IsChecked={LockFillSpaceItem.IsChecked}");

            ViewSettings.LockFillSpace = LockFillSpaceItem.IsChecked;

            if (ViewSettings.LockFillSpace)
            {
                _lockedFillSpaceBars = Math.Max(0, (_startIndex + _visibleBarCount) - _bars.Count);
            }

            ClampStartIndex();
            UpdateScrollBar();
            SkCanvas.InvalidateVisual();
        }
        private void LogState(string eventName)
        {
            double n = _bars.Count;
            double v = _visibleBarCount;
            double s = _startIndex;
            double l = _lockedFillSpaceBars;
            double max = GetMaxStartIndex();
            double anchor = n + l - v;
            double rightEdge = s + v;

            System.Diagnostics.Debug.WriteLine(
                $"[{eventName}] N={n} V={v:0.###} S={s:0.###} L={l:0.###} Max={max:0.###} Anchor={anchor:0.###} RightEdge={rightEdge:0.###}");
        }

        private void WindowAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is WindowAction action)
            {
                WindowActionRequested?.Invoke(action);
            }
        }

        private void Symbol_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SymbolDialog
            {
                Owner = System.Windows.Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.SelectedSymbol != null)
            {
                CurrentSymbol = dialog.SelectedSymbol;
                IsCrossTradingEnabled = dialog.IsCrossTradingEnabled;
                CrossTradeSymbol = dialog.CrossTradeSymbol;
                LoadBarsForSymbol(CurrentSymbol);
                SymbolChanged?.Invoke(CurrentSymbol);
            }
        }

        private static List<Bar> GenerateSampleBars(int count, string symbol)
        {
            var bars = new List<Bar>(count);
            var rng = new Random(symbol.GetHashCode());
            float price = 100f + Math.Abs(symbol.GetHashCode() % 400);
            var startTime = new DateTime(2026, 8, 21, 8, 0, 0);

            for (int i = 0; i < count; i++)
            {
                float open = price;
                float change = (float)(rng.NextDouble() * 4 - 2);
                float close = open + change;
                float high = Math.Max(open, close) + (float)(rng.NextDouble() * 1.5);
                float low = Math.Min(open, close) - (float)(rng.NextDouble() * 1.5);
                bars.Add(new Bar(open, high, low, close, startTime.AddMinutes(i)));
                price = close;
            }

            return bars;
        }

        private void SkCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;
            canvas.Clear(new SKColor(0x18, 0x18, 0x18));

            if (_bars.Count == 0) return;

            int startIdx = Math.Max(0, (int)Math.Floor(_startIndex));
            int visibleCount = (int)Math.Ceiling(_visibleBarCount) + 1;
            int endIdx = Math.Min(_bars.Count, startIdx + visibleCount);
            if (endIdx <= startIdx) return;

            float chartWidth = info.Width;
            float chartHeight = info.Height;

            float plotLeft = LeftMargin;
            float plotRight = chartWidth - (float)ViewSettings.PriceAxisWidth;
            float plotRightWithPadding = plotRight - (float)ViewSettings.RightPaddingPixels;
            float plotTop = TopMargin;
            float plotBottom = chartHeight - BottomAxisHeight;
            float plotWidth = Math.Max(1, plotRightWithPadding - plotLeft);
            float plotHeight = Math.Max(1, plotBottom - plotTop);

            _lastPlotWidth = plotWidth;
            _plotRight = plotRight;
            _plotBottom = plotBottom;
            _plotRightFraction = plotRight / chartWidth;
            _plotBottomFraction = plotBottom / chartHeight;

            float rawMinPrice = float.MaxValue;
            float rawMaxPrice = float.MinValue;
            for (int i = startIdx; i < endIdx; i++)
            {
                rawMinPrice = Math.Min(rawMinPrice, _bars[i].Low);
                rawMaxPrice = Math.Max(rawMaxPrice, _bars[i].High);
            }

            float rawRange = rawMaxPrice - rawMinPrice;
            if (rawRange <= 0) rawRange = 1;

            float center = (rawMinPrice + rawMaxPrice) / 2f + _verticalPanOffset;
            float displayedRange = (float)(rawRange / _verticalZoomFactor);
            float minPrice = center - displayedRange / 2f;
            float maxPrice = center + displayedRange / 2f;
            float priceRange = displayedRange;

            _lastPricePerPixel = priceRange / plotHeight;

            float barSlotWidth = plotWidth / (float)_visibleBarCount;
            float bodyWidth = barSlotWidth * 0.6f;

            float PriceToY(float price)
            {
                float t = (price - minPrice) / priceRange;
                return plotBottom - t * plotHeight;
            }

            using var bullPaint = new SKPaint { Color = new SKColor(0x3D, 0xA3, 0x5D), IsAntialias = true };
            using var bearPaint = new SKPaint { Color = new SKColor(0xC9, 0x4F, 0x4F), IsAntialias = true };
            using var wickPaint = new SKPaint { Color = new SKColor(0xA0, 0xA0, 0xA0), StrokeWidth = 1.5f, IsAntialias = true };
            using var gridPaint = new SKPaint { Color = new SKColor(0x2A, 0x2A, 0x2A), StrokeWidth = 1f, IsAntialias = false };
            using var axisBgPaint = new SKPaint { Color = new SKColor(0x20, 0x20, 0x20) };
            using var axisTextPaint = new SKPaint { Color = new SKColor(0xA0, 0xA0, 0xA0), IsAntialias = true };
            using var axisFont = new SKFont { Size = 11f };

            

            canvas.DrawRect(new SKRect(plotRight, 0, chartWidth, chartHeight), axisBgPaint);

            double niceStep = CalculateNiceStep(priceRange, 6);
            double firstTick = Math.Ceiling(minPrice / niceStep) * niceStep;
            for (double price = firstTick; price <= maxPrice; price += niceStep)
            {
                float y = PriceToY((float)price);
                canvas.DrawLine(plotLeft, y, plotRight, y, gridPaint);
                canvas.DrawText(price.ToString("0.##"), plotRight + 6, y + 4, axisFont, axisTextPaint);
            }

            double visibleMinutes = (_bars[endIdx - 1].Time - _bars[startIdx].Time).TotalMinutes;
            if (visibleMinutes <= 0) visibleMinutes = 1;
            double targetLabelCount = Math.Max(1, plotWidth / 80.0);
            int niceStepMinutes = CalculateNiceTimeStepMinutes(visibleMinutes / targetLabelCount);

            for (int i = startIdx; i < endIdx; i++)
            {
                if (_bars[i].Time.Minute % niceStepMinutes != 0) continue;

                double slot = i - _startIndex;
                float centerX = (float)(plotLeft + barSlotWidth * slot + barSlotWidth / 2.0);
                if (centerX < plotLeft || centerX > plotRightWithPadding) continue;

                canvas.DrawLine(centerX, plotTop, centerX, plotBottom, gridPaint);
            }

            for (int i = startIdx; i < endIdx; i++)
            {
                var bar = _bars[i];
                double slot = i - _startIndex;
                float centerX = (float)(plotLeft + barSlotWidth * slot + barSlotWidth / 2.0);

                if (i == _bars.Count - 1)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[Newest Candle] i={i} S={_startIndex:0.###} " +
                        $"V={_visibleBarCount:0.###} " +
                        $"slot={slot:0.###} " +
                        $"centerX={centerX:0.###} " +
                        $"plotLeft={plotLeft:0.###} " +
                        $"plotRight={plotRightWithPadding:0.###}");
                }

                if (centerX < plotLeft || centerX > plotRightWithPadding) continue;

                float highY = PriceToY(bar.High);
                float lowY = PriceToY(bar.Low);
                canvas.DrawLine(centerX, highY, centerX, lowY, wickPaint);

                bool isBull = bar.Close >= bar.Open;
                var bodyPaint = isBull ? bullPaint : bearPaint;

                float openY = PriceToY(bar.Open);
                float closeY = PriceToY(bar.Close);
                float top = Math.Min(openY, closeY);
                float bottom = Math.Max(openY, closeY);
                if (bottom - top < 1f) bottom = top + 1f;

                var rect = new SKRect(centerX - bodyWidth / 2f, top, centerX + bodyWidth / 2f, bottom);
                canvas.DrawRect(rect, bodyPaint);
            }

            canvas.DrawRect(new SKRect(0, plotBottom, chartWidth, chartHeight), axisBgPaint);

            for (int i = startIdx; i < endIdx; i++)
            {
                if (_bars[i].Time.Minute % niceStepMinutes != 0) continue;

                double slot = i - _startIndex;
                float centerX = (float)(plotLeft + barSlotWidth * slot + barSlotWidth / 2.0);
                if (centerX < plotLeft || centerX > plotRightWithPadding) continue;

                string label = _bars[i].Time.ToString("HH:mm");
                canvas.DrawText(label, centerX - 15, plotBottom + 16, axisFont, axisTextPaint);
            }
        }

        private static double CalculateNiceStep(double range, int targetTicks)
        {
            if (range <= 0) return 1;
            double rawStep = range / targetTicks;
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
            double normalized = rawStep / magnitude;
            double niceNormalized = normalized < 1.5 ? 1 : normalized < 3 ? 2 : normalized < 7 ? 5 : 10;
            return niceNormalized * magnitude;
        }

        private static int CalculateNiceTimeStepMinutes(double rawMinutesPerLabel)
        {
            int[] niceSteps = { 1, 2, 5, 10, 15, 30, 60, 120, 240 };
            foreach (int step in niceSteps)
            {
                if (rawMinutesPerLabel <= step) return step;
            }
            return 240;
        }

        private double GetMaxStartIndex()
        {
            if (ViewSettings.LockFillSpace)
            {
                return Math.Max(0, _bars.Count - _visibleBarCount + _lockedFillSpaceBars);
            }

            return Math.Max(0, _bars.Count - 1);
        }

        private void ClampStartIndex()
        {
            _startIndex = Clamp(_startIndex, 0, GetMaxStartIndex());
            LogState("ClampStartIndex");
        }

        private void UpdateScrollBar()
        {
            if (HistoryScrollBar == null) return;

            _suppressScrollBarEvent = true;
            HistoryScrollBar.Maximum = GetMaxStartIndex();
            HistoryScrollBar.ViewportSize = _visibleBarCount;
            HistoryScrollBar.Value = Math.Min(_startIndex, HistoryScrollBar.Maximum);
            _suppressScrollBarEvent = false;
            LogState("UpdateScrollBar");
        }

        private bool _suppressScrollBarEvent;

        private void HistoryScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_suppressScrollBarEvent) return;
            _startIndex = e.NewValue;
            ClampStartIndex();
            SkCanvas.InvalidateVisual();
        }

        // ZOOM TEMPORARILY DISABLED - step 3 will rebuild this
        private void SkCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void SkCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(SkCanvas);
            _lastMousePos = pos;

            if (pos.X >= _plotRightFraction * SkCanvas.ActualWidth)
            {
                _isDraggingPriceAxis = true;
                _axisDragStartPos = pos;
                _axisDragStartVerticalZoom = _verticalZoomFactor;
                SkCanvas.CaptureMouse();
                return;
            }

            if (pos.Y >= _plotBottomFraction * SkCanvas.ActualHeight)
            {
                _isDraggingTimeAxis = true;
                _axisDragStartPos = pos;
                _axisDragStartVisibleBarCount = _visibleBarCount;
                _axisDragStartRightEdge = _startIndex + _visibleBarCount;
                SkCanvas.CaptureMouse();
                return;
            }

            if (ViewSettings.LockPanning) return;
            _isDragging = true;
            SkCanvas.CaptureMouse();
        }

        private void SkCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(SkCanvas);

            if (_isDraggingPriceAxis)
            {
                double totalDeltaY = pos.Y - _axisDragStartPos.Y;
                double zoomChange = 1 - totalDeltaY * ViewSettings.AxisDragSensitivity;
                zoomChange = Math.Max(0.05, zoomChange);
                _verticalZoomFactor = Clamp(_axisDragStartVerticalZoom * zoomChange, 0.1, 10.0);
                SkCanvas.InvalidateVisual();
                return;
            }

            if (_isDraggingTimeAxis)
            {
                double totalDeltaX = pos.X - _axisDragStartPos.X;
                double zoomChange = 1 - totalDeltaX * ViewSettings.AxisDragSensitivity;
                zoomChange = Math.Max(0.05, zoomChange);

                _visibleBarCount = Clamp(_axisDragStartVisibleBarCount / zoomChange, 10, _bars.Count);
                _startIndex = _axisDragStartRightEdge - _visibleBarCount;

                ClampStartIndex();
                UpdateScrollBar();
                SkCanvas.InvalidateVisual();
                return;
            }

            if (!_isDragging || ViewSettings.LockPanning) return;

            double deltaXPan = pos.X - _lastMousePos.X;
            double deltaYPan = pos.Y - _lastMousePos.Y;
            _lastMousePos = pos;

            double barsPerPixel = _visibleBarCount / _lastPlotWidth;
            _startIndex -= deltaXPan * barsPerPixel;

            System.Diagnostics.Debug.WriteLine(
                $"[Pan BEFORE CLAMP] deltaX={deltaXPan:0.###} " +
                $"barsPerPixel={barsPerPixel:0.######} " +
                $"S={_startIndex:0.###} " +
                $"V={_visibleBarCount:0.###} " +
                $"Max={GetMaxStartIndex():0.###}");

            ClampStartIndex();

            System.Diagnostics.Debug.WriteLine(
                $"[Pan AFTER CLAMP] S={_startIndex:0.###} " +
                $"Max={GetMaxStartIndex():0.###}");

            _verticalPanOffset += (float)(deltaYPan * _lastPricePerPixel);

            UpdateScrollBar();
            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _isDraggingPriceAxis = false;
            _isDraggingTimeAxis = false;
            SkCanvas.ReleaseMouseCapture();
        }

        private void SkCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            _isDraggingPriceAxis = false;
            _isDraggingTimeAxis = false;
            SkCanvas.ReleaseMouseCapture();
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private readonly struct Bar
        {
            public readonly float Open, High, Low, Close;
            public readonly DateTime Time;
            public Bar(float open, float high, float low, float close, DateTime time)
            {
                Open = open; High = high; Low = low; Close = close; Time = time;
            }
        }
    }
}